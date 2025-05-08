using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ManagersCleanup : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (PacmanMultiplayer.Instance != null)
        {
            Destroy(PacmanMultiplayer.Instance.gameObject);
        }
        if (PacmanLobby.Instance != null)
        {
            Destroy(PacmanLobby.Instance.gameObject);
        }
    }
}
