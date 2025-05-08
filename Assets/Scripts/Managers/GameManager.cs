using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);
    public NetworkVariable<float> survivalRemainingTime = new NetworkVariable<float>();

    private Pacman serverPacman;
    public int TotalLives = 3;

    public Dictionary<ulong, int> playerRemainingLivesTable = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> playerIndexTable = new Dictionary<ulong, int>();
    public List<Pacman> spawnedPlayers = new List<Pacman>();
    private List<Vector3> playerStartingPositions = new List<Vector3>();

    [HideInInspector] public bool gameOver;

    [Header("Character references")]
    [HideInInspector] public Ghost Inky;
    [HideInInspector] public Ghost Blinky;
    [HideInInspector] public Ghost Pinky;
    [HideInInspector] public Ghost Clyde;
    [HideInInspector] public Ghost[] ghosts = new Ghost[4];
    private bool isAnyGhostFrightened;

    [Header("Prefabs")]
    [SerializeField] private Ghost blinkyPrefab;
    [SerializeField] private Ghost inkyPrefab;
    [SerializeField] private Ghost pinkyPrefab;
    [SerializeField] private Ghost clydePrefab;
    [SerializeField] private Pellet pelletPrefab;
    [SerializeField] private Pellet pelletPowerUpPrefab;

    private List<Node> ghostHouseEntranceNodes = new List<Node>();
    private List<Node> teleportNodes = new List<Node>();

    [HideInInspector] public CustomGrid<Node> grid;
    public PacmanMap map;
    public GhostDifficulty difficulty;
    public GameMode CurrentGameMode;
    private int currentStateIndex;
    private GhostState currentGhostState;
    private float stateDurationElapsed;
    [SerializeField] private float frightenedStateDuration = 6f;
    private float frightenedStateElapsed;

    private List<Pellet> pellets = new List<Pellet>();
    private List<Pellet> eatenPellets = new List<Pellet>();

    public UnityAction OnMapSet;
    public UnityAction<Ghost> OnGhostEaten;
    public UnityAction OnGhostEatenByLocalPlayer;
    public UnityAction OnPacmanLivesChanged;
    public UnityAction OnGameVictory;
    public UnityAction OnGameLose;
    public UnityAction OnGameStart;
    public UnityAction OnCountdownStart;
    public UnityAction OnNormalPelletEaten;
    public UnityAction<Vector3> OnPowerupPelletEaten;
    public UnityAction OnPelletsRespawned;
    public UnityAction OnPlayerDead;

    public NetworkVariable<float> CountDownTimer = new NetworkVariable<float>();
    private const float COUNT_DOWN_DURATION = 4f;

    public bool IsWin() => pellets.Count == 0;
    public List<Node> GetGhostHouseEntranceNodes() => ghostHouseEntranceNodes;
    public List<Node> GetTeleportNodes() => teleportNodes;

    public bool IsLocalClientDead;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Ghost ghost in ghosts)
        {
            if (ghost)
                ghost.ghostState.OnValueChanged -= OnGhostStateChanged;
        }
    }

    private void Update()
    {
        if (gameStarted.Value == false) return;
        if (!IsServer) return;
        HandleGhostState();
        HandleSurvivalModeRemainingTime();
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SetMapClientRpc(PacmanLobby.Instance.GetMapName(), PacmanLobby.Instance.GetGhostDifficultyName(), PacmanLobby.Instance.GetGameMode());
        PacmanLobby.Instance.DeleteLobby();
        gameStarted.Value = true;
        currentStateIndex = 0;
        currentGhostState = difficulty.ghostStateDurations[currentStateIndex].ghostState;
        survivalRemainingTime.Value = 240f;
        StartCoroutine(CountDown());
        SpawnPlayers();
        UpdateRemainingLivesTableForClient();
        SpawnPellets(playerStartingPositions);
        SpawnGhosts();
        ChangeGhostState(currentGhostState);
    }

    private void SpawnPlayers()
    {
        Vector3 spawnPosition = map.grid.GetWorldPosition(map.playerStartCoordinate.x, map.playerStartCoordinate.y);
        int i = 0;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Pacman clientSelectedModel = PacmanMultiplayer.Instance.GetPlayerSelectedModel(clientID);
            GameObject spawnedPlayer = Instantiate(clientSelectedModel, spawnPosition, Quaternion.identity).gameObject;
            Pacman spawnedPlayerPacman = spawnedPlayer.GetComponent<Pacman>();
            spawnedPlayerPacman.NetworkObject.SpawnAsPlayerObject(clientID, true);
            if (spawnedPlayerPacman.NetworkObject.IsOwnedByServer)
            {
                serverPacman = spawnedPlayerPacman;
            }
            spawnedPlayers.Add(spawnedPlayerPacman);
            playerRemainingLivesTable[clientID] = TotalLives;
            playerIndexTable[clientID] = i;
            playerStartingPositions.Add(spawnPosition);
            spawnPosition += Vector3.right;
            i++;
        }
        NetworkObjectReference[] playerNetworkObjects = new NetworkObjectReference[i];
        for (int j = 0; j < playerNetworkObjects.Length; j++)
        {
            playerNetworkObjects[j] = spawnedPlayers[j].NetworkObject;
        }
        SetSpawnPlayersClientRpc(playerNetworkObjects);
    }

    [ClientRpc]
    private void SetSpawnPlayersClientRpc(NetworkObjectReference[] playerObjectReferences)
    {
        spawnedPlayers.Clear();
        foreach (NetworkObjectReference playerObjectReference in playerObjectReferences)
        {
            playerObjectReference.TryGet(out var playerNetworkObject);
            Pacman player = playerNetworkObject.GetComponent<Pacman>();
            spawnedPlayers.Add(player);
            if (player.IsOwner)
            {
                CameraHandler.Instance.SetFollowTarget(player);
            }
        }
    }

    [ClientRpc]
    private void SetMapClientRpc(string mapName, string ghostDiffitultyName, GameMode gameMode)
    {
        PacmanMap map = GameResourceHandler.Instance.GetMap(mapName);
        GhostDifficulty ghostDifficulty = GameResourceHandler.Instance.GetGhostDifficulty(ghostDiffitultyName);
        CurrentGameMode = gameMode;
        SetMap(map, ghostDifficulty);
    }

    public void SetMap(PacmanMap map, GhostDifficulty difficulty)
    {
        this.map = map;
        this.difficulty = difficulty;
        grid = this.map.GetGrid();
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Node node = grid.GetCellValue(x, y);
                if (node.isGhostHouseEntrance)
                {
                    ghostHouseEntranceNodes.Add(node);
                }
                if (node.isTeleportNode)
                {
                    teleportNodes.Add(node);
                }
            }
        }
        OnMapSet?.Invoke();
    }

    private void SpawnPellets(List<Vector3> exceptionPositions)
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Node node = grid.GetCellValue(x, y);
                Pellet prefab;
                if (node.hasPowerUpPellet)
                    prefab = pelletPowerUpPrefab;
                else if (node.passable && !node.isGhostHouse)
                    prefab = pelletPrefab;
                else
                    continue;
                Vector3 pelletSpawnPosition = grid.GetWorldPosition(x, y);
                if (exceptionPositions.Contains(pelletSpawnPosition))
                    continue;
                if (grid.GetCellValue(x, y).isTeleportNode)
                    continue;
                Pellet pellet = Instantiate(prefab, grid.GetWorldPosition(x, y), Quaternion.identity);
                pellet.NetworkObject.Spawn(true);
                pellet.transform.SetParent(transform);
                pellets.Add(pellet);
            }
        }
    }

    private void SpawnGhosts()
    {
        ghosts[0] = Blinky = Instantiate(blinkyPrefab, map.GetGhostStartingNode(GhostName.Blinky).GetWorldPosition(), Quaternion.identity);
        ghosts[1] = Pinky = Instantiate(pinkyPrefab, map.GetGhostStartingNode(GhostName.Pinky).GetWorldPosition(), Quaternion.identity);
        ghosts[2] = Inky = Instantiate(inkyPrefab, map.GetGhostStartingNode(GhostName.Inky).GetWorldPosition(), Quaternion.identity);
        ghosts[3] = Clyde = Instantiate(clydePrefab, map.GetGhostStartingNode(GhostName.Clyde).GetWorldPosition(), Quaternion.identity);
        foreach (Ghost ghost in ghosts)
        {
            ghost.NetworkObject.Spawn(true);
            Node scatterNode = map.GetGhostScatterDestination(ghost.ghostName);
            ghost.InitializeNodes(scatterNode);
            ghost.SetTargetPlayer(serverPacman);
        }
        SetGhostClientRpc(Blinky.NetworkObject, Pinky.NetworkObject, Inky.NetworkObject, Clyde.NetworkObject);
    }

    [ClientRpc]
    private void SetGhostClientRpc(NetworkObjectReference blinkyReference, NetworkObjectReference pinkyReference, NetworkObjectReference inkyReference, NetworkObjectReference clydeReference)
    {
        blinkyReference.TryGet(out var blinkyNetworkObject);
        pinkyReference.TryGet(out var pinkyNetworkObject);
        inkyReference.TryGet(out var inkyNetworkObject);
        clydeReference.TryGet(out var clydeNetworkObject);
        ghosts[0] = blinkyNetworkObject.GetComponent<Ghost>();
        ghosts[1] = pinkyNetworkObject.GetComponent<Ghost>();
        ghosts[2] = inkyNetworkObject.GetComponent<Ghost>();
        ghosts[3] = clydeNetworkObject.GetComponent<Ghost>();
        foreach (Ghost ghost in ghosts)
        {
            ghost.ghostState.OnValueChanged += OnGhostStateChanged;
        }
    }

    IEnumerator CountDown()
    {
        yield return null;
        CountDownTimer.Value = COUNT_DOWN_DURATION;
        OnCountdownStartClientRpc();
        yield return new WaitForSecondsRealtime(0.5f);
        while (CountDownTimer.Value > 0)
        {
            CountDownTimer.Value -= Time.unscaledDeltaTime;
            yield return null;
        }
        OnCountdownEndClientRpc();
    }

    [ClientRpc]
    private void OnCountdownStartClientRpc()
    {
        OnCountdownStart?.Invoke();
        AudioManager.Instance.Play("BackgroundMusic");
        Time.timeScale = 0f;
    }

    [ClientRpc]
    private void OnCountdownEndClientRpc()
    {
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
    }

    private void HandleSurvivalModeRemainingTime()
    {
        if (CurrentGameMode != GameMode.Survival || gameOver) return;
        if (survivalRemainingTime.Value > 0)
        {
            survivalRemainingTime.Value -= Time.deltaTime;
        }
        else
        {
            Debug.Log("SURVIVAL MODE TIMEOUT!");
            Victory();
        }
    }

    private void HandleGhostState()
    {
        if (currentGhostState == GhostState.Frightened)
        {
            frightenedStateElapsed += Time.deltaTime;
            if (frightenedStateElapsed >= frightenedStateDuration)
            {
                frightenedStateElapsed = 0f;
                ChangeGhostState(difficulty.ghostStateDurations[currentStateIndex].ghostState);
            }
            return;
        }
        stateDurationElapsed += Time.deltaTime;
        if (stateDurationElapsed >= difficulty.ghostStateDurations[currentStateIndex].duration)
        {
            currentStateIndex++;
            ChangeGhostState(difficulty.ghostStateDurations[currentStateIndex].ghostState);
            stateDurationElapsed = 0;
        }
    }

    private void ChangeGhostState(GhostState newState)
    {
        if (currentGhostState == GhostState.Frightened && newState == GhostState.Frightened)
        {
            frightenedStateElapsed = 0;
            foreach (Ghost ghost in ghosts)
            {
                if (ghost.currentNode.isGhostHouse || ghost.ghostState.Value == GhostState.Eaten)
                    continue;

                if (ghost.ghostState.Value == GhostState.Scatter || ghost.ghostState.Value == GhostState.Chase)
                {
                    ghost.ghostState.Value = newState;
                    ghost.OnFrightenedStateEnter();
                }
            }
        }
        foreach (Ghost ghost in ghosts)
        {
            if (ghost.currentNode.isGhostHouse || ghost.ghostState.Value == GhostState.Eaten)
                continue;
            if (newState == GhostState.Frightened)
            {
                ghost.OnFrightenedStateEnter();
            }
            ghost.ghostState.Value = newState;
        }
        currentGhostState = newState;
    }

    public void EatGhost(Ghost ghost)
    {
        EatGhostServerRpc(ghost.NetworkObject);
        OnGhostEatenByLocalPlayer?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EatGhostServerRpc(NetworkObjectReference ghostNetworkObjectReference)
    {
        ghostNetworkObjectReference.TryGet(out var ghostNetworkObject);
        Ghost ghost = ghostNetworkObject.GetComponent<Ghost>();
        if (ghost.ghostState.Value == GhostState.Eaten || ghost.currentNode.isGhostHouse) return;
        ghost.ghostState.Value = GhostState.Eaten;
        EatGhostClientRpc(ghostNetworkObjectReference);
    }

    [ClientRpc]
    private void EatGhostClientRpc(NetworkObjectReference ghostNetworkObjectReference)
    {
        ghostNetworkObjectReference.TryGet(out var ghostNetworkObject);
        Ghost ghost = ghostNetworkObject.GetComponent<Ghost>();
        ghost.SpawnDeathExplosion();
        AudioManager.Instance.Play("GhostEaten", true);
        OnGhostEaten?.Invoke(ghost);
    }

    public void KillPacman()
    {
        KillPacmanServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void KillPacmanServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client ID of the player that died
        ulong diedPlayerClientID = serverRpcParams.Receive.SenderClientId;
        // Disable the player
        Pacman player = GetPlayerBasedOnClientID(diedPlayerClientID);

        playerRemainingLivesTable[diedPlayerClientID]--;

        UpdateRemainingLivesTableForClient();

        // Spawn explosion
        GameObject explostionPrefab = PacmanMultiplayer.Instance.GetPlayerExplosionPrefab(diedPlayerClientID);
        GameObject spawnedExplosion = Instantiate(explostionPrefab, player.transform.position, Quaternion.identity);
        spawnedExplosion.GetComponent<NetworkObject>().Spawn(true);
        PlayDeathEffectClientRpc(diedPlayerClientID);

        if (playerRemainingLivesTable[diedPlayerClientID] >= 1)
        {
            // Respawn
            Vector3 respawnPosition = GetClientPlayerSpawnPosition(diedPlayerClientID);
            if (player.NetworkObject.IsOwnedByServer) // If the server player dies
            {
                player.Respawn(respawnPosition);
            }
            else
            {
                RespawnPlayerClientRpc(diedPlayerClientID, respawnPosition, player.NetworkObject);
            }
        }
        else
        {
            SetLocalPlayerDeadClientRpc(player.OwnerClientId);
            player.NetworkObject.Despawn(true);
            // Check if all players have died to init game over
            bool isGameOver = true;
            foreach (var kvp in playerRemainingLivesTable)
            {
                // If there's at least one player with remaining lives > 0
                if (kvp.Value > 0)
                {
                    isGameOver = false;
                }
            }
            if (isGameOver)
            {
                GameOverClientRpc();
            }
            return;
        }
    }

    [ClientRpc]
    private void PlayDeathEffectClientRpc(ulong diedClientID)
    {
        // Everyone should hear the death sound effect
        AudioManager.Instance.Play("PacmanDie", true);
        if (NetworkManager.Singleton.LocalClientId != diedClientID) return;
        // Only the player that died should see the camera shake
        CameraHandler.Instance.ShakeCamera(2f, 0.5f);
    }

    [ClientRpc]
    private void SetLocalPlayerDeadClientRpc(ulong deadClientID)
    {
        if (NetworkManager.Singleton.LocalClientId == deadClientID)
        {
            IsLocalClientDead = true;
            OnPlayerDead?.Invoke();
        }
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong diedClientID, Vector3 respawnPosition, NetworkObjectReference networkObjectReference)
    {
        if (NetworkManager.Singleton.LocalClientId != diedClientID) return;
        networkObjectReference.TryGet(out var networkObject);
        Pacman player = networkObject.GetComponent<Pacman>();
        player.Respawn(respawnPosition);
    }

    private void UpdateRemainingLivesTableForClient()
    {
        // We can't pass Dictionary as Rpc params so we create 2 arrays for keys and values and use it instead
        ulong[] clientIDs = new ulong[playerRemainingLivesTable.Count];
        int[] remainingLives = new int[playerRemainingLivesTable.Count];
        int i = 0;
        foreach (var kvp in playerRemainingLivesTable)
        {
            clientIDs[i] = kvp.Key;
            remainingLives[i] = kvp.Value;
            i++;
        }
        UpdatePlayerRemainingLivesTableClientRpc(clientIDs, remainingLives);
    }

    [ClientRpc]
    private void UpdatePlayerRemainingLivesTableClientRpc(ulong[] clientIDs, int[] remainingLives)
    {
        playerRemainingLivesTable.Clear();
        for (int i = 0; i < clientIDs.Length; i++)
        {
            playerRemainingLivesTable[clientIDs[i]] = remainingLives[i];
        }
        OnPacmanLivesChanged?.Invoke();
    }

    private Pacman GetPlayerBasedOnClientID(ulong clientID)
    {
        return spawnedPlayers[GetClientPlayerIndex(clientID)];
    }

    private int GetClientPlayerIndex(ulong clientID) => playerIndexTable[clientID];

    private Vector3 GetClientPlayerSpawnPosition(ulong clientID) => playerStartingPositions[GetClientPlayerIndex(clientID)];

    public void EatPowerupPellet(GameObject pellet)
    {
        EatPowerupPelletServerRpc(pellet.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void EatPowerupPelletServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out var networkObject);
        Pellet pellet = networkObject.GetComponent<Pellet>();
        RemovePelletFromList(pellet);
        ChangeGhostState(GhostState.Frightened);
        EatPowerupPelletClientRpc(pellet.transform.position);
    }

    [ClientRpc]
    private void EatPowerupPelletClientRpc(Vector3 eatenPosition)
    {
        AudioManager.Instance.Play("PowerupPelletEaten");
        OnPowerupPelletEaten?.Invoke(eatenPosition);
    }

    public void EatNormalPellet(GameObject pellet)
    {
        AudioManager.Instance.Play("PelletEaten", true);
        EatNormalPelletServerRpc(pellet.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void EatNormalPelletServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out var networkObject);
        Pellet pellet = networkObject.GetComponent<Pellet>();
        RemovePelletFromList(pellet);
        EatNormallPelletClientRpc();
    }

    public void EatPelletRespawner(GameObject pelletRespawner)
    {
        EatPelletRespawnerServerRpc(pelletRespawner.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void EatPelletRespawnerServerRpc(NetworkObjectReference pelletRespawnerNetworkObjectReference)
    {
        pelletRespawnerNetworkObjectReference.TryGet(out var fruitNetworkObject);
        fruitNetworkObject.Despawn();
        RespawnPellets();
    }

    [ClientRpc]
    private void EatNormallPelletClientRpc()
    {
        OnNormalPelletEaten?.Invoke();
    }

    private void RemovePelletFromList(Pellet pellet)
    {
        if (CurrentGameMode == GameMode.Classic)
        {
            pellets.Remove(pellet);
            eatenPellets.Add(pellet);
            DisablePelletClientRpc(pellet.GetComponent<NetworkObject>());
            if (IsWin())
            {
                foreach (Pacman player in spawnedPlayers)
                {
                    player.movementSpeed = 0;
                }
                Victory();
            }
        }
        else if (CurrentGameMode == GameMode.Survival)
        {
            eatenPellets.Add(pellet);
            DisablePelletClientRpc(pellet.GetComponent<NetworkObject>());
        }
    }

    [ClientRpc]
    private void DisablePelletClientRpc(NetworkObjectReference pelletNetworkObjectReference)
    {
        pelletNetworkObjectReference.TryGet(out var pelletNetworkObject);
        Pellet pellet = pelletNetworkObject.GetComponent<Pellet>();
        pellet.gameObject.SetActive(false);
    }

    [Command]
    private void RespawnPellets()
    {
        eatenPellets.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        Vector3 mapMiddlePoint = grid.GetCellValue(0, 0).GetWorldPosition() + grid.GetCellValue(grid.Width - 1, 0).GetWorldPosition();
        float middlePoint = mapMiddlePoint.x / 2f;
        NetworkObjectReference[] pelletNetworkObjectReferences = new NetworkObjectReference[eatenPellets.Count];
        for (int i = 0; i < eatenPellets.Count; i++)
        {
            pelletNetworkObjectReferences[i] = eatenPellets[i].NetworkObject;
        }
        RespawnPelletClientRpc(pelletNetworkObjectReferences);
        eatenPellets.Clear();
        OnPelletsRespawned?.Invoke();
    }

    [ClientRpc]
    private void RespawnPelletClientRpc(NetworkObjectReference[] pelletNetworkObjectReferences)
    {
        foreach (NetworkObjectReference pelletReference in pelletNetworkObjectReferences)
        {
            pelletReference.TryGet(out var pelletNetworkObject);
            Pellet pellet = pelletNetworkObject.GetComponent<Pellet>();
            pellet.gameObject.SetActive(true);
            pellet.PlayRespawnEffect();
        }
        AudioManager.Instance.Play("PelletRespawnerEaten");
    }

    public void Victory()
    {
        VictoryClientRpc();
    }

    [ClientRpc]
    private void VictoryClientRpc()
    {
        gameOver = true;
        Time.timeScale = 0f;
        OnGameVictory?.Invoke();
        AudioManager.Instance.StopAllSounds();
        AudioManager.Instance.Play("Victory");
    }

    [ClientRpc]
    private void GameOverClientRpc()
    {
        gameOver = true;
        Time.timeScale = 0f;
        AudioManager.Instance.StopAllSounds();
        OnGameLose?.Invoke();
        AudioManager.Instance.Play("GameOver");
    }

    private void OnGhostStateChanged(GhostState oldState, GhostState newState)
    {
        if (newState == GhostState.Frightened)
        {
            isAnyGhostFrightened = true;
        }
        else if (oldState == GhostState.Frightened)
        {
            isAnyGhostFrightened = IsAtLeastOneGhostFrightened();
        }
    }

    private bool IsAtLeastOneGhostFrightened()
    {
        foreach (Ghost ghost in ghosts)
        {
            if (ghost.ghostState.Value == GhostState.Frightened)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAnyGhostFrightened() => isAnyGhostFrightened;

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("Gameplay", () =>
        {
        });
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("MainMenu");
    }
}
