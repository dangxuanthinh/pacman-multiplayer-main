using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(PacmanMultiplayer.MAX_PLAYER - 1, "asia-southeast1");
            return relayAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public async Task<string> GenerateRelayCode(Allocation relayAllocation)
    {
        try
        {
            string code = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);
            return code;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return "";
        }
    }

    public async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }
}
