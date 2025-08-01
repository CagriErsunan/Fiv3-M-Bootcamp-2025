using Unity.Netcode;
using UnityEngine;

public class CompetitionJumpPlayer : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float baseJumpForce = 7f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 3f; // Ray uzunluğu artırıldı

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        // Boşluğa düşerse yeniden doğsun
        if (transform.position.y < -10f)
        {
            Respawn();
        }
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v).normalized;
        Vector3 vel = rb.linearVelocity;
        vel.x = dir.x * moveSpeed;
        vel.z = dir.z * moveSpeed;
        rb.linearVelocity = vel;
    }

    private void Jump()
    {
        Vector3 vel = rb.linearVelocity;
        vel.y = baseJumpForce;
        rb.linearVelocity = vel;
    }

    private bool IsGrounded()
    {
        // Ray başlangıç noktası karakterin biraz altından başlasın
        Vector3 origin = transform.position + Vector3.down * 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance))
        {
            Debug.Log("Zemine temas: " + hit.collider.tag);
            return hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Step");
        }

        Debug.Log("Zemine temas yok.");
        return false;
    }

    private void Respawn()
    {
        transform.position = new Vector3(0, 5, 0); // Doğma pozisyonu (gerekirse değiştir)
        rb.linearVelocity = Vector3.zero;
    }
}
