using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private RegisterUI registerUI;

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitLogin);
        exitButton.onClick.AddListener(() =>
        {
            Hide(null);
        });
        registerButton.onClick.AddListener(() =>
        {
            registerUI.gameObject.SetActive(true);
        });

        UserManager.Instance.OnSignedIn += Hide;
    }

    private void OnDestroy()
    {
        UserManager.Instance.OnSignedIn -= Hide;
    }

    private void OnSubmitLogin()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Invalid input");
            return;
        }
        UserManager.Instance.Login(username, password);
    }

    private void Hide(string data)
    {
        usernameInput.text = "";
        passwordInput.text = "";
        gameObject.SetActive(false);
    }
}
