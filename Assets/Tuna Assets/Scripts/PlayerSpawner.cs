using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;   // Humanoid
    public GameObject kartPrefab;     // Kart yarýþ

    [Header("Scene Settings")]
    public string raceSceneName = "GameScene1"; // Kart sahnesinin adý

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnPlayerForClient(OwnerClientId, SceneManager.GetActiveScene().name);
    }

    private void SpawnPlayerForClient(ulong clientId, string sceneName)
    {
        GameObject prefabToSpawn;

        if (sceneName == raceSceneName)
            prefabToSpawn = kartPrefab;
        else
            prefabToSpawn = playerPrefab;

        GameObject playerObj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
