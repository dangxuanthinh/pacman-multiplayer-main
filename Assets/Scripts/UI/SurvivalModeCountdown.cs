using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivalModeCountdown : MonoBehaviour
{
    private TextMeshProUGUI countdown;

    private void Awake()
    {
        countdown = GetComponent<TextMeshProUGUI>();
        countdown.enabled = false;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += CheckGameMode;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= CheckGameMode;
    }

    private void Update()
    {
        float remainingSeconds = GameManager.Instance.survivalRemainingTime.Value;
        int minutes = (int)(remainingSeconds / 60);
        int seconds = (int)(remainingSeconds % 60);
        string timeFormatted = minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
        countdown.text = timeFormatted;
    }

    private void CheckGameMode()
    {
        if (GameManager.Instance.CurrentGameMode != GameMode.Survival)
        {
            Destroy(gameObject);
        }
        else
        {
            countdown.enabled = true;
        }
    }
}
