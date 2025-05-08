using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPacman : MonoBehaviour
{
    public int playerIndex;
    [SerializeField] private List<GameObject> models = new List<GameObject>();
    [SerializeField] private TextMeshPro statusText;
    [SerializeField] private Button kickButton;

    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            KickPlayer();
        });
    }

    private void Start()
    {
        PacmanMultiplayer.Instance.OnPlayerNetworkListChanged += UpdatePlayerVisual;
        CharacterSelectManager.Instance.OnPlayerReadyChanged += UpdatePlayerVisual;
        UpdatePlayerVisual();
    }

    private void OnDestroy()
    {
        PacmanMultiplayer.Instance.OnPlayerNetworkListChanged -= UpdatePlayerVisual;
        CharacterSelectManager.Instance.OnPlayerReadyChanged -= UpdatePlayerVisual;
    }

    public void UpdatePlayerVisual()
    {
        if (PacmanMultiplayer.Instance.IsPlayerConnected(playerIndex))
        {
            PlayerMultiplayerData playerData = PacmanMultiplayer.Instance.GetPlayerMultiplayerData(playerIndex);
            gameObject.SetActive(true);
            statusText.gameObject.SetActive(true);
            SetPlayerModel(playerData.modelIndex);
            if (NetworkManager.Singleton.IsServer)
            {
                kickButton.gameObject.SetActive(!playerData.isServer);
            }
            else
            {
                kickButton.gameObject.SetActive(false);
            }
            if (playerData.isServer)
            {
                statusText.text = $"\n{playerData.playerName}\nHOST";
                return;
            }

            if (CharacterSelectManager.Instance.IsPlayerReady(playerData.clientID))
            {
                statusText.text = $"\n{playerData.playerName}\nREADY";
            }
            else
            {
                statusText.text = $"\n{playerData.playerName}\nNOT READY";
            }
        }
        else
        {
            statusText.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    public void SetPlayerModel(int activeModelIndex)
    {
        for (int i = 0; i < models.Count; i++)
        {
            models[i].gameObject.SetActive(i == activeModelIndex);
        }
    }

    private void KickPlayer()
    {
        PlayerMultiplayerData playerData = PacmanMultiplayer.Instance.GetPlayerMultiplayerData(playerIndex);
        CharacterSelectManager.Instance.NotifiyClientKicked(playerData.clientID);
        PacmanLobby.Instance.KickPlayer(playerData.playerID.ToString());
        PacmanMultiplayer.Instance.KickPlayer(playerData.clientID);
    }
}
