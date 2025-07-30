using UnityEngine;
using Unity.Netcode;

public class TrapController : NetworkBehaviour
{
    // This runs on the SERVER when the hammer's collider hits another collider.
    [System.Obsolete]
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Check if the object we hit has a PlayerController.
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Debug.Log($"Trap hit Player {player.OwnerClientId}. Telling them to respawn.");

            // Call the player's public respawn function.
            player.Respawn();
        }
    }
}