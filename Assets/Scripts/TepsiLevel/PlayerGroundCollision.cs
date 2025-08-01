using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerGroundCollision : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // Sadece server bu kontrolü yapar

        if (SceneManager.GetActiveScene().name != "level_cambaz_tepsisi")
            return;
            
        if (collision.gameObject.CompareTag("Zemin"))
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null && !controller.isEliminated.Value)
            {
                controller.Eliminate();
                Debug.Log($"Player {OwnerClientId} eliminated by touching ground.");
            }
        }
    }
}
