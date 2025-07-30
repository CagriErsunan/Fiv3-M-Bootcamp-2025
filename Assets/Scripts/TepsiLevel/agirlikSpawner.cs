using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class agirlikSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> agirlikPrefabs; // List of prefabs to spawn
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns
    [SerializeField] private float spawnRange = 5f;
    [SerializeField] private float spawnHeight = 15f;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(SpawnObjectsRoutine());
    }

    private IEnumerator SpawnObjectsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Vector2 randomPoint = Random.insideUnitCircle * spawnRange;
            Vector3 spawnPosition = new Vector3(randomPoint.x, spawnHeight, randomPoint.y);

            GameObject selectedPrefab = agirlikPrefabs[Random.Range(0, agirlikPrefabs.Count)];
            //Debug.Log("Spawning object: " + selectedPrefab.name + " at position: " + spawnPosition);
            // 1. Objeyi SADECE SERVER'da yerel olarak yarat.
            GameObject newObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            
            NetworkObject networkObject = newObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
            }
            else
            {
                Debug.LogError("Spawn edilen prefab NetworkObject içermiyor: " + selectedPrefab.name);
            }
        }
    }

}
