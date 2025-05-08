using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button retryButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        pauseButton.onClick.AddListener(ShowPauseMenu);
        continueButton.onClick.AddListener(HidePauseMenu);
        exitButton.onClick.AddListener(GameManager.Instance.ExitGame);
        retryButton.onClick.AddListener(GameManager.Instance.RestartGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        AudioManager.Instance.HandleAudioOnGamePaused();
        Time.timeScale = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
    }

    private void HidePauseMenu()
    {
        AudioManager.Instance.HandleAudioOnGameUnPaused();
        Time.timeScale = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.DOFade(0f, 0.1f);
    }
}
