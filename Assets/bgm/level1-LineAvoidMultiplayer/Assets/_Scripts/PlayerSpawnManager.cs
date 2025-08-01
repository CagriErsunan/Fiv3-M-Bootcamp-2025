using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private SpawnZone spawnZone;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only the server spawns players

        // Spawn players for all connected clients
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        
        // Spawn existing players (if late-joining)
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        Vector3 spawnPosition = spawnZone.GetRandomSpawnPoint();
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.SpawnAsPlayerObject(clientId); // Assign ownership
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
    }
}