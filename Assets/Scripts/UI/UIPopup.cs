using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour
{
    public static UIPopup Instance;

    private UnityAction OnConfirmedAction; // Other scripts will send in an event
    private UnityAction OnCanceledAction; // Other scripts will send in an event
    [SerializeField] private GameObject comfirmPanel;
    [SerializeField] private GameObject backgroundDim;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private const float FADE_DURATION = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
    }

    public void OpenPopup(string confirmText, UnityAction confirmAction, UnityAction cancelAction, bool showConfirmButton = true, bool showCancelButton = true)
    {
        OnConfirmedAction = confirmAction;
        OnCanceledAction = cancelAction;
        this.confirmText.text = confirmText;
        confirmButton.gameObject.SetActive(showConfirmButton);
        cancelButton.gameObject.SetActive(showCancelButton);
        Show();
    }

    public void HidePopup()
    {
        confirmText.text = string.Empty;
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        Hide();
    }

    private void Confirm()
    {
        OnConfirmedAction?.Invoke();
        Hide();
    }

    private void Cancel()
    {
        OnCanceledAction?.Invoke();
        Hide();
    }

    private void Show()
    {
        backgroundDim.SetActive(true);
        comfirmPanel.SetActive(true);
    }

    private void Hide()
    {
        backgroundDim.SetActive(false);
        comfirmPanel.SetActive(false);
    }
}
