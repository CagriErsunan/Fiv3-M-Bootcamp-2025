using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    // PlayerSpawner.cs içindeki Start() fonksiyonu
    void Start()
    {
        // Sadece bir Photon odasına bağlıysak oyuncu yarat.
        if (PhotonNetwork.InRoom)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f));
            PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Odaya bağlı değilken oyuncu yaratılamaz! Lütfen lobi sahnesinden başlayın.");
        }
    }
}