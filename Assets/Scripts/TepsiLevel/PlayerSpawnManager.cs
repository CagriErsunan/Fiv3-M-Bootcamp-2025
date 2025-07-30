using Unity.Netcode;
using UnityEngine;

// MonoBehaviour yerine NetworkBehaviour'dan türetmek daha iyi bir pratiktir.
public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GameObject playerPrefab; // Player prefab'ını Inspector'dan buraya sürükle
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    void Start()
    {
        Debug.Log("PlayerSpawnManager başlatıldı.");
    }
    // OnNetworkSpawn, bu obje ağ üzerinde aktif olduğunda çalışır.
    public override void OnNetworkSpawn()
    {
        // Bu script'in mantığını sadece Server'ın çalıştırması gerekir.
        if (!IsServer) return;

        // Bir client bağlandığında OnClientConnected fonksiyonunu çağır.
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Server (Host) ilk başladığında, kendisi için de bir oyuncu yaratmalı.
        // OnClientConnected callback'i Host için de tetiklenir, bu yüzden ekstra bir koda gerek yok.
    }

    private void OnClientConnected(ulong clientId)
    {
        // Spawn noktası kalmadıysa uyarı ver ve başa dön.
        if (nextSpawnIndex >= spawnPoints.Length)
        {
            Debug.LogWarning("Tüm spawn noktaları dolu. Başa dönülüyor.");
            nextSpawnIndex = 0;
        }

        // Doğru spawn noktasını al.
        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        
        // Bir sonraki oyuncu için indeksi artır.
        nextSpawnIndex++;

        // 1. Player prefab'ını DOĞRUDAN spawn noktasında INSTANTIATE et.
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Yaratılan objenin NetworkObject component'ını al ve sahipliğini ilgili client'a vererek SPAWN et.
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        Debug.Log($"Client {clientId} için oyuncu, spawn noktası {nextSpawnIndex-1}'de yaratıldı.");
    }

    public override void OnNetworkDespawn()
    {
        // Obje yok olduğunda callback'i dinlemeyi bırak ki hafızada sızıntı olmasın.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}