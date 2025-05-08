using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyCreationUI : MonoBehaviour
{
    public TMP_InputField lobbyNameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Toggle publicLobbyToggle;
    [SerializeField] private ChangeDifficultyUI changeDifficultyUI;
    [SerializeField] private ChangeGameModeUI changeGameModeUI;
    [SerializeField] private PacmanLevelSelector changeMapUI;
    [SerializeField] private Transform panel;
    private CanvasGroup canvasGroup;
    private bool isPublic;

    private void Awake()
    {
        canvasGroup = panel.GetComponent<CanvasGroup>();
        publicLobbyToggle.onValueChanged.AddListener(SetPublic);
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            confirmButton.onClick.AddListener(() =>
            {
                ConfirmCreateLobby();
            });
            confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "CONFIRM";
        }
        else if (SceneManager.GetActiveScene().name == "CharacterSelect")
        {
            confirmButton.onClick.AddListener(() =>
            {
                ConfirmUpdateLobby();
            });
            confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "UPDATE";
        }

        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void OnEnable()
    {
        panel.localScale = Vector3.one * 0.8f;
        panel.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void ConfirmCreateLobby()
    {
        LobbyConfiguration lobbyConfiguration = GetLobbyConfiguration();
        PacmanLobby.Instance.SetLobbyConfiguration(lobbyConfiguration);
        PacmanLobby.Instance.CreateLobby(lobbyNameInputField.text, lobbyConfiguration);
    }

    private void ConfirmUpdateLobby()
    {
        LobbyConfiguration lobbyConfiguration = GetLobbyConfiguration();
        PacmanLobby.Instance.UpdateLobby(lobbyConfiguration);
        gameObject.SetActive(false);
    }

    private LobbyConfiguration GetLobbyConfiguration()
    {
        LobbyConfiguration lobbyConfiguration = new LobbyConfiguration();
        lobbyConfiguration.map = changeMapUI.GetSelectedMap();
        lobbyConfiguration.ghostDifficulty = changeDifficultyUI.GetSelectedDifficulty();
        lobbyConfiguration.gameMode = changeGameModeUI.GetSelectedGameMode();
        lobbyConfiguration.isPrivate = !isPublic;
        return lobbyConfiguration;
    }

    private void SetPublic(bool isPublic)
    {
        this.isPublic = isPublic;
    }

    public void ResetOptions()
    {
        lobbyNameInputField.text = UserManager.Instance.SignedInUserName + "'s lobby";
        SetPublic(true);
        changeDifficultyUI.ResetToDefault();
        changeGameModeUI.ResetToDefault();
        changeMapUI.ResetToDefault();
    }

    public void Hide()
    {
        panel.DOScale(0.4f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void OpenLobbyUpdate(LobbyConfiguration lobbyConfiguration)
    {
        publicLobbyToggle.SetIsOnWithoutNotify(!lobbyConfiguration.isPrivate);
        SetPublic(!lobbyConfiguration.isPrivate);
        changeDifficultyUI.SetSelectedDifficulty(lobbyConfiguration.ghostDifficulty);
        changeGameModeUI.SetSelectedGameMode(lobbyConfiguration.gameMode);
        changeMapUI.SetSelectedMap(lobbyConfiguration.map);
    }
}
