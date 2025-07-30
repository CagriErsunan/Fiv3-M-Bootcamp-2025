using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TepsiCharacterController : NetworkBehaviour
{
   [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A-D veya Sol-Sağ
        float vertical = Input.GetAxisRaw("Vertical");     // W-S veya Yukarı-Aşağı

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.6f);
;
    }

}
