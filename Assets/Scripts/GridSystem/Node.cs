using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class Node : IEquatable<Node>
{
    [HideInInspector, NonSerialized] public CustomGrid<Node> grid;
    public int x;
    public int y;
    public bool passable;

    public bool isTeleportNode;
    [HideIf("@isTeleportNode == false")] public Vector2Int teleportDestinationCoordinate;
    [HideIf("@isTeleportNode == false"), NonSerialized] public Node teleportDesination;
    public bool hasPowerUpPellet;
    public bool isGhostHouse;
    public bool isGhostHouseEntrance;
    public GhostName ghostHouseOwner;
    public GhostName scatterDestinationOwner;

    public Node(int x, int y, CustomGrid<Node> grid)
    {
        ghostHouseOwner = GhostName.None;
        this.x = x;
        this.y = y;
        this.grid = grid;
        passable = true;
    }

    public Vector2Int GridPosition => new Vector2Int(x, y);

    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y);
    }

    public float DistanceTo(Node destination)
    {
        return grid.GetGridEuclideanDistance(this, destination);
    }

    public List<Node> GetNeighbours()
    {
        return grid.GetNeighbours(this);
    }

    public bool IsUpOf(Node otherNode)
    {
        return GridPosition + Vector2Int.down == grid.GetGridPosition(otherNode);
    }

    public bool IsDownOf(Node otherNode)
    {
        return GridPosition + Vector2Int.up == grid.GetGridPosition(otherNode);
    }

    public bool IsLeftOf(Node otherNode)
    {
        return GridPosition + Vector2Int.right == grid.GetGridPosition(otherNode);
    }

    public bool IsRightOf(Node otherNode)
    {
        return GridPosition + Vector2Int.left == grid.GetGridPosition(otherNode);
    }

    public List<Node> GetPassableNeighbours()
    {
        List<Node> neighbours = grid.GetNeighbours(this);
        List<Node> passableNeighbours = new List<Node>();
        foreach (Node neighbour in neighbours)
        {
            if (neighbour.passable)
            {
                passableNeighbours.Add(neighbour);
            }
        }
        return passableNeighbours;
    }


    public override string ToString()
    {
        return $"Node x: {x}, y: {y}, passable: {passable}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Node other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public bool Equals(Node other)
    {
        if (this.x == other.x && this.y == other.y)
        {
            return true;
        }
        return false;
    }
}
