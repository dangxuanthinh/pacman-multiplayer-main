using JetBrains.Annotations;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PacmanLobby : MonoBehaviour
{
    public static PacmanLobby Instance;

    private Lobby joinedLobby;
    private LobbyConfiguration lobbyConfiguration;

    private UnityTransport unityTransport;
    private float heartBeatElapsed;
    private float lobbyRefreshElapsed;

    public UnityAction<List<Lobby>> OnLobbiesRefreshed;
    public UnityAction OnLobbyChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAuthentication();
    }

    private void Start()
    {
        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    private void Update()
    {
        if (IsLobbyHost())
        {
            PingHeartbeatPeriodically();
        }

        RefreshLobbiesPeriodically();
    }

    private void RefreshLobbiesPeriodically()
    {
        if (joinedLobby != null || AuthenticationService.Instance.IsSignedIn == false || SceneManager.GetActiveScene().name != "Lobby") return;
        lobbyRefreshElapsed += Time.deltaTime;
        if (lobbyRefreshElapsed >= 4.5f)
        {
            lobbyRefreshElapsed = 0;
            RefreshLobbies();
        }
    }

    private void PingHeartbeatPeriodically()
    {
        heartBeatElapsed += Time.deltaTime;
        if (heartBeatElapsed >= 15f)
        {
            heartBeatElapsed = 0;
            LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeAuthentication()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 9999).ToString());
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void RefreshLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            OnLobbiesRefreshed?.Invoke(queryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void CreateLobby(string lobbyName, LobbyConfiguration lobbyConfiguration)
    {
        try
        {
            UIPopup.Instance.OpenPopup("CREATING LOBBY...", null, null, false, false);

            Allocation relayAllocation = await RelayManager.Instance.AllocateRelay();
            Debug.Log("Relay allocated");
            string relayJoinCode = await RelayManager.Instance.GenerateRelayCode(relayAllocation);
            Debug.Log("Relay code generated");
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = lobbyConfiguration.isPrivate;
            createLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { "GameMode",  new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.gameMode.ToString())},
                { "Map", new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.map.mapName) },
                { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.ghostDifficulty.difficultyName) },
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, PacmanMultiplayer.MAX_PLAYER, createLobbyOptions);
            Debug.Log("Lobby created");
            await SetupLobbyCallBacks();
            Debug.Log("Lobby CallBacks set up");
            RelayServerData relayServerData = new RelayServerData(relayAllocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);
            PacmanMultiplayer.Instance.StartHost();
            SceneLoader.Instance.LoadSceneNetwork("CharacterSelect");
            UIPopup.Instance.HidePopup();
        }
        catch (LobbyServiceException e)
        {
            UIPopup.Instance.HidePopup();
            Debug.LogError(e);
        }
    }

    private async Task SetupLobbyCallBacks()
    {
        try
        {
            LobbyEventCallbacks callBacks = new LobbyEventCallbacks();
            callBacks.LobbyChanged += OnLobbyChangedCallBack;
            await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callBacks);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void OnLobbyChangedCallBack(ILobbyChanges changes)
    {
        Debug.Log("Lobby updated");

        if (joinedLobby == null)
            return;

        changes.ApplyToLobby(joinedLobby);

        LobbyConfiguration lobbyConfiguration = new LobbyConfiguration();
        lobbyConfiguration.map = GameResourceHandler.Instance.GetMap(joinedLobby.Data["Map"].Value);
        lobbyConfiguration.ghostDifficulty = GameResourceHandler.Instance.GetGhostDifficulty(joinedLobby.Data["Difficulty"].Value);
        if (Enum.TryParse(joinedLobby.Data["GameMode"].Value, true, out GameMode gameMode))
        {
            lobbyConfiguration.gameMode = gameMode;
        }
        lobbyConfiguration.isPrivate = joinedLobby.IsPrivate;
        SetLobbyConfiguration(lobbyConfiguration);

        OnLobbyChanged?.Invoke();
    }

    [Command]
    public void LogLobbyData()
    {
        foreach (var kvp in joinedLobby.Data)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Value}");
        }
        Debug.Log(lobbyConfiguration);
    }

    public void SetLobbyConfiguration(LobbyConfiguration lobbyConfiguration)
    {
        this.lobbyConfiguration = lobbyConfiguration;
        Debug.Log("Lobby configuration set: " + lobbyConfiguration.ToString());
    }

    public async void UpdateLobby(LobbyConfiguration lobbyConfiguration)
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
            updateLobbyOptions.IsPrivate = lobbyConfiguration.isPrivate;
            updateLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { "GameMode",  new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.gameMode.ToString())},
                { "Map", new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.map.mapName) },
                { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, lobbyConfiguration.ghostDifficulty.difficultyName) },
            };
            joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updateLobbyOptions);
            SetLobbyConfiguration(lobbyConfiguration);
            OnLobbyChanged?.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            UIPopup.Instance.OpenPopup("Finding lobby...", null, null, false, false);
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            await SetupLobbyCallBacks();
            await SetupUnityTransportRelayServerData();
            PacmanMultiplayer.Instance.StartClient();
            UIPopup.Instance.HidePopup();
        }
        catch (LobbyServiceException e)
        {
            UIPopup.Instance.OpenPopup("Quick join lobby failed", null, null, true, false);
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithCode(string lobbyCode)
    {
        try
        {
            UIPopup.Instance.OpenPopup("Joining lobby...", null, null, false, false);
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            await SetupLobbyCallBacks();
            await SetupUnityTransportRelayServerData();
            PacmanMultiplayer.Instance.StartClient();
            UIPopup.Instance.HidePopup();
        }
        catch (LobbyServiceException e)
        {
            UIPopup.Instance.OpenPopup("Could not join lobby with code, either an error occured or the code is incorrect", null, null, true, false);
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithID(string lobbyID)
    {
        try
        {
            UIPopup.Instance.OpenPopup("Joining lobby...", null, null, false, false);
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);
            await SetupLobbyCallBacks();
            await SetupUnityTransportRelayServerData();
            PacmanMultiplayer.Instance.StartClient();
            UIPopup.Instance.HidePopup();
        }
        catch (LobbyServiceException e)
        {
            UIPopup.Instance.OpenPopup("An error occured when joining lobby", null, null, true, false);
            Debug.LogError(e);
        }
    }

    private async Task SetupUnityTransportRelayServerData()
    {
        string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
        JoinAllocation relayJoinAllocation = await RelayManager.Instance.JoinRelay(relayJoinCode);
        RelayServerData relayServerData = new RelayServerData(relayJoinAllocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);
        Debug.Log("Unity transport replay server data set");
    }

    [Command]
    public void ShowLobbyData()
    {
        Debug.Log(joinedLobby.Data["Map"].Value);
    }

    public LobbyConfiguration GetLobbyConfiguration()
    {
        return lobbyConfiguration;
    }

    public string GetJoinedLobbyCode()
    {
        return joinedLobby.LobbyCode;
    }

    public string GetMapName()
    {
        return joinedLobby.Data["Map"].Value;
    }

    public string GetGhostDifficultyName()
    {
        return joinedLobby.Data["Difficulty"].Value;
    }

    public GameMode GetGameMode()
    {
        if (Enum.TryParse(joinedLobby.Data["GameMode"].Value, out GameMode gameMode))
        {
            return gameMode;
        }
        else
        {
            Debug.LogError("Can't parse GameMode enum from : " + joinedLobby.Data["GameMode"].Value + " defaulting to Survival");
            return GameMode.Survival;
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby == null) return;
        if (NetworkManager.Singleton.IsServer == false) return;
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby == null) return;
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public async void KickPlayer(string playerID)
    {
        if (IsLobbyHost() == false) return;
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void OnApplicationQuit()
    {
        DeleteLobby();
    }
}
