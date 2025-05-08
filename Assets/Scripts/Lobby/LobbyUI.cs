using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinByCodeButton;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private LobbyCreationUI lobbyCreationUI;

    private void Start()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            PacmanLobby.Instance.LeaveLobby();
            SceneLoader.Instance.LoadScene("MainMenu");
        });

        createButton.onClick.AddListener(() =>
        {
            lobbyCreationUI.gameObject.SetActive(true);
            lobbyCreationUI.ResetOptions();
        });

        joinButton.onClick.AddListener(() =>
        {
            PacmanLobby.Instance.QuickJoinLobby();
        });

        joinByCodeButton.onClick.AddListener(() =>
        {
            PacmanLobby.Instance.JoinLobbyWithCode(codeInputField.text);
        });
    }
}
