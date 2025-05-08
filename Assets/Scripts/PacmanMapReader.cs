using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanMapReader : MonoBehaviour
{
    [SerializeField] private Transform worldNodesHolder;
    [SerializeField] private NodeInfo nodeInfoPrefab;
    [SerializeField] private PacmanMap map;

    [Header("Debug")]
    [SerializeField] private Material red;
    [SerializeField] private Material pink;
    [SerializeField] private Material cyan;
    [SerializeField] private Material orange;
    [SerializeField] private Material grey;
    [SerializeField] private Material yellow;
    [SerializeField] private Material white;
    [SerializeField] private Material brown;

    [Button]
    public void SpawnWorldNodesFromMap()
    {
        List<Node> teleportNodes = new List<Node>();
        List<NodeInfo> spawnedNodeInfoList = new List<NodeInfo>();
        Transform spawnParent = new GameObject("Nodes").transform;
        spawnParent.position = new Vector3(-0.5f, -0.5f, 0f);
        for (int i = 0; i < map.flattenedNodeArray.Length; i++)
        {
            Node node = map.flattenedNodeArray[i];
            NodeInfo nodeInfo = Instantiate(nodeInfoPrefab, spawnParent);
            spawnedNodeInfoList.Add(nodeInfo);

            nodeInfo.transform.localPosition = new Vector3(node.x + 0.5f, node.y + 0.5f, 0);

            nodeInfo.node = node;
            nodeInfo.passable = node.passable;
            nodeInfo.isGhostHouse = node.isGhostHouse;
            nodeInfo.hasPowerUpPellet = node.hasPowerUpPellet;
            nodeInfo.isGhostHouseEntrance = node.isGhostHouseEntrance;
            nodeInfo.ghostHouseOwner = node.ghostHouseOwner;
            nodeInfo.scatterDestinationOwner = node.scatterDestinationOwner;

            MeshRenderer meshRenderer = nodeInfo.GetComponent<MeshRenderer>();
            Material material = white;
            if (nodeInfo.teleportDesination != null)
                material = grey;
            else if (nodeInfo.ghostHouseOwner == GhostName.Blinky || nodeInfo.scatterDestinationOwner == GhostName.Blinky)
                material = red;
            else if (nodeInfo.ghostHouseOwner == GhostName.Inky || nodeInfo.scatterDestinationOwner == GhostName.Inky)
                material = cyan;
            else if (nodeInfo.ghostHouseOwner == GhostName.Pinky || nodeInfo.scatterDestinationOwner == GhostName.Pinky)
                material = pink;
            else if (nodeInfo.ghostHouseOwner == GhostName.Clyde || nodeInfo.scatterDestinationOwner == GhostName.Clyde)
                material = orange;
            else if (nodeInfo.hasPowerUpPellet)
                material = yellow;
            else if (nodeInfo.passable == false)
                material = brown;
            meshRenderer.material = material;

            if (node.teleportDesination != null)
            {
                teleportNodes.Add(node);
            }
        }
        foreach (Node node in teleportNodes)
        {
            Node teleportDestinationNode = node.teleportDesination;
            NodeInfo nodeInfo = spawnedNodeInfoList.Find(n => n.node == node);
            nodeInfo.teleportDesination = spawnedNodeInfoList.Find(n => n.node == teleportDestinationNode).transform;
        }
    }


    [Button]
    public void ReadMap()
    {
        int maxX = 0;
        int maxY = 0;
        for (int i = 0; i < worldNodesHolder.childCount; i++)
        {
            Transform nodeGameObject = worldNodesHolder.GetChild(i);
            if (nodeGameObject.localPosition.x > maxX) maxX = (int)nodeGameObject.localPosition.x;
            if (nodeGameObject.localPosition.y > maxY) maxY = (int)nodeGameObject.localPosition.y;
        }
        int width = maxX + 1;
        int height = maxY + 1;
        int mapSize = width * height;
        map.width = width;
        map.height = height;
        map.originPosition = Vector3.zero;
        map.cellSize = 1;
        map.cellGap = 0;
        map.flattenedNodeArray = new Node[mapSize];
        map.grid = new CustomGrid<Node>(width, height, Vector3.zero, 1, 0, (CustomGrid<Node> grid, int x, int y) =>
        {
            return new Node(x, y, grid);
        });
        List<NodeInfo> teleportNodes = new List<NodeInfo>();
        Node[] flattenedNodeArray = new Node[mapSize];
        for (int i = 0; i < worldNodesHolder.childCount; i++)
        {
            Transform nodeGameObject = worldNodesHolder.GetChild(i);
            NodeInfo nodeInfo = nodeGameObject.GetComponent<NodeInfo>();
            Vector2 nodeCoordinate = (Vector2)nodeGameObject.transform.localPosition - Vector2.one * 0.5f;
            int x = (int)nodeCoordinate.x;
            int y = (int)nodeCoordinate.y;
            Node node = map.grid.GetCellValue(x, y);

            nodeInfo.node = node;
            node.passable = nodeInfo.passable;
            node.isGhostHouse = nodeInfo.isGhostHouse;
            node.hasPowerUpPellet = nodeInfo.hasPowerUpPellet;
            node.ghostHouseOwner = nodeInfo.ghostHouseOwner;
            node.isGhostHouseEntrance = nodeInfo.isGhostHouseEntrance;
            node.scatterDestinationOwner = nodeInfo.scatterDestinationOwner;

            if (nodeInfo.teleportDesination != null)
            {
                teleportNodes.Add(nodeInfo);
            }
            flattenedNodeArray[x + y * width] = node;
        }

        foreach (NodeInfo nodeInfo in teleportNodes)
        {
            nodeInfo.node.isTeleportNode = true;
            nodeInfo.node.teleportDesination = teleportNodes.Find(n => n.transform == nodeInfo.teleportDesination).node;
            nodeInfo.node.teleportDestinationCoordinate = new Vector2Int(nodeInfo.node.teleportDesination.x, nodeInfo.node.teleportDesination.y);
        }

        map.flattenedNodeArray = flattenedNodeArray;

#if UNITY_EDITOR
        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssetIfDirty(map);
#endif
    }
}
