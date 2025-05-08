using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField repeatPasswordInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitRegister);
        exitButton.onClick.AddListener(() =>
        {
            Hide();
        });

        UserManager.Instance.OnRegistered += Hide;
    }

    private void OnDestroy()
    {
        UserManager.Instance.OnRegistered -= Hide;
    }

    private void OnSubmitRegister()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string repeatPassword = repeatPasswordInput.text;

        if (password.Length < 6)
        {
            UIPopup.Instance.OpenPopup("Password must have at least 6 characters", null, null, true, false);
            return;
        }

        if (username.Length < 6)
        {
            UIPopup.Instance.OpenPopup("User name must have at least 6 characters", null, null, true, false);
            return;
        }

        if (repeatPassword != password)
        {
            UIPopup.Instance.OpenPopup("Password does not match", null, null, true, false);
            passwordInput.text = "";
            repeatPasswordInput.text = "";
            return;
        }

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Invalid input");
            return;
        }

        UserManager.Instance.Register(username, password, "");
    }

    private void Hide()
    {
        usernameInput.text = "";
        passwordInput.text = "";
        repeatPasswordInput.text = "";
        gameObject.SetActive(false);
    }
}
