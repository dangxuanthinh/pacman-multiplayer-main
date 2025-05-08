using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;
    private Image backgroundDim;

    private void Awake()
    {
        backgroundDim = GetComponent<Image>();
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += HideCountdown;
        GameManager.Instance.OnCountdownStart += ShowCountdown;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= HideCountdown;
        GameManager.Instance.OnCountdownStart -= ShowCountdown;
    }

    private void Update()
    {
        int countDown = Mathf.FloorToInt(GameManager.Instance.CountDownTimer.Value);
        if (countDown == 0)
        {
            countDownText.text = "START";
        }
        else
        {
            countDownText.text = countDown.ToString();
        }
    }

    private void ShowCountdown()
    {
        backgroundDim.enabled = true;
        countDownText.enabled = true;
    }

    private void HideCountdown()
    {
        backgroundDim.enabled = false;
        countDownText.enabled = false;
    }
}
