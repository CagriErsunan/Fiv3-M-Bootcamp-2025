using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public Vector3 GetRandomSpawnPoint()
    {
        Collider spawnCollider = GetComponent<Collider>();
        if (spawnCollider == null)
        {
            Debug.LogError("SpawnZone must have a Collider!");
            return transform.position;
        }

        // Get a random point inside the collider bounds
        Vector3 randomPoint = new Vector3(
            Random.Range(spawnCollider.bounds.min.x, spawnCollider.bounds.max.x),
            spawnCollider.bounds.center.y,
            Random.Range(spawnCollider.bounds.min.z, spawnCollider.bounds.max.z)
        );

        return randomPoint;
    }
}