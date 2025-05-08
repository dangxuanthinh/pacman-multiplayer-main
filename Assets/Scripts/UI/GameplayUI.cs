using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro scorePopupPrefab;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private Transform pacmanRemainingLivesHolder;
    [SerializeField] private Image pacmanIconPrefab;

    private void Start()
    {
        GameScoreManager.Instance.CurrentScore.OnValueChanged += UpdateScore;
        GameScoreManager.Instance.OnGhostEatenScoreIncreased += SpawnScorePopUp;
        GameManager.Instance.OnPacmanLivesChanged += UpdateRemainingLives;
        GameManager.Instance.OnGameStart += UpdateRemainingLives;
    }

    private void OnDestroy()
    {
        GameScoreManager.Instance.CurrentScore.OnValueChanged -= UpdateScore;
        GameScoreManager.Instance.OnGhostEatenScoreIncreased -= SpawnScorePopUp;
        GameManager.Instance.OnPacmanLivesChanged -= UpdateRemainingLives;
        GameManager.Instance.OnGameStart -= UpdateRemainingLives;
    }

    public void UpdateScore(int score, int newScore)
    {
        currentScore.text = "SCORE:" + newScore.ToString();
        currentScore.DOColor(Color.yellow, 0.1f).OnComplete(() =>
        {
            currentScore.DOColor(Color.white, 0.1f);
        });
        currentScore.transform.DOScale(Vector3.one * 1.1f, 0.1f).OnComplete(() =>
        {
            currentScore.transform.DOScale(Vector3.one * 1f, 0.1f);
        });
    }

    public void SpawnScorePopUp(Vector3 spawnPosition, int bonusScoreAmount)
    {
        spawnPosition.z = -2f;
        TextMeshPro scorePopup = Instantiate(scorePopupPrefab, spawnPosition, Quaternion.identity);
        scorePopup.text = bonusScoreAmount.ToString();
        scorePopup.transform.localScale = Vector3.one * 0.6f;
        scorePopup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            scorePopup.transform.DOScale(0f, 0.2f).SetDelay(0.4f).OnComplete(() =>
            {
                Destroy(scorePopup);
            });
        });
    }

    public void UpdateRemainingLives()
    {
        foreach (Transform child in pacmanRemainingLivesHolder)
        {
            Destroy(child.gameObject);
        }
        foreach (var kvp in GameManager.Instance.playerRemainingLivesTable)
        {
            Image remainingLivesIcon = Instantiate(pacmanIconPrefab, pacmanRemainingLivesHolder);
            remainingLivesIcon.color = PacmanMultiplayer.Instance.GetPlayerColor(kvp.Key);
            TextMeshProUGUI remainingLivesText = remainingLivesIcon.GetComponentInChildren<TextMeshProUGUI>();
            remainingLivesText.text = $"x{kvp.Value}";
        }
    }
}
