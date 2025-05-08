using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button achievementButton;
    [SerializeField] private Button startMultiplayerPlayerbutton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private LoginUI loginUI;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        Time.timeScale = 1f;

        startMultiplayerPlayerbutton.onClick.AddListener(() =>
        {
            if (UserManager.Instance.IsSignedIn == false)
            {
                loginUI.gameObject.SetActive(true);
                return;
            }
            SceneLoader.Instance.LoadScene("Lobby");
        });

        achievementButton.onClick.AddListener(() =>
        {
            AchievementPanel.Instance.Open();
        });

        exitGameButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        optionButton.onClick.AddListener(() =>
        {
            settingsUI.panel.SetActive(true);
        });
    }

    private void Start()
    {
        AudioManager.Instance.Play("LobbyMusic");
    }
}
