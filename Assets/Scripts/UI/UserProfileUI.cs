using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserProfileUI : MonoBehaviour
{
    [SerializeField] private Button signOutButton;
    [SerializeField] private TextMeshProUGUI welcomeText;

    private void Start()
    {
        signOutButton.onClick.AddListener(SignOut);
    }

    private void Update()
    {
        string username = UserManager.Instance.SignedInUserName;
        if (string.IsNullOrEmpty(username))
        {
            signOutButton.gameObject.SetActive(false);
            welcomeText.text = string.Empty;
        }
        else
        {
            signOutButton.gameObject.SetActive(true);
            welcomeText.text = $"Welcome {username}";
        }
    }

    private void SignOut()
    {
        UserManager.Instance.SignOut();
    }
}
