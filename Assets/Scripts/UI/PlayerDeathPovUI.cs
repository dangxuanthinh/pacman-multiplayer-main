using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerDeathPovUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spectateText;
    private bool switchedCamera;

    private void Start()
    {
        CameraHandler.Instance.OnCameraSwitched += OnCameraSwitched;
    }

    private void OnDestroy()
    {
        CameraHandler.Instance.OnCameraSwitched -= OnCameraSwitched;
    }

    private void OnCameraSwitched()
    {
        switchedCamera = true;
    }

    private void Update()
    {
        // If local player is dead but the game is not over
        if (GameManager.Instance.IsLocalClientDead && GameManager.Instance.gameOver == false && switchedCamera == false)
        {
            spectateText.gameObject.SetActive(true);
        }
        else
        {
            spectateText.gameObject.SetActive(false);
        }
    }
}
