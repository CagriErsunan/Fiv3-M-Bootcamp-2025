using UnityEngine;
using System.Collections.Generic;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private List<int> usedIndices = new List<int>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Boþ bir spawn noktasý bulur. Yoksa index 0'ý döner.
    /// </summary>
    public Transform GetFreeSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPointManager: No spawn points assigned!");
            return null;
        }

        // Boþ olanlarý bul
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedIndices.Contains(i))
            {
                usedIndices.Add(i);
                return spawnPoints[i];
            }
        }

        Debug.LogWarning("SpawnPointManager: All points used, reusing index 0.");
        return spawnPoints[0];
    }

    /// <summary>
    /// Yeni round veya sahne baþýnda resetlemek için
    /// </summary>
    public void ResetSpawnPoints()
    {
        usedIndices.Clear();
    }
}
