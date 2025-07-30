using UnityEngine;
using Unity.Netcode;

// This script is now just a component to identify a launch pad and hold its settings.
public class LaunchPad : NetworkBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 25f; // Make public so the player can read it
    [SerializeField] public Transform launchTarget; // Make public so the player can read it

    // We can DELETE the OnTriggerEnter and CooldownRoutine functions entirely.
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Vector3 worldDirection = (launchTarget.position - transform.position).normalized;
            // The LaunchPad's only job is to call the public Launch function.
            player.Launch(worldDirection, launchForce);
        }
    }
    // The gizmo is still very useful for level design.
    private void OnDrawGizmosSelected()
    {
        if (launchTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, launchTarget.position);
            Gizmos.DrawWireSphere(launchTarget.position, 1f);
        }
    }
}