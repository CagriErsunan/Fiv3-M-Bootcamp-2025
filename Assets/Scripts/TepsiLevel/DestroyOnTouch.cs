using Unity.Netcode;
using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
     private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerGroundCollision playerHealth = other.GetComponent<PlayerGroundCollision>();
           // if (playerHealth != null) playerHealth.Die();
        }
        else if (other.CompareTag("gulle"))
        {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Debug.Log("Server bir gülle tespit etti ve yok ediyor.");
                // Despawn(), objeyi tüm client'lardan kaldırır ve yok eder.
                if (networkObject.IsSpawned)
                {
                    networkObject.Despawn(true);
                }
            }
        }
    }
}
