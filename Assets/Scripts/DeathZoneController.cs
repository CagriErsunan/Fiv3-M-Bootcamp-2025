using UnityEngine;
using Unity.Netcode;

public class DeathZoneController : MonoBehaviour
{
    // This function runs on the SERVER when an object enters the trigger.
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        // We only want the server to handle this logic.
        if (!NetworkManager.Singleton.IsServer) return;

        // Check if the object that entered was a PLAYER.
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log($"DeathZone hit Player {player.OwnerClientId}. Telling them to respawn.");
                // We use the player's own existing respawn function.
                player.Respawn();
            }
        }
        // Check if the object that entered was an ENEMY.
        else if (other.CompareTag("Enemy"))
        {
            EnemyAIController enemy = other.GetComponent<EnemyAIController>();
            if (enemy != null)
            {
                Debug.Log("DeathZone hit an Enemy. Telling it to respawn.");
                // We will call a new public function on the enemy's script.
                enemy.Respawn();
            }
        }
    }
}