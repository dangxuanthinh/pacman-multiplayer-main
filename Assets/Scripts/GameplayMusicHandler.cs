using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayMusicHandler : NetworkBehaviour
{
    void Update()
    {
        if (GameManager.Instance.gameStarted.Value == true)
        {
            HandleMusic();
        }
    }

    private void HandleMusic()
    {
        if (GameManager.Instance.IsAnyGhostFrightened() && GameManager.Instance.gameOver == false)
            AudioManager.Instance.Play("GhostScared");
        else
            AudioManager.Instance.Stop("GhostScared");
    }
}
