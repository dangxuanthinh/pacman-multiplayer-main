using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Pacman Map"), System.Serializable]
public class PacmanMap : ScriptableObject
{
    public Sprite previewSprite;
    public string mapName;
    //public GameObject tilemapPrefab;
    public CustomGrid<Node> grid;
    public Node[] flattenedNodeArray;
    public int width;
    public int height;
    public Vector3 originPosition;
    public float cellSize;
    public float cellGap;
    [ValidateInput("CheckPlayerCoordinate", "Player start position is invalid")]
    public Vector2Int playerStartCoordinate;
    [ValidateInput("CheckPlayerCoordinate", "Respawn pellet position is invalid")]
    public List<Vector2Int> respawnPelletCoordinates = new List<Vector2Int>();

    public Color colorTheme;
    [SerializeField] private TextAsset json;

    private bool CheckPlayerCoordinate()
    {
        foreach (var node in flattenedNodeArray)
        {
            if (node.x == playerStartCoordinate.x && node.y == playerStartCoordinate.y)
            {
                if (node.passable && !node.isGhostHouse && node.teleportDesination == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckRespawnPelletCoordinate()
    {
        foreach (var node in flattenedNodeArray)
        {
            Vector2Int nodeCoordinate = new Vector2Int(node.x, node.y);
            foreach (Vector2Int coordinate in respawnPelletCoordinates)
            {
                if (coordinate != nodeCoordinate) continue;
                if (node.passable == false || node.teleportDesination != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    [OnInspectorInit]
    private void CreateGrid()
    {
        grid = new CustomGrid<Node>(width, height, Vector3.zero, 1, 0, (CustomGrid<Node> grid, int x, int y) =>
        {
            return new Node(x, y, grid);
        });

        Node[,] gridArray = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridArray[x, y] = flattenedNodeArray[x + y * width];
                gridArray[x, y].grid = grid;
            }
        }
        grid.SetGrid(gridArray);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid.GetCellValue(x, y);
                if (node.isTeleportNode)
                {
                    node.teleportDesination = grid.GetCellValue(node.teleportDestinationCoordinate.x, node.teleportDestinationCoordinate.y);
                }
            }
        }
    }

    public CustomGrid<Node> GetGrid()
    {
        CreateGrid();
        return grid;
    }

    public Node GetGhostScatterDestination(GhostName ghostName)
    {
        if (ghostName == GhostName.None)
        {
            Debug.LogError("Invalid ghost name to get scatter destination");
            return null;
        }
        else
        {
            return flattenedNodeArray.FirstOrDefault(n => n.scatterDestinationOwner == ghostName);
        }
    }

    public Node GetGhostStartingNode(GhostName ghostName)
    {
        if (ghostName == GhostName.None)
        {
            Debug.LogError("Invalid ghost name to get starting position");
            return null;
        }
        else
        {
            return flattenedNodeArray.FirstOrDefault(n => n.ghostHouseOwner == ghostName);
        }
    }

#if UNITY_EDITOR
    [Button]
    public void ExportMapToJson()
    {
        string json = JsonConvert.SerializeObject(flattenedNodeArray);
        using (StreamWriter writer = File.AppendText($"Assets/{this.name}.json"))
        {
            writer.WriteLine(json);
        }
    }

    [Button]
    public void ReadMapFromJson()
    {
        this.flattenedNodeArray = JsonConvert.DeserializeObject<List<Node>>(json.ToString()).ToArray();
        originPosition = Vector3.zero;
        cellSize = 1;
        cellGap = 0;
        for (int i = 0; i < flattenedNodeArray.Length; i++)
        {
            Node node = flattenedNodeArray[i];
            if (node.x + 1 >= width)
            {
                width = node.x + 1;
            }
            if (node.y + 1 >= height)
            {
                height = node.y + 1;
            }

            if (node.isTeleportNode)
            {
                node.teleportDesination = flattenedNodeArray.ToList().Find(n => n.x == node.teleportDestinationCoordinate.x && n.y == node.teleportDestinationCoordinate.y);
            }
        }
    }
#endif
}
