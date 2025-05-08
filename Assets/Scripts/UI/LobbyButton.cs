using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI availableSlot;
    [SerializeField] private TextMeshProUGUI gameMode;
    private Lobby lobbyData;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PacmanLobby.Instance.JoinLobbyWithID(lobbyData.Id);
        });
    }

    public void SetData(Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        availableSlot.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        gameMode.text = $"{lobby.Data["GameMode"].Value}";
        lobbyData = lobby;
    }
}
