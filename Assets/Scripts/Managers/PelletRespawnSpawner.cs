using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PelletRespawnSpawner : NetworkBehaviour
{
    [SerializeField] private PelletRespawner pelletRespawnerPrefab;
    [SerializeField] private float timeBtwPelletRespawnerSpawns = 20f;
    private float pelletRespawnerSpawnedElapsed;
    private bool pelletRespawnerAvailableOnMap;

    private void Start()
    {
        GameManager.Instance.OnPelletsRespawned += SetSpawning;
        GameManager.Instance.OnGameStart += CheckGameMode;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.Instance.OnPelletsRespawned -= SetSpawning;
        GameManager.Instance.OnGameStart -= CheckGameMode;
    }

    private void Update()
    {
        if (!IsServer) return;
        HandlePelletRespawnerSpawning();
    }

    private void HandlePelletRespawnerSpawning()
    {
        if (GameManager.Instance.CurrentGameMode != GameMode.Survival) return;
        if (pelletRespawnerAvailableOnMap) return;

        pelletRespawnerSpawnedElapsed += Time.deltaTime;
        if (pelletRespawnerSpawnedElapsed > timeBtwPelletRespawnerSpawns)
        {
            Vector2Int spawnCoordinate = Utils.GetRandomElement(GameManager.Instance.map.respawnPelletCoordinates);
            Vector3 spawnPosition = new Vector3(spawnCoordinate.x, spawnCoordinate.y, 0f);
            PelletRespawner pelletRespawner = Instantiate(pelletRespawnerPrefab, spawnPosition, Quaternion.identity);
            pelletRespawner.GetComponent<NetworkObject>().Spawn(true);
            pelletRespawnerSpawnedElapsed = 0f;
            pelletRespawnerAvailableOnMap = true;
        }
    }

    private void SetSpawning()
    {
        // When a player eats a pellet respawner, set this to false to enable the spawning counter
        pelletRespawnerAvailableOnMap = false;
    }

    private void CheckGameMode()
    {
        if (!IsServer) return;
        if (GameManager.Instance.CurrentGameMode != GameMode.Survival)
        {
            NetworkObject.Despawn();
        }
    }
}
