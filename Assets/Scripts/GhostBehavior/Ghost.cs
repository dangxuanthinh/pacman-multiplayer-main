using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UIElements;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}


public abstract class Ghost : NetworkBehaviour
{
    public float movementSpeed;
    public Node startNode;
    public Node targetNode;
    public Node currentNode;
    public Node previousNode;
    public Node nextNode;

    public Node scatterTargetNode;

    private Node ghostHouseEntranceTarget;

    public MoveDirection currentMoveDirection;

    public NetworkVariable<GhostState> ghostState = new NetworkVariable<GhostState>();

    public GhostName ghostName;

    private bool inGhostHouse;

    private float normalMovementSpeed;
    private float eatenMovementSpeed;
    private float slowedMovementSpeed;
    private float ghostHouseMovementSpeed;

    public ParticleSystem deathEffect;
    public Color deathExplosionColor;

    protected Pacman targetPlayer;

    public NetworkVariable<bool> isTimeStopped = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);
    private NetworkVariable<Vector3> targetDestination = new NetworkVariable<Vector3>();
    private NetworkTransform networkTransform;

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        ghostHouseMovementSpeed = 3f;
        normalMovementSpeed = movementSpeed;
        eatenMovementSpeed = movementSpeed + 5;
        slowedMovementSpeed = movementSpeed - 3;
    }

    public void SetTargetPlayer(Pacman player)
    {
        this.targetPlayer = player;
    }

    public void InitializeNodes(Node scatterNode)
    {
        currentNode = GameManager.Instance.grid.GetCellValue(transform.position);
        startNode = currentNode;
        scatterTargetNode = scatterNode;
        targetNode = scatterNode;
        previousNode = null;
        nextNode = currentNode;
        ghostHouseEntranceTarget = Utils.GetRandomElement(GameManager.Instance.GetGhostHouseEntranceNodes());
    }

    public void SpawnDeathExplosion()
    {
        ParticleSystem particle = Instantiate(deathEffect, transform.position, Quaternion.identity);
        var main = particle.main;
        main.startColor = deathExplosionColor;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (ghostState.Value == GhostState.Eaten)
                movementSpeed = eatenMovementSpeed;
            else if (currentNode.isGhostHouse)
                movementSpeed = ghostHouseMovementSpeed;
            else if (ghostState.Value == GhostState.Frightened)
                movementSpeed = slowedMovementSpeed;
            else
                movementSpeed = normalMovementSpeed;

            currentNode = GameManager.Instance.grid.GetCellValue(transform.position);
            inGhostHouse = currentNode.isGhostHouse;
            Debug.DrawLine(transform.position, GameManager.Instance.grid.GetWorldPosition(targetNode), Color.red);
            Debug.DrawLine(transform.position, GameManager.Instance.grid.GetWorldPosition(nextNode), Color.blue);

            if (currentNode.teleportDesination != null && previousNode.teleportDesination == null)
            {
                networkTransform.Teleport(currentNode.teleportDesination.GetWorldPosition(), transform.rotation, transform.localScale);
                transform.position = currentNode.teleportDesination.GetWorldPosition();
                previousNode = currentNode;
                currentNode = currentNode.teleportDesination;
                nextNode = currentNode;
            }

            if (currentNode == startNode && ghostState.Value == GhostState.Eaten)
            {
                ghostState.Value = GhostState.Chase;
                nextNode = previousNode;
                previousNode = currentNode;
            }

            switch (ghostState.Value)
            {
                case GhostState.Scatter:
                    targetNode = scatterTargetNode;
                    break;
                case GhostState.Chase:
                    FindChaseTarget();
                    break;
                case GhostState.Frightened:
                    break;
                case GhostState.Eaten:
                    targetNode = startNode;
                    break;
                default:
                    break;
            }

            if (DistanceToNextNode() <= 0.01f && currentNode != previousNode)
            {
                if (ghostState.Value == GhostState.Frightened)
                {
                    currentMoveDirection = CalculateNextFrightenedDirection();
                }
                else
                {
                    currentMoveDirection = CalculateNextMoveDirection();
                }
            }
            targetDestination.Value = GameManager.Instance.grid.GetWorldPosition(nextNode);
        }

        if (isTimeStopped.Value == false)
        {
            float movementSpeedMultiplier = GameManager.Instance.difficulty.movementSpeedMultiplier;
            transform.position = Vector2.MoveTowards(transform.position, targetDestination.Value, movementSpeed * movementSpeedMultiplier * Time.deltaTime);
        }
    }

    protected abstract void FindChaseTarget();

    private MoveDirection GetOppositeMoveDirection(MoveDirection moveDirection)
    {
        switch (moveDirection)
        {
            case MoveDirection.Up: return MoveDirection.Down;
            case MoveDirection.Down: return MoveDirection.Up;
            case MoveDirection.Left: return MoveDirection.Right;
            case MoveDirection.Right: return MoveDirection.Left;
            default:
                return MoveDirection.Up;
        }
    }

    private float DistanceToNextNode()
    {
        return Vector2.Distance(transform.position, GameManager.Instance.grid.GetWorldPosition(nextNode));
    }

    List<Func<Node, Node, bool>> directionChecks = new List<Func<Node, Node, bool>>
    {
        (current, next) => next.IsUpOf(current),
        (current, next) => next.IsLeftOf(current),
        (current, next) => next.IsDownOf(current),
        (current, next) => next.IsRightOf(current)
    };

    private MoveDirection CalculateNextFrightenedDirection()
    {
        List<Node> possibleNextNodes = currentNode.GetPassableNeighbours();
        possibleNextNodes.Remove(previousNode); // Ghosts cannot turn 180 degrees

        // Frightened ghost must turn at a intersection if possible -> Remove the node the ghost is currently facing towards
        if (IsAtIntersection())
        {
            possibleNextNodes.Remove(GetNodeAheadBasedOnDirection(currentNode, currentMoveDirection));
        }
        if (possibleNextNodes.Count == 0)
        {
            possibleNextNodes = currentNode.GetPassableNeighbours();
        }
        List<Node> ghostHouseNodes = new List<Node>();
        foreach (Node node in possibleNextNodes)
        {
            Debug.DrawLine(transform.position, node.GetWorldPosition(), Color.green, 1f);
            if (node.isGhostHouse)
                ghostHouseNodes.Add(node);
        }
        foreach (Node node in ghostHouseNodes)
        {
            possibleNextNodes.Remove(node);
        }
        int randomIndex = UnityEngine.Random.Range(0, possibleNextNodes.Count);
        nextNode = possibleNextNodes[randomIndex];
        previousNode = currentNode;
        return GetMoveDirectionFromNode(currentNode, nextNode);
    }

    public void OnFrightenedStateEnter()
    {
        previousNode = GetNodeAheadBasedOnDirection(currentNode, GetOppositeMoveDirection(currentMoveDirection));
        if (previousNode.isGhostHouse || !previousNode.passable)
        {
            var neighbours = currentNode.GetPassableNeighbours();
            neighbours.Remove(previousNode);
            previousNode = Utils.GetRandomElement(neighbours);
        }
        nextNode = previousNode;
        previousNode = currentNode;
    }

    private bool IsAtIntersection()
    {
        var grid = GameManager.Instance.grid;
        Node rightNode = grid.GetCellValue(currentNode.x + 1, currentNode.y);
        Node leftNode = grid.GetCellValue(currentNode.x - 1, currentNode.y);
        Node upNode = grid.GetCellValue(currentNode.x, currentNode.y + 1);
        Node downNode = grid.GetCellValue(currentNode.x, currentNode.y - 1);
        switch (currentMoveDirection)
        {
            case MoveDirection.Up:
                return (rightNode.passable && !rightNode.isGhostHouse) || (leftNode.passable && !leftNode.isGhostHouse);
            case MoveDirection.Down:
                return (rightNode.passable && !rightNode.isGhostHouse) || (leftNode.passable && !leftNode.isGhostHouse);
            case MoveDirection.Left:
                return (upNode.passable && !upNode.isGhostHouse) || (downNode.passable && !downNode.isGhostHouse);
            case MoveDirection.Right:
                return (upNode.passable && !upNode.isGhostHouse) || (downNode.passable && !downNode.isGhostHouse);
            default: return false;
        }
    }

    private MoveDirection CalculateNextMoveDirection()
    {
        List<Node> possibleNextNodes = currentNode.GetPassableNeighbours();

        possibleNextNodes.Remove(previousNode); // Ghosts cannot turn 180 degrees

        if (!inGhostHouse && (ghostState.Value != GhostState.Eaten))
        {
            // Ignore ghost house nodes
            possibleNextNodes = possibleNextNodes.Where(node => !node.isGhostHouse).ToList();
        }

        Dictionary<Node, float> nodeDistanceTable = new Dictionary<Node, float>();
        List<float> distancesToTarget = new List<float>();
        if (currentNode.isGhostHouse && ghostState.Value != GhostState.Eaten)
        {
            targetNode = ghostHouseEntranceTarget;
        }
        for (int i = 0; i < possibleNextNodes.Count; i++)
        {
            // Get distance to target
            float distanceToTarget = possibleNextNodes[i].DistanceTo(targetNode);
            // Update table
            nodeDistanceTable[possibleNextNodes[i]] = possibleNextNodes[i].DistanceTo(targetNode);
            distancesToTarget.Add(possibleNextNodes[i].DistanceTo(targetNode));
        }
        // Sort distances by ascending to choose the node with the shortest distance
        distancesToTarget.Sort();

        // Get all neighbour nodes with that distance to the target
        List<Node> bestNodes = new List<Node>();
        foreach (KeyValuePair<Node, float> kvp in nodeDistanceTable)
        {
            if (kvp.Value == distancesToTarget[0])
            {
                bestNodes.Add(kvp.Key);
            }
        }
        if (bestNodes.Count > 0)
        {
            if (ghostState.Value == GhostState.Eaten && currentNode.isGhostHouse == false)
            {
                foreach (Node node in bestNodes)
                {
                    if (node.isGhostHouse)
                    {
                        nextNode = node;
                        previousNode = currentNode;
                        return GetMoveDirectionFromNode(currentNode, node);
                    }
                }
            }
            foreach (var check in directionChecks)
            {
                Node selectedNode = bestNodes.FirstOrDefault(node => check(currentNode, node));
                if (selectedNode != null)
                {
                    nextNode = selectedNode;
                    previousNode = currentNode;
                    return GetMoveDirectionFromNode(currentNode, selectedNode);
                }
            }
        }
        Debug.Log("Something went wrong when calculating next move direction");
        return MoveDirection.Right;
    }

    private Node GetNodeAheadBasedOnDirection(Node startNode, MoveDirection moveDirection)
    {
        var grid = GameManager.Instance.grid;
        switch (moveDirection)
        {
            case MoveDirection.Up: return grid.GetCellValueClamped(startNode.x, startNode.y + 1);
            case MoveDirection.Down: return grid.GetCellValueClamped(startNode.x, startNode.y - 1);
            case MoveDirection.Left: return grid.GetCellValueClamped(startNode.x - 1, startNode.y);
            case MoveDirection.Right: return grid.GetCellValueClamped(startNode.x + 1, startNode.y);
            default:
                return null;
        }
    }

    private MoveDirection GetMoveDirectionFromNode(Node startNode, Node endNode)
    {
        if (startNode.x == endNode.x && startNode.y < endNode.y)
            return MoveDirection.Up;
        else if (startNode.x == endNode.x && startNode.y > endNode.y)
            return MoveDirection.Down;
        else if (startNode.y == endNode.y && startNode.x > endNode.x)
            return MoveDirection.Left;
        else if (startNode.y == endNode.y && startNode.x < endNode.x)
            return MoveDirection.Right;
        else
        {
            Debug.LogWarning($"Invalid start node and end node to get move direction!\n Start node: {startNode} \n End node: {endNode}");
            return MoveDirection.Up;
        }
    }
}
