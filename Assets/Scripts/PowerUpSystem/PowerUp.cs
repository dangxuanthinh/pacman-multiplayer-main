using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class PowerUp : NetworkBehaviour
{
    public string powerupName;
    public float duration;
    public float remainingDuration;
    public Sprite icon;
    protected Pacman player;
    protected FollowAndRotateWithTransform targetTransform;

    public void SetParent(Pacman player)
    {
        this.player = player;
        targetTransform = GetComponent<FollowAndRotateWithTransform>();
        NetworkObject.TrySetParent(player.transform.GetChild(0));
        targetTransform.SetTarget(player.transform);
        player.OnNetworkObjectDespawn += OnPlayerDespawned;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (player)
            player.OnNetworkObjectDespawn -= OnPlayerDespawned;
    }

    private void OnPlayerDespawned()
    {
        if (IsOwner)
        {
            Cleanup(player);
            if (NetworkObject.IsSpawned)
                PowerupManager.Instance.DespawnPowerupObjectServerRpc(NetworkObject);
        }
    }


    public abstract void Setup(Pacman player); // Called at the moment of picking up the powerup
    public abstract void Cleanup(Pacman player); // Called at the end of the powerup
}
