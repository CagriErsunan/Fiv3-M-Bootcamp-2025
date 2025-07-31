using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("");
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetNameServerRpc(LobbyData.PlayerName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        isReady.Value = ready;
    }
}
