using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRingPowerup : PowerUp
{

    public override void Cleanup(Pacman player)
    {
        AudioManager.Instance.Stop("FireRingPowerup");
    }

    public override void Setup(Pacman player)
    {
        AudioManager.Instance.Play("FireRingPowerup");
    }
}
