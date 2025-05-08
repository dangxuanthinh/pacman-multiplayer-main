using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

public class Pacman : NetworkBehaviour
{
    public float movementSpeed;
    [HideInInspector] public float defaultMovementSpeed;
    public MoveDirection currentMoveDirection;
    [SerializeField] private float changeDirectionBufferDuration = 100f;
    private float changeDirectionTimer;
    private MoveDirection newMoveDirection;
    [HideInInspector] public Node currentNode;
    [HideInInspector] public Node nodeAhead;
    [HideInInspector] public Node previousNode;
    [HideInInspector] public Node lastFrameNode;

    private float distanceFromCurrentNode;

    private Touch touch;
    private Vector2 touchPosWhenChangingDirection;

    public Vector2 moveDirectionVector;
    private NetworkTransform networkTransform;

    [SerializeField] private ParticleSystem playerExplosion;

    public bool IsInvincible { get; private set; }
    public bool IsDeadTemporary { get; private set; }
    public bool HasEatenPowerup { get; private set; }

    public UnityAction OnNetworkObjectDespawn;
    public UnityAction OnPelletEaten;
    public UnityAction OnGhostEaten;
    public UnityAction<PowerUp> OnPowerupPickedUp;

    public static Pacman LocalInstance;

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        currentNode = GameManager.Instance.grid.GetCellValue(transform.position);
        defaultMovementSpeed = movementSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
    }

    public override void OnNetworkDespawn()
    {
        OnNetworkObjectDespawn?.Invoke();
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsOwner) return;

        changeDirectionTimer -= Time.deltaTime;

        distanceFromCurrentNode = Vector3.Distance(transform.position, GameManager.Instance.grid.GetWorldPosition(currentNode));

        SetCurrentAndLastFrameNode();
        CalculateNodeAhead();
        CalculateMoveDirectionVector();
        HandleInput();

        if (changeDirectionTimer > 0)
        {
            int targetX = currentNode.x;
            int targetY = currentNode.y;

            switch (newMoveDirection)
            {
                case MoveDirection.Up:
                    targetY += 1;
                    break;
                case MoveDirection.Down:
                    targetY -= 1;
                    break;
                case MoveDirection.Left:
                    targetX -= 1;
                    break;
                case MoveDirection.Right:
                    targetX += 1;
                    break;
            }

            Node targetNode = GameManager.Instance.grid.GetCellValue(targetX, targetY);

            if (targetNode.passable && !targetNode.isGhostHouse)
            {
                bool oppositeDirection = (newMoveDirection == MoveDirection.Up && currentMoveDirection == MoveDirection.Down) ||
                                         (newMoveDirection == MoveDirection.Down && currentMoveDirection == MoveDirection.Up) ||
                                         (newMoveDirection == MoveDirection.Left && currentMoveDirection == MoveDirection.Right) ||
                                         (newMoveDirection == MoveDirection.Right && currentMoveDirection == MoveDirection.Left);

                if (oppositeDirection || distanceFromCurrentNode <= 0.2f)
                {
                    nodeAhead = targetNode;
                    changeDirectionTimer = 0;
                    currentMoveDirection = newMoveDirection;
                    touchPosWhenChangingDirection = touch.position;
                }
            }
        }

        // If pacman is stepping on a teleport node
        if (currentNode.teleportDesination != null && previousNode.teleportDesination == null)
        {
            networkTransform.Teleport(currentNode.teleportDesination.GetWorldPosition(), transform.rotation, transform.localScale);
            transform.position = currentNode.teleportDesination.GetWorldPosition();
            currentNode = currentNode.teleportDesination;
            lastFrameNode = currentNode;
            previousNode = currentNode;
            return;
        }

        MovePacman();
    }

    public bool IsNearTeleportNode()
    {
        foreach (Node node in GameManager.Instance.GetTeleportNodes())
        {
            if (Vector3.Distance(transform.position, node.GetWorldPosition()) < 3f)
            {
                return true;
            }
        }
        return false;
    }

    private void CalculateMoveDirectionVector()
    {
        switch (currentMoveDirection)
        {
            case MoveDirection.Up:
                moveDirectionVector = Vector3.up;
                break;
            case MoveDirection.Down:
                moveDirectionVector = Vector3.down;
                break;
            case MoveDirection.Left:
                moveDirectionVector = Vector3.left;
                break;
            case MoveDirection.Right:
                moveDirectionVector = Vector3.right;
                break;
            default:
                break;
        }
    }

    private void CalculateNodeAhead()
    {
        switch (currentMoveDirection)
        {
            case MoveDirection.Up:
                nodeAhead = GameManager.Instance.grid.GetCellValue(currentNode.x, currentNode.y + 1);
                break;
            case MoveDirection.Down:
                nodeAhead = GameManager.Instance.grid.GetCellValue(currentNode.x, currentNode.y - 1);
                break;
            case MoveDirection.Left:
                nodeAhead = GameManager.Instance.grid.GetCellValue(currentNode.x - 1, currentNode.y);
                break;
            case MoveDirection.Right:
                nodeAhead = GameManager.Instance.grid.GetCellValue(currentNode.x + 1, currentNode.y);
                break;
            default:
                break;
        }
    }

    private void SetCurrentAndLastFrameNode()
    {
        if (GameManager.Instance.grid.GetCellValue(transform.position).passable)
        {
            currentNode = GameManager.Instance.grid.GetCellValue(transform.position);
            if (currentNode != lastFrameNode)
            {
                previousNode = lastFrameNode;
            }
            lastFrameNode = currentNode;
        }
    }

    private void MovePacman()
    {
        if ((nodeAhead == null || !nodeAhead.passable || nodeAhead.isGhostHouse) && distanceFromCurrentNode <= 0.1f)
        {
            return; // If the node ahead is invalid, stop
        }
        else
        {
            if (nodeAhead == null)
            {
                return;
            }
            // Move pacman towards the node ahead
            Vector3 desiredPosition = GameManager.Instance.grid.GetWorldPosition(nodeAhead.x, nodeAhead.y);
            if (!nodeAhead.passable)
            {
                desiredPosition = currentNode.GetWorldPosition();
            }
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition, movementSpeed * Time.deltaTime);
        }
    }

    private void HandleInput()
    {
        // Mobile input
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchPosWhenChangingDirection = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 swipeDelta = touch.position - touchPosWhenChangingDirection;

                if (swipeDelta.magnitude > 60f)
                {
                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    {
                        newMoveDirection = swipeDelta.x > 0 ? MoveDirection.Right : MoveDirection.Left;
                    }
                    else
                    {
                        newMoveDirection = swipeDelta.y > 0 ? MoveDirection.Up : MoveDirection.Down;
                    }
                    changeDirectionTimer = changeDirectionBufferDuration;
                }
            }
        }

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            newMoveDirection = MoveDirection.Up;
            changeDirectionTimer = changeDirectionBufferDuration;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            newMoveDirection = MoveDirection.Down;
            changeDirectionTimer = changeDirectionBufferDuration;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            newMoveDirection = MoveDirection.Left;
            changeDirectionTimer = changeDirectionBufferDuration;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            newMoveDirection = MoveDirection.Right;
            changeDirectionTimer = changeDirectionBufferDuration;
        }
    }

    public void OnPowerupEaten(PowerUp powerUp)
    {
        HasEatenPowerup = true;
        Debug.Log("Power up picked up");
        OnPowerupPickedUp?.Invoke(powerUp);
    }

    public void Respawn(Vector3 respawnPosition)
    {
        StartCoroutine(RespawnCoroutine(respawnPosition));
    }

    private IEnumerator RespawnCoroutine(Vector3 respawnPosition)
    {
        IsDeadTemporary = true;
        movementSpeed = 0f;
        yield return new WaitForSeconds(2f);
        IsDeadTemporary = false;
        movementSpeed = defaultMovementSpeed;
        transform.position = respawnPosition;
        networkTransform.Teleport(respawnPosition, transform.rotation, transform.localScale);
        StartCoroutine(SetInvincibleCoroutine());
    }

    private IEnumerator SetInvincibleCoroutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(3f);
        IsInvincible = false;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDeathExplosionRpc()
    {
        playerExplosion.Play();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || IsDeadTemporary) return;
        if (other.CompareTag("Ghost"))
        {
            Ghost ghost = other.GetComponent<Ghost>();
            if (ghost.ghostState.Value == GhostState.Frightened)
            {
                GameManager.Instance.EatGhost(ghost);
                OnGhostEaten?.Invoke();
            }
            else if (ghost.ghostState.Value != GhostState.Eaten)
            {
                if (IsInvincible == false && ghost.isTimeStopped.Value == false)
                {
                    GameManager.Instance.KillPacman();
                }
            }
        }
        if (other.CompareTag("Pellet"))
        {
            other.gameObject.SetActive(false);
            GameManager.Instance.EatNormalPellet(other.gameObject);
            OnPelletEaten?.Invoke();
        }
        if (other.CompareTag("PowerUpPellet"))
        {
            other.gameObject.SetActive(false);
            GameManager.Instance.EatPowerupPellet(other.gameObject);
            OnPelletEaten?.Invoke();
        }
        if (other.CompareTag("PelletRespawner"))
        {
            other.gameObject.SetActive(false);
            GameManager.Instance.EatPelletRespawner(other.gameObject);
        }
    }
}
