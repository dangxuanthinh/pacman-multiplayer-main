using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameConclusionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private TextMeshProUGUI bestScore;
    [SerializeField] private TextMeshProUGUI bonusCoin;
    [SerializeField] private GameObject backgroundDim;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button exitButton;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        exitButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Instance.LoadScene("MainMenu");
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameVictory += ShowVictory;
        GameManager.Instance.OnGameLose += ShowDefeat;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameVictory -= ShowVictory;
        GameManager.Instance.OnGameLose -= ShowDefeat;
    }

    private void ShowVictory()
    {
        Show(true, GameScoreManager.Instance.GetCoinReward());
    }

    private void ShowDefeat()
    {
        Show(false, GameScoreManager.Instance.GetCoinReward());
    }

    public void Show(bool isWin, int bonusCoinAmount, float delay = 1f)
    {
        if (isWin)
        {
            victoryText.text = "VICTORY";
            victoryText.color = Color.green;
        }
        else
        {
            victoryText.text = "GAME OVER";
            victoryText.color = Color.red;
        }
        currentScore.text = $"Score:{GameScoreManager.Instance.CurrentScore.Value}";
        bonusCoin.text = $"Coin +{bonusCoinAmount}<sprite name=coin>";
        canvasGroup.DOFade(1f, 1f).SetDelay(delay).SetUpdate(UpdateType.Normal).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        });
    }
}
