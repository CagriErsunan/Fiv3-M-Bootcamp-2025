using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("[RelayManager] Signed in anonymously.");
        }
    }

    // 1️⃣ Host kurar ve Join Code üretir
    public async Task<string> CreateRelay(int maxConnections = 4)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("[RelayManager] Relay created. JoinCode: " + joinCode);

            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
                .SetRelayServerData(allocation.RelayServer.IpV4,
                                    (ushort)allocation.RelayServer.Port,
                                    allocation.AllocationIdBytes,
                                    allocation.Key,
                                    allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("[RelayManager] CreateRelay failed: " + e.Message);
            return null;
        }
    }

    // 2️⃣ Client join olur
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
                .SetRelayServerData(joinAllocation.RelayServer.IpV4,
                                    (ushort)joinAllocation.RelayServer.Port,
                                    joinAllocation.AllocationIdBytes,
                                    joinAllocation.Key,
                                    joinAllocation.ConnectionData,
                                    joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("[RelayManager] JoinRelay failed: " + e.Message);
            return false;
        }
    }
}
