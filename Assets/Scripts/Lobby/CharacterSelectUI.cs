using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button updateLobbyButton;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI ghostDifficultyText;
    [SerializeField] private TextMeshProUGUI mapText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private LobbyCreationUI updateLobbyUI;
    private TextMeshProUGUI readyText;

    private void Awake()
    {
        readyText = readyButton.GetComponentInChildren<TextMeshProUGUI>();

        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectManager.Instance.SetReady();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Host disconnected, deleting lobby");
                PacmanLobby.Instance.DeleteLobby();
                NetworkManager.Singleton.Shutdown();
                SceneLoader.Instance.LoadScene("MainMenu");
                return;
            }
            PacmanLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Instance.LoadScene("Lobby");
        });

        if (NetworkManager.Singleton.IsServer)
        {
            updateLobbyButton.gameObject.SetActive(true);
            updateLobbyButton.onClick.AddListener(() =>
            {
                updateLobbyUI.gameObject.SetActive(true);
                updateLobbyUI.OpenLobbyUpdate(PacmanLobby.Instance.GetLobbyConfiguration());
            });
        }
        else
        {
            updateLobbyButton.gameObject.SetActive(false);
        }

        lobbyCodeText.text = "Lobby code: " + PacmanLobby.Instance.GetJoinedLobbyCode();
    }

    private void Start()
    {
        CharacterSelectManager.Instance.OnPlayerReadyChanged += UpdateReadyButton;
        PacmanMultiplayer.Instance.OnPlayerNetworkListChanged += UpdateReadyButton;
        PacmanLobby.Instance.OnLobbyChanged += UpdateLobbyConfigurationUI;
        UpdateReadyButton();
        UpdateLobbyConfigurationUI();
    }

    private void OnDestroy()
    {
        CharacterSelectManager.Instance.OnPlayerReadyChanged -= UpdateReadyButton;
        PacmanMultiplayer.Instance.OnPlayerNetworkListChanged -= UpdateReadyButton;
        PacmanLobby.Instance.OnLobbyChanged -= UpdateLobbyConfigurationUI;
    }

    private void UpdateReadyButton()
    {
        if (CharacterSelectManager.Instance.IsServer)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                CharacterSelectManager.Instance.StartGame();
                AudioManager.Instance.Play("ButtonPressed", true);
            });
            readyText.text = "START";
            return;
        }
        if (CharacterSelectManager.Instance.IsPlayerReady(NetworkManager.Singleton.LocalClientId))
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                CharacterSelectManager.Instance.CancelReady();
                AudioManager.Instance.Play("ButtonPressed", true);
            });
            readyText.text = "CANCEL READY";
        }
        else
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(() =>
            {
                CharacterSelectManager.Instance.SetReady();
                AudioManager.Instance.Play("ButtonPressed", true);
            });
            readyText.text = "READY";
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (CharacterSelectManager.Instance.AllClientsReady)
            {
                readyButton.interactable = true;
            }
            else
            {
                readyButton.interactable = false;
            }
        }
    }

    private void UpdateLobbyConfigurationUI()
    {
        ghostDifficultyText.text = "GHOST AI: " + PacmanLobby.Instance.GetGhostDifficultyName();
        mapText.text = "MAP: " + PacmanLobby.Instance.GetMapName();
        gameModeText.text = "MODE: " + PacmanLobby.Instance.GetGameMode();
    }
}
