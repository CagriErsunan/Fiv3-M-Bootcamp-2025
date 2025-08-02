using UnityEngine;
using Unity.Netcode;

public class GroundEliminator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu tag kontrolü
        if (!other.CompareTag("Player")) return;

        // Oyuncunun Network tarafı sadece server'da işlenmeli
        if (!NetworkManager.Singleton.IsServer) return;

        // PlayerController var mı kontrol et
        var controller = other.GetComponent<PlayerController>();
        if (controller != null && !controller.isEliminatedTepsi.Value)
        {
            controller.Eliminate();
            Debug.Log($"Player {controller.OwnerClientId} eliminated by ZEMIN trigger.");
        }
    }
}
