using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    private UserAPIHandler apiHandler;

    public string SignedInUserName { get; private set; }

    public bool IsSignedIn => string.IsNullOrEmpty(SignedInUserName) == false;

    public UnityAction<string> OnSignedIn;
    public UnityAction OnSignedOut;
    public UnityAction OnRegistered;

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

        apiHandler = new UserAPIHandler();
    }

    public async void Login(string username, string password)
    {
        try
        {
            UIPopup.Instance.OpenPopup("Logging in...", null, null, false, false);
            var loginSuccessData = await apiHandler.SendLoginRequest(username, password);
            SignedInUserName = loginSuccessData.username;
            string userSaveData = loginSuccessData.data;
            if (string.IsNullOrEmpty(SignedInUserName) == false)
                OnSignedIn?.Invoke(userSaveData);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIPopup.Instance.OpenPopup("Login failed", null, null, true, false);
        }
    }

    public async void Register(string username, string password, string data)
    {
        try
        {
            UIPopup.Instance.OpenPopup("Creating account", null, null, false, false);
            bool success = await apiHandler.SendRegisterRequest(username, password, data);
            if (success)
                OnRegistered?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIPopup.Instance.OpenPopup("Registration failed", null, null, true, false);
        }
    }

    public void SignOut()
    {
        SignedInUserName = string.Empty;
    }

    public async void SaveGame(string data)
    {
        if (string.IsNullOrEmpty(SignedInUserName))
        {
            Debug.LogError("Signed in username is null or empty, user is probably not signed in properly");
            return;
        }
        await apiHandler.SendUpdateRequest(SignedInUserName, data);
    }
}
