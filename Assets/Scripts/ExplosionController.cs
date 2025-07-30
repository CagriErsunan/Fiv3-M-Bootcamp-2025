using UnityEngine;
using Unity.Netcode;

public class ExplosionController : NetworkBehaviour
{
    // This runs on the SERVER when something enters the trigger.
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.TakeDamage();
        }
    }
}