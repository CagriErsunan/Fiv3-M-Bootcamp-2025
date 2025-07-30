using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FallingObjectController : NetworkBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float airGravityScale = 0.3f;
    [SerializeField] private float terminalVelocity = 15f;
    [SerializeField] private float airDrag = 0.5f;
    [SerializeField] private float groundDrag = 0.1f;

    [SerializeField] private float despawnDelay = 2f; // Time after landing before despawn

    private Rigidbody rb;
    private NetworkVariable<bool> isGrounded = new NetworkVariable<bool>();

    // Mass categories (set these in inspector per prefab)
    public enum MassCategory { Light, Medium, Heavy }
    public MassCategory massCategory;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (IsServer)
        {
            InitializeMass();
            ConfigureFallingPhysics();
        }
    }

    private void InitializeMass()
    {
        // Set mass based on category (tweak values as needed)
        switch (massCategory)
        {
            case MassCategory.Light:
                rb.mass = 1f;
                airDrag = 0.8f;  // More drag for floaty effect
                break;
            case MassCategory.Medium:
                rb.mass = 5f;
                airDrag = 0.5f;
                break;
            case MassCategory.Heavy:
                rb.mass = 10f;
                airDrag = 0.3f;  // Less drag for faster heavy falls
                break;
        }
    }

    private void ConfigureFallingPhysics()
    {
        rb.useGravity = false; // We'll handle gravity manually
        rb.linearDamping = airDrag;
        rb.angularDamping = 0.5f;
    }

    void FixedUpdate()
    {
        if (!IsServer || isGrounded.Value) return;

        // Apply custom gravity
        rb.AddForce(Vector3.down * 9.81f * airGravityScale, ForceMode.Acceleration);

        // Cap terminal velocity
        if (rb.linearVelocity.y < -terminalVelocity)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                -terminalVelocity,
                rb.linearVelocity.z
            );
}
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetGroundedServerRpc()
    {
        isGrounded.Value = true;
        rb.useGravity = true;
        rb.linearDamping = groundDrag;
        
        // Start despawn countdown
        StartCoroutine(DespawnAfterDelay());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || isGrounded.Value) return;

        if (collision.gameObject.CompareTag("Platform"))
        {
            SetGroundedServerRpc();
        }
    }
    
private IEnumerator DespawnAfterDelay()
{
    yield return new WaitForSeconds(despawnDelay);
    
    if (IsServer)
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject); // Optional: Only needed if not using object pooling
    }
}
}