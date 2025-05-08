using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerupPickup : NetworkBehaviour
{
    public PowerUp powerupPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner && other.CompareTag("Player"))
        {
            PickupPowerupServerRpc(powerupPrefab.powerupName, other.GetComponent<NetworkObject>());
            other.GetComponent<Pacman>().OnPowerupEaten(powerupPrefab);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupPowerupServerRpc(string powerupName, NetworkObjectReference playerNetworkReference)
    {
        playerNetworkReference.TryGet(out var playerNetworkObject);
        PowerupManager.Instance.SpawnPowerupObject(powerupName, playerNetworkObject.gameObject);
        PowerupManager.Instance.RemovePowerupPickup(this);
    }
}
