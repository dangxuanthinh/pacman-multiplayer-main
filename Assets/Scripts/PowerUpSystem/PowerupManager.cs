using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerupManager : NetworkBehaviour
{
    public static PowerupManager Instance;

    public float powerupSpawnInterval = 15;

    [SerializeField] private List<PowerupPickup> powerupPickups = new List<PowerupPickup>();
    [SerializeField] private List<PowerUp> powerups = new List<PowerUp>();
    private List<PowerupPickup> spawnedPowerupPickups = new List<PowerupPickup>();
    private List<Vector3> freePowerupSpawnPosition = new List<Vector3>();
    private float powerupSpawnedElapsed;

    private List<PlayerPowerupObtained> playerObtainedPowerupList = new List<PlayerPowerupObtained>();

    public NetworkVariable<float> timeStopRemainingDurationNormalized = new NetworkVariable<float>(0f);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
            GameManager.Instance.OnPowerupPelletEaten += AddPowerupSpawnPosition;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsServer)
            GameManager.Instance.OnPowerupPelletEaten -= AddPowerupSpawnPosition;
    }

    private void Update()
    {
        if (!IsServer) return;
        HandlePowerupPickupSpawning();
        HandlePowerupDuration();
        HandleTimeStopDurationForUI();
    }

    private void HandleTimeStopDurationForUI()
    {
        // Keep tracking of remaining duration of TimeStop powerup for UI to display
        bool hasTimeStopPowerup = false;
        foreach (var playerPowerupObtained in playerObtainedPowerupList)
        {
            PowerUp powerup = playerPowerupObtained.powerup;
            if (powerup.powerupName == "TimeStop")
            {
                hasTimeStopPowerup = true;
                timeStopRemainingDurationNormalized.Value = powerup.remainingDuration / powerup.duration;
            }
        }
        if (hasTimeStopPowerup == false)
        {
            timeStopRemainingDurationNormalized.Value = 0f;
        }
    }

    private void AddPowerupSpawnPosition(Vector3 position)
    {
        freePowerupSpawnPosition.Add(position);
    }

    public void RemovePowerupPickup(PowerupPickup powerupPickup)
    {
        spawnedPowerupPickups.Remove(powerupPickup);
        freePowerupSpawnPosition.Add(powerupPickup.transform.position);
        powerupPickup.NetworkObject.Despawn();
    }

    private void HandlePowerupDuration()
    {
        for (int i = playerObtainedPowerupList.Count - 1; i >= 0; i--)
        {
            PowerUp powerUp = playerObtainedPowerupList[i].powerup;
            powerUp.remainingDuration -= Time.deltaTime;

            if (powerUp.remainingDuration <= 0)
            {
                DespawnPowerupObject(powerUp, playerObtainedPowerupList[i].player);
            }
        }
    }

    private void HandlePowerupPickupSpawning()
    {
        if (GameManager.Instance.gameOver || GameManager.Instance.spawnedPlayers.Count == 0 || spawnedPowerupPickups.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count || freePowerupSpawnPosition.Count == 0) return;
        powerupSpawnedElapsed += Time.deltaTime;
        if (powerupSpawnedElapsed > powerupSpawnInterval)
        {
            Vector3 powerupSpawnPosition = Utils.GetRandomElement(freePowerupSpawnPosition);
            PowerupPickup powerupToSpawn = Utils.GetRandomElement(powerupPickups);
            PowerupPickup spawnedPickup = Instantiate(powerupToSpawn, powerupSpawnPosition, Quaternion.identity);
            freePowerupSpawnPosition.Remove(powerupSpawnPosition);
            spawnedPickup.NetworkObject.Spawn();
            spawnedPowerupPickups.Add(spawnedPickup);
            powerupSpawnedElapsed = 0;
        }
    }

    public void SpawnPowerupObject(string powerupName, GameObject player)
    {
        Pacman playerPacman = player.GetComponent<Pacman>();

        foreach (var playerObtainedPowerup in playerObtainedPowerupList)
        {
            PowerUp powerup = playerObtainedPowerup.powerup;
            if (playerObtainedPowerup.playerClientID == playerPacman.OwnerClientId && powerup.powerupName == powerupName)
            {
                powerup.remainingDuration = powerup.duration;
                return;
            }
        }

        PowerUp powerupPrefab = GetPowerUp(powerupName);
        GameObject spawnedPowerupGO = Instantiate(powerupPrefab, player.transform).gameObject;

        PowerUp spawnedPowerup = spawnedPowerupGO.GetComponent<PowerUp>();
        spawnedPowerup.NetworkObject.Spawn(true);
        spawnedPowerup.NetworkObject.ChangeOwnership(playerPacman.OwnerClientId);
        spawnedPowerup.remainingDuration = spawnedPowerup.duration;

        PlayerPowerupObtained playerPowerupObtained = new PlayerPowerupObtained()
        {
            player = playerPacman,
            powerup = spawnedPowerup,
            playerClientID = playerPacman.OwnerClientId
        };
        playerObtainedPowerupList.Add(playerPowerupObtained);

        SpawnPowerupObjectClientRpc(spawnedPowerup.NetworkObject, player.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void SpawnPowerupObjectClientRpc(NetworkObjectReference powerupNetworkObjectReference, NetworkObjectReference playerNetworkObjectReference)
    {
        powerupNetworkObjectReference.TryGet(out var powerupNetworkObject);
        playerNetworkObjectReference.TryGet(out var playerNetworkObject);
        PowerUp powerUp = powerupNetworkObject.GetComponent<PowerUp>();
        powerUp.SetParent(playerNetworkObject.GetComponent<Pacman>());
        powerUp.Setup(playerNetworkObject.GetComponent<Pacman>());
    }

    public void DespawnPowerupObject(PowerUp powerup, Pacman player)
    {
        if (powerup.NetworkObject.IsSpawned == false) return;
        CleanupPowerupClientRpc(powerup.NetworkObject, player.NetworkObject);
    }

    [ClientRpc]
    private void CleanupPowerupClientRpc(NetworkObjectReference powerupNetworkObjectReference, NetworkObjectReference playerNetworkObjectReference)
    {
        powerupNetworkObjectReference.TryGet(out var powerupNetworkObject);
        playerNetworkObjectReference.TryGet(out var playerNetworkObject);
        powerupNetworkObject.GetComponent<PowerUp>().Cleanup(playerNetworkObject.GetComponent<Pacman>());
        Debug.Log("Cleaning up " + powerupNetworkObject.gameObject.name);
        DespawnPowerupObjectServerRpc(powerupNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnPowerupObjectServerRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out var networkObject))
        {
            PowerUp powerup = networkObject.GetComponent<PowerUp>();
            PlayerPowerupObtained powerupToRemove = playerObtainedPowerupList.Find(p => p.powerup == powerup);
            playerObtainedPowerupList.Remove(powerupToRemove);
            powerup.NetworkObject.Despawn(true);
        }
    }

    private PowerUp GetPowerUp(string powerupName)
    {
        return powerups.Find(p => p.powerupName == powerupName);
    }

    [Serializable]
    private class PlayerPowerupObtained
    {
        public Pacman player;
        public ulong playerClientID;
        public PowerUp powerup;
    }
}
