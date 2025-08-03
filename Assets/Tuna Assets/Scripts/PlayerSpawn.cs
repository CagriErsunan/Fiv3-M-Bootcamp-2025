using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Transform spawnPoint = SpawnPointManager.Instance.GetFreeSpawnPoint();
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        Debug.Log($"[PlayerSpawn] Player {OwnerClientId} spawned at {transform.position}");
    }
}
