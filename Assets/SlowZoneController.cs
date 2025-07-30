using UnityEngine;
using Unity.Netcode;

public class SlowZoneController : NetworkBehaviour
{
    // This runs on the SERVER when a player enters the trigger.
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.TakeDamage();
            // We will create this public function on the PlayerController next.
            player.ApplySlowEffect(3.0f);
        }
    }
}