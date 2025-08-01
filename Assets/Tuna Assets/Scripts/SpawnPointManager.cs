using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance;

    public Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPoint(int playerIndex)
    {
        return spawnPoints[playerIndex % spawnPoints.Length];
    }
}
