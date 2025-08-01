using Unity.Netcode;
using UnityEngine;

public class BridgeStepSpawner : NetworkBehaviour
{
    [Header("Bridge Settings")]
    [SerializeField] private GameObject woodPlankPrefab;
    [SerializeField] private Transform startPoint;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnBridge();
        }
    }

    void SpawnBridge()
    {
        Vector3 spawnPos = startPoint.position;

        float stepLength = 3f;      // her tahta arası mesafe (Z ekseni)
        int index = 0;

        // 1. İlk 4 normal adım: tek zıplama
        for (int i = 0; i < 4; i++)
        {
            SpawnPlank(spawnPos + Vector3.forward * stepLength * index);
            index += 2; // tahta + boşluk
        }

        // 2. Çift zıplama alanı: boşluk–boşluk–tahta
        index += 2;
        SpawnPlank(spawnPos + Vector3.forward * stepLength * index);
        index += 2;

        // 3. Yine çift zıplama alanı: boşluk–tahta–boşluk
        SpawnPlank(spawnPos + Vector3.forward * stepLength * index);
        index += 2;

        // 4. Triple Jump bölgesi: tahta–boşluk–tahta–boşluk–tahta
        for (int i = 0; i < 3; i++)
        {
            SpawnPlank(spawnPos + Vector3.forward * stepLength * index);
            index += 2; // araya boşluk bırakıyoruz
        }

        // Son 1 tahta (örnek): bu kısmı sen genişletebilirsin
        SpawnPlank(spawnPos + Vector3.forward * stepLength * index);
    }

    void SpawnPlank(Vector3 position)
    {
        GameObject plank = Instantiate(woodPlankPrefab, position, Quaternion.identity);
        plank.GetComponent<NetworkObject>().Spawn();
    }
}
