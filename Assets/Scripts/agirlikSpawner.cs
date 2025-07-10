using System.Collections;
using UnityEngine;

public class agirlikSpawner : MonoBehaviour
{
    [SerializeField] private GameObject agirlikPrefab; // Prefab to spawn
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns
    [SerializeField] private float spawnRange = 5f;
    [SerializeField] private float spawnHeight = 15f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnAgirlik());
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private IEnumerator SpawnAgirlik()
    {
        while (true)
        {
            // Generate a random position within the specified range
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnRange, spawnRange),
                spawnHeight,
                Random.Range(-spawnRange, spawnRange)
            );

            // Instantiate the prefab at the random position
            Instantiate(agirlikPrefab, spawnPosition, Quaternion.identity);

            // Wait for the specified interval before spawning the next object
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
