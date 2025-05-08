using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PacmanMultiplayer : NetworkBehaviour
{
    public static PacmanMultiplayer Instance;

    public const int MAX_PLAYER = 4;

    public List<GameObject> playerModels = new List<GameObject>();
    public List<GameObject> playerExplosionPrefabs = new List<GameObject>();

    public List<Pacman> playerPrefabs = new List<Pacman>();
    public List<Color> playerModelColors = new List<Color>();

    private NetworkList<PlayerMultiplayerData> playerMultiplayerDataList;

    public UnityAction OnPlayerNetworkListChanged;
    public UnityAction OnClientConnected;
    public UnityAction OnClientDisconnected;

    private void Awake()
    {
        playerMultiplayerDataList = new NetworkList<PlayerMultiplayerData>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        playerMultiplayerDataList.OnListChanged += PlayerMultiplayerDataListChanged;
    }


    [Command]
    public void LocalClientID()
    {
        Debug.Log(NetworkManager.Singleton.LocalClientId);
    }

    private void PlayerMultiplayerDataListChanged(NetworkListEvent<PlayerMultiplayerData> changeEvent)
    {
        OnPlayerNetworkListChanged?.Invoke();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallBack;
        NetworkManager.Singleton.OnClientConnectedCallback += Host_OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback += Host_OnClientDisconnectedCallBack;
        NetworkManager.Singleton.StartHost();
        Debug.Log("Start Host");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
        Debug.Log("Start Client");
    }

    private void Client_OnClientDisconnectCallback(ulong clientID)
    {
        // If the host disconnected
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            string reason = NetworkManager.Singleton.DisconnectReason;
            if (string.IsNullOrEmpty(reason))
            {
                //UIPopup.Instance.OpenPopup("Disconnected", null, null, true, false);
            }
            else
            {
                UIPopup.Instance.OpenPopup(reason, null, null, true, false);
            }
            PacmanLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Instance.LoadScene("MainMenu");
        }
    }

    private void Client_OnClientConnectedCallBack(ulong clientID)
    {
        // Pass player ID and player name to the server
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
        SetPlayerNameServerRpc(GetPlayerName());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIDServerRpc(string playerID, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerMultiplayerDataIndex(serverRpcParams.Receive.SenderClientId);
        PlayerMultiplayerData playerData = GetPlayerMultiplayerData(playerDataIndex);
        playerData.playerID = playerID;
        playerMultiplayerDataList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerMultiplayerDataIndex(serverRpcParams.Receive.SenderClientId);
        PlayerMultiplayerData playerData = playerMultiplayerDataList[playerDataIndex];
        playerData.playerName = playerName;
        playerMultiplayerDataList[playerDataIndex] = playerData;
    }

    private void Host_OnClientDisconnectedCallBack(ulong clientID)
    {
        // Remove disconnected client's multiplayer data from list
        for (int i = 0; i < playerMultiplayerDataList.Count; i++)
        {
            PlayerMultiplayerData playerData = playerMultiplayerDataList[i];
            if (playerData.clientID == clientID)
            {
                playerMultiplayerDataList.RemoveAt(i);
            }
        }
        OnClientDisconnected?.Invoke();
    }

    private void Host_OnClientConnectedCallBack(ulong clientID)
    {
        // Create new player data for the newly connected client and add it to the list
        bool isClientAlreadyConnected = false;
        foreach (var playerData in playerMultiplayerDataList)
        {
            if (playerData.clientID == clientID)
            {
                isClientAlreadyConnected = true;
            }
        }
        if (isClientAlreadyConnected == false)
        {
            playerMultiplayerDataList.Add(new PlayerMultiplayerData
            {
                clientID = clientID,
                modelIndex = GetFirstUnusedModelIndex(),
                playerName = GetPlayerName(),
                isServer = NetworkManager.Singleton.LocalClientId == clientID,
            });
        }
        OnClientConnected?.Invoke();
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void ConnectionApprovalCallBack(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != "CharacterSelect")
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Lobby is full";
        }
        connectionApprovalResponse.Approved = true;
    }

    public void KickPlayer(ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID, "You've been kicked by the host");
        Host_OnClientDisconnectedCallBack(clientID);
    }


    public bool IsPlayerConnected(int playerIndex)
    {
        return playerMultiplayerDataList.Count > playerIndex;
    }


    public PlayerMultiplayerData GetPlayerMultiplayerData(int playerIndex)
    {
        return playerMultiplayerDataList[playerIndex];
    }

    public PlayerMultiplayerData GetPlayerMultiplayerData(ulong clientID)
    {
        foreach (var playerData in playerMultiplayerDataList)
        {
            if (playerData.clientID == clientID)
            {
                return playerData;
            }
        }
        return default;
    }

    public int GetPlayerMultiplayerDataIndex(ulong clientID)
    {
        for (int i = 0; i < playerMultiplayerDataList.Count; i++)
        {
            if (playerMultiplayerDataList[i].clientID == clientID)
            {
                return i;
            }
        }
        return -1;
    }

    public void SetPlayerSelectedModel(int modelIndex)
    {
        SetPlayerSelectedModelServerRpc(modelIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerSelectedModelServerRpc(int modelIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientID = serverRpcParams.Receive.SenderClientId;
        int playerDataIndex = GetPlayerMultiplayerDataIndex(clientID);
        PlayerMultiplayerData playerData = playerMultiplayerDataList[playerDataIndex];
        playerData.modelIndex = modelIndex;
        playerMultiplayerDataList[playerDataIndex] = playerData;
    }

    private bool IsModelAvailable(int modelIndex)
    {
        foreach (var playerData in playerMultiplayerDataList)
        {
            if (playerData.modelIndex == modelIndex)
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedModelIndex()
    {
        for (int i = 0; i < playerModels.Count; i++)
        {
            if (IsModelAvailable(i))
            {
                return i;
            }
        }
        return 0;
    }

    public Color GetPlayerColor(ulong clientID)
    {
        PlayerMultiplayerData playerData = GetPlayerMultiplayerData(clientID);
        return playerModelColors[playerData.modelIndex];
    }

    public Pacman GetPlayerSelectedModel(ulong clientID)
    {
        PlayerMultiplayerData playerData = GetPlayerMultiplayerData(clientID);
        return playerPrefabs[playerData.modelIndex];
    }

    public string GetPlayerName()
    {
        return UserManager.Instance.SignedInUserName;
    }

    public GameObject GetPlayerExplosionPrefab(ulong playerClientID)
    {
        PlayerMultiplayerData playerData = GetPlayerMultiplayerData(playerClientID);
        return playerExplosionPrefabs[playerData.modelIndex];
    }
}
