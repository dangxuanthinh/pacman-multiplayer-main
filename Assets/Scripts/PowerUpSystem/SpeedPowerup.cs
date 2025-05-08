using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPowerup : PowerUp
{
    public override void Cleanup(Pacman player)
    {
        if (player != null)
        {
            player.movementSpeed = player.defaultMovementSpeed;
        }
    }

    public override void Setup(Pacman player)
    {
        if (player != null)
        {
            player.movementSpeed = player.movementSpeed * 1.5f;
            AudioManager.Instance.Play("SpeedBoost");
        }
    }
}
