using UnityEngine;
using Unity.Netcode;

public class BossProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    private Rigidbody rb;

    // This function runs once when the projectile is spawned on the network.
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        // We only want the server to control the projectile's logic.
        if (!IsServer) return;

        // Give the projectile an initial velocity in the direction it's facing.
        rb.linearVelocity = transform.forward * speed;

        // Destroy the projectile after a few seconds to clean it up.
        Destroy(gameObject, 5f);
    }

    // This function runs ON THE SERVER when the projectile's trigger overlaps with another collider.
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Did we hit a player?
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // If yes, tell that player to take damage.
            player.TakeDamage();
        }

        // Destroy the projectile immediately after it hits something (a player or a wall).
        // We check that it's not another projectile to be safe.
        if (!other.CompareTag("Projectile")) // We'll give our prefab this tag.
        {
            Destroy(gameObject);
        }
    }
}