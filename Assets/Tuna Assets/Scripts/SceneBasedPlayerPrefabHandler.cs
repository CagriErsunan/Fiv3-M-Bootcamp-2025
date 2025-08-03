using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;

public class SceneBasedPlayerPrefabHandler : MonoBehaviour, IPlayerPrefabHandler
{
    [Header("Prefabs")]
    [SerializeField] private GameObject lobbyPlayerPrefab; // İnsan
    [SerializeField] private GameObject kartPlayerPrefab;  // Kart

    [Header("Spawn Points")]
    [SerializeField] private SpawnPointManager spawnPointManager;

    private void Awake()
    {
        // Tek olsun
        if (FindObjectsOfType<SceneBasedPlayerPrefabHandler>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.AddPlayerPrefabHandler(this);
        }
    }

    /// <summary>
    /// Hangi prefab spawn olacaksa burada belirlenir.
    /// </summary>
    public GameObject GetPlayerPrefab(NetworkManager networkManager, ulong clientId)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Sadece Tuna Scene’de kart
        if (sceneName == "Tuna Scene")
        {
            return kartPlayerPrefab;
        }

        // Diğer sahneler: insan
        return lobbyPlayerPrefab;
    }

    /// <summary>
    /// Spawn edilecek pozisyonu buradan kontrol ediyoruz
    /// </summary>
    public void InstantiatePlayerPrefab(NetworkManager networkManager, ulong clientId, GameObject playerPrefab, out GameObject playerObj)
    {
        // Spawn point al
        Transform spawnPoint = SpawnPointManager.Instance != null
            ? SpawnPointManager.Instance.GetFreeSpawnPoint()
            : null;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        playerObj = Instantiate(playerPrefab, pos, rot);
    }

    public void OnDestroyPlayerPrefab(NetworkManager networkManager, GameObject player)
    {
        Destroy(player);
    }
}
