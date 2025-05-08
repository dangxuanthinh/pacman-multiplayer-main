using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeStopPowerup : PowerUp
{
    public override void Cleanup(Pacman player)
    {
        if (IsServer)
        {
            if (GameManager.Instance.ghosts.Length == 0) return;
            foreach (Ghost ghost in GameManager.Instance.ghosts)
            {
                if (ghost == null) continue;
                ghost.isTimeStopped.Value = false;
            }
        }
    }

    public override void Setup(Pacman player)
    {
        if (IsServer)
        {
            if (GameManager.Instance.ghosts.Length == 0) return;
            foreach (Ghost ghost in GameManager.Instance.ghosts)
            {
                if (ghost == null) continue;
                ghost.isTimeStopped.Value = true;
            }
        }
        AudioManager.Instance.Play("TimeStop");
    }
}
