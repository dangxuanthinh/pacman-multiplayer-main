using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeStopGhostClockVisual : MonoBehaviour
{
    [SerializeField] private Image clockImage;

    private void Update()
    {
        float fillAmount = PowerupManager.Instance.timeStopRemainingDurationNormalized.Value;
        if (fillAmount <= 0)
            clockImage.enabled = false;
        else
            clockImage.enabled = true;
        clockImage.fillAmount = fillAmount;
    }
}
