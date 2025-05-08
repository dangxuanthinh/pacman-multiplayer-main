using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CustomGrid<GridObject>
{
    [ShowInInspector]
    public int width;
    [ShowInInspector]
    public int height;
    [ShowInInspector]
    public Vector3 originPosition;

    public float cellSize;
    public float cellGap;

    [ShowInInspector, TableMatrix(DrawElementMethod = "DrawElement", SquareCells = true)]
    public GridObject[,] gridArray;

    public UnityAction<int, int> OnGridValueChanged;

    public int Width => width;
    public int Height => height;

#if UNITY_EDITOR
    public static Node DrawElement(Rect rect, Node value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            GUI.changed = true;
            Event.current.Use();
        }
        Color color = Color.white;
        if (value.teleportDesination != null)
            color = Color.grey;
        else if (value.ghostHouseOwner == GhostName.Blinky || value.scatterDestinationOwner == GhostName.Blinky)
            color = Color.red;
        else if (value.ghostHouseOwner == GhostName.Inky || value.scatterDestinationOwner == GhostName.Inky)
            color = Color.cyan;
        else if (value.ghostHouseOwner == GhostName.Pinky || value.scatterDestinationOwner == GhostName.Pinky)
            color = new Color(1, 0.05f, 0.6f);
        else if (value.ghostHouseOwner == GhostName.Clyde || value.scatterDestinationOwner == GhostName.Clyde)
            color = new Color(1f, 0.5f, 0f);
        else if (value.hasPowerUpPellet)
            color = Color.yellow;
        else if (value.isGhostHouse == true)
            color = Color.blue;
        else if (value.passable && value.isGhostHouse == false)
            color = Color.white;
        else if (value.passable == false)
            color = Color.black;
        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), color);
        return value;
    }
#endif

    public CustomGrid(int width, int height, Vector3 originPosition, float cellSize, float cellGap, Func<CustomGrid<GridObject>, int, int, GridObject> gridObject)
    {
        this.width = width;
        this.height = height;
        this.originPosition = originPosition;
        gridArray = new GridObject[width, height];
        this.cellSize = cellSize;
        this.cellGap = cellGap;

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = gridObject(this, x, y);
                //Debug.Log(gridArray[x, y]);
            }
        }
    }

    public void SetGrid(GridObject[,] gridArray)
    {
        this.gridArray = gridArray;
    }

    public void SetCellValue(int x, int y, GridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            InvokeOnGridValueChanged(x, y);
        }
        else
        {
            Debug.Log($"Invalid x = {x}, y = {y} for grid of width = {width}, height = {height}");
        }
    }

    public void InvokeOnGridValueChanged(int x, int y)
    {
        OnGridValueChanged?.Invoke(x, y);
    }

    public void SetCellValue(Vector3 worldPosition, GridObject value)
    {

    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public Vector3 GetWorldPosition(GridObject gridObject)
    {
        Vector2Int gridPosition = GetGridPosition(gridObject);
        return GetWorldPosition(gridPosition.x, gridPosition.y);
    }

    public List<GridObject> GetNeighbours(GridObject gridObject)
    {
        List<GridObject> neighbours = new List<GridObject>();
        Vector2Int gridPosition = GetGridPosition(gridObject);
        neighbours.Add(GetCellValue(gridPosition.x, gridPosition.y + 1));
        neighbours.Add(GetCellValue(gridPosition.x, gridPosition.y - 1));
        neighbours.Add(GetCellValue(gridPosition.x + 1, gridPosition.y));
        neighbours.Add(GetCellValue(gridPosition.x - 1, gridPosition.y));

        List<GridObject> validNeighbours = new List<GridObject>();
        foreach (GridObject neighbour in neighbours)
        {
            if (neighbour != null)
            {
                validNeighbours.Add(neighbour);
            }
        }
        return validNeighbours;
    }

    public int GetGridDistance(GridObject start, GridObject end)
    {
        Vector2Int startCoordinate = GetGridPosition(start);
        Vector2Int endCoordinate = GetGridPosition(end);
        return GetGridDistance(startCoordinate.x, startCoordinate.y, endCoordinate.x, endCoordinate.y);
    }

    public int GetGridDistance(int x1, int y1, int x2, int y2)
    {
        int distanceX = Mathf.Abs(x2 - x1);
        int distanceY = Mathf.Abs(y2 - y1);
        return distanceX + distanceY;
    }

    public Vector2Int GetGridPosition(GridObject gridObject)
    {
        if (gridObject == null)
        {
            Debug.Log("Can't find grid position of a null gridObject");
            return Vector2Int.zero;
        }
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (gridArray[x, y].Equals(gridObject))
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }

    public float GetWorldDistance(GridObject start, GridObject end)
    {
        return Vector3.Distance(GetWorldPosition(start), GetWorldPosition(end));
    }

    public float GetGridEuclideanDistance(GridObject start, GridObject end)
    {
        Vector2 startCoordinate = GetGridPosition(start);
        Vector2 endCoordinate = GetGridPosition(end);

        float deltaX = endCoordinate.x - startCoordinate.x;
        float deltaY = endCoordinate.y - startCoordinate.y;

        float distance = deltaX * deltaX + deltaY * deltaY;

        return distance;
    }

    public GridObject GetCellValue(Vector3 worldPosition)
    {
        Vector3 relativePosition = worldPosition - originPosition;
        float totalCellSize = cellSize + cellGap;

        int gridX = Mathf.RoundToInt(relativePosition.x / totalCellSize);
        int gridY = Mathf.RoundToInt(relativePosition.y / totalCellSize);

        float xRemainder = relativePosition.x % totalCellSize;
        float yRemainder = relativePosition.y % totalCellSize;

        if (xRemainder > cellSize || yRemainder > cellSize)
        {
            return default;
        }

        return GetCellValue(gridX, gridY);
    }

    public GridObject GetCellValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
            return default;
    }

    public GridObject GetCellValueClamped(int x, int y)
    {
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);
        return gridArray[x, y];
    }
}

