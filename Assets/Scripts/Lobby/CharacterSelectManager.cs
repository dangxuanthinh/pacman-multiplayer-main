using QFSW.QC;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CharacterSelectManager : NetworkBehaviour
{
    public static CharacterSelectManager Instance;

    [ShowInInspector] public Dictionary<ulong, bool> playerReadyTable;

    public UnityAction OnPlayerReadyChanged;

    public bool AllClientsReady;

    private void Awake()
    {
        Instance = this;
        playerReadyTable = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        PacmanMultiplayer.Instance.OnClientConnected += RefreshPlayerReadyTable;
        PacmanMultiplayer.Instance.OnClientDisconnected += RefreshPlayerReadyTable;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PacmanMultiplayer.Instance.OnClientConnected -= RefreshPlayerReadyTable;
        PacmanMultiplayer.Instance.OnClientDisconnected -= RefreshPlayerReadyTable;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetReady();
        }
    }

    public void SetReady()
    {
        SetReadyServerRpc();
    }

    public void CancelReady()
    {
        CancelReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CancelReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyTable[serverRpcParams.Receive.SenderClientId] = false;
        playerReadyTable[NetworkManager.Singleton.LocalClientId] = true; // The host is always ready
        RefreshPlayerReadyTable();
        CancelReadyClientRpc(serverRpcParams.Receive.SenderClientId, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyTable[serverRpcParams.Receive.SenderClientId] = true;
        playerReadyTable[NetworkManager.Singleton.LocalClientId] = true; // The host is always ready
        RefreshPlayerReadyTable();
        SetReadyClientRpc(serverRpcParams.Receive.SenderClientId, AllClientsReady);
    }

    [Command]
    private void RefreshPlayerReadyTable()
    {
        AllClientsReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerReadyTable.ContainsKey(clientID) == false || playerReadyTable[clientID] == false)
            {
                AllClientsReady = false;
                break;
            }
        }
    }

    public void StartGame()
    {
        if (IsServer)
        {
            // We don't delete the lobby here because the lobby contains Data important to load the map/ghost difficulty
            // In Gameplay scene, GameManager will fetch the neccessary data from the lobby and delete lobby immedietely afterwards
            SceneLoader.Instance.LoadSceneNetwork("Gameplay");
        }
    }

    [ClientRpc]
    private void SetReadyClientRpc(ulong clientID, bool allClientsReady)
    {
        playerReadyTable[clientID] = true;
        AllClientsReady = allClientsReady;
        OnPlayerReadyChanged?.Invoke();
    }

    [ClientRpc]
    private void CancelReadyClientRpc(ulong clientID, bool allClientsReady)
    {
        playerReadyTable[clientID] = false;
        AllClientsReady = allClientsReady;
        OnPlayerReadyChanged?.Invoke();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyTable.ContainsKey(clientId) && playerReadyTable[clientId] == true;
    }

    public void NotifiyClientKicked(ulong kickedClientID)
    {
        SetReady(); // Call SetReady() to refresh the ready table
        //NotifyPlayerKickedClientRpc(kickedClientID);
    }

    [ClientRpc]
    public void NotifyPlayerKickedClientRpc(ulong kickedClientID)
    {
        Debug.Log(kickedClientID + " got kicked");
        if (NetworkManager.Singleton.LocalClientId == kickedClientID)
        {
            UIPopup.Instance.OpenPopup("You've been kicked by the host", null, null, true, false);
            SceneLoader.Instance.LoadScene("Lobby");
        }
    }
}
