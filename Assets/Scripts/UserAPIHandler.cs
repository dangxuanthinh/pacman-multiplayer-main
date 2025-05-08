using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UserAPIHandler
{
    /*
        Request {
            endpoint: "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/login"
            method: POST
            header: "Content-Type: application/json"
            content: {"username": <"cuong123">, "password": <"cuong123">}
        }

        Reponse {
            content: {
                "isLoginSuccessful": <1> | <0>
                "message": <"Login successful"> | <"Missing username or password">, <"Username does not exist">, <"Password invalid">
                <"data": {
                    data of user
                }>
            }
        }
    */
    public async Task<LoginSuccessData> SendLoginRequest(string username, string password)
    {
        LoginData loginData = new LoginData(username, password);
        string jsonData = JsonUtility.ToJson(loginData);

        string apiEndpoint = "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/login";
        UnityWebRequest request = UnityWebRequest.Put(apiEndpoint, jsonData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            LoginServerResponse response = JsonUtility.FromJson<LoginServerResponse>(responseText);

            if (response.isLoginSuccessful == 1)
            {
                Debug.Log("Login successful, retrieved data: " + response.data);
                LoginSuccessData loginSuccessData = new LoginSuccessData()
                {
                    username = username,
                    data = response.data
                };
                UIPopup.Instance.HidePopup();
                return loginSuccessData;
            }
            else
            {
                Debug.LogError("Login failed! response from server: " + response.message);
                UIPopup.Instance.OpenPopup($"{response.message}", null, null, true, false);
                return null;
            }
        }
        else
        {
            Debug.LogError(request.error);
            Debug.LogError(request.responseCode);
            return null;
        }
    }

    public class LoginSuccessData
    {
        public string username;
        public string data;
    }


    [System.Serializable]
    private class LoginData
    {
        public string username;
        public string password;

        public LoginData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }

    [System.Serializable]
    private class LoginServerResponse
    {
        public int isLoginSuccessful;
        public string message;
        public string data;
    }


    /*
        Request {
            endpoint: "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/updateData"
            method: POST
            header: "Content-Type: application/json"
            content: {"username": <"cuong123">, "data": <{new_data}>}
        }

        Reponse {
            content: {
                "isValid": <1> | <0>
                "message": <"Update successful"> | <"Missing username or newData in the request">, <Username dose not exist>
            }
        }
    */
    public async Task SendUpdateRequest(string username, string data)
    {
        UpdateData updateData = new UpdateData(username, data);
        string jsonData = JsonUtility.ToJson(updateData);

        string apiEndpoint = "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/updateData";
        UnityWebRequest request = UnityWebRequest.Put(apiEndpoint, jsonData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            ServerResponse response = JsonUtility.FromJson<ServerResponse>(responseText);

            if (response.isValid == 1)
            {
                Debug.Log("Data saved successfully!");
            }
            else
            {
                Debug.LogError("Failed to save data! Response from server: " + response.message);
            }
        }
        else
        {
            Debug.LogError(request.error);
            Debug.LogError(request.responseCode);
        }
    }

    [System.Serializable]
    private class UpdateData
    {
        public string username;
        public string data;

        public UpdateData(string username, string data)
        {
            this.username = username;
            this.data = data;
        }
    }

    [System.Serializable]
    private class ServerResponse
    {
        public int isValid;
        public string message;
    }

    /*
        Request {
            endpoint: "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/register"
            method: POST
            header: "Content-Type: application/json"
            content: {"username": <"cuong123">, "password": <"cuong123">, "data": <{initial_data}>}
        }

        Reponse {
            content: {
                "isCreateSuccessful": <1> | <0>
                "message": <"Account created successfully"> | <"Missing username or password">, <"Username already exists">
            }
        }
    */
    public async Task<bool> SendRegisterRequest(string username, string password, string data)
    {
        RegisterData registerData = new RegisterData(username, password, data);
        string jsonData = JsonUtility.ToJson(registerData);

        string apiEndpoint = "https://gaw91x58y5.execute-api.ap-southeast-1.amazonaws.com/product/register";
        UnityWebRequest request = UnityWebRequest.Put(apiEndpoint, jsonData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            RegisterServerResponse response = JsonUtility.FromJson<RegisterServerResponse>(responseText);

            if (response.isCreateSuccessful == 1)
            {
                Debug.Log("Register account complete!");
                UIPopup.Instance.HidePopup();
                return true;
            }
            else
            {
                Debug.LogError("Register account faled! Response server: " + response.message);
                UIPopup.Instance.OpenPopup($"{response.message}", null, null, true, false);
                return false;
            }
        }
        else
        {
            Debug.LogError(request.error);
            Debug.LogError(request.responseCode);
            return false;
        }
    }

    [System.Serializable]
    private class RegisterData
    {
        public string username;
        public string password;
        public string data;

        public RegisterData(string username, string password, string data)
        {
            this.username = username;
            this.password = password;
            this.data = data;
        }
    }

    [System.Serializable]
    private class RegisterServerResponse
    {
        public int isCreateSuccessful;
        public string message;
    }
}
