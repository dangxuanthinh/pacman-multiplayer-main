using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform lobbiesHolder;
    [SerializeField] private LobbyButton lobbyButtonPrefab;

    private void Start()
    {
        PacmanLobby.Instance.OnLobbiesRefreshed += RefreshLobbies;
    }

    private void OnDestroy()
    {
        PacmanLobby.Instance.OnLobbiesRefreshed -= RefreshLobbies;
    }

    public void RefreshLobbies(List<Lobby> lobbies)
    {
        foreach (Transform child in lobbiesHolder)
        {
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbies)
        {
            LobbyButton lobbyButton = Instantiate(lobbyButtonPrefab, lobbiesHolder);
            lobbyButton.SetData(lobby);
        }
    }
}
