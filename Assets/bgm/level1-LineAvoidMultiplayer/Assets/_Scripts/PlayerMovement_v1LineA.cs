using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded;
    private int currentJumps = 0;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private int maxJumps = 1;
    private bool hasTripleJumpBonus = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            currentJumps = 0;
            // Eğer bonus daha önce aktif edilmişse sıfırla
            if (maxJumps == 3 && !hasTripleJumpBonus)
                maxJumps = 1;
        }

        // 🛫 Zıplama
        if (Input.GetKeyDown(KeyCode.Space) && currentJumps < maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            currentJumps++;

            // Eğer 2. zıplamadaysa ve bonus varsa → 3. hakkı tanımla
            if (currentJumps == 2 && hasTripleJumpBonus)
            {
                maxJumps = 3; // oyuncuya 3. zıplama hakkı verilir
            }

            // Eğer 3. zıplama gerçekleştiyse ve bonus kullanıldıysa → bonusu tüket
            if (currentJumps == 3)
            {
                hasTripleJumpBonus = false;
                maxJumps = 1;
            }
        }
    }

    // 🎁 Rakibi düşürünce çalışacak
    [ServerRpc]
    public void GrantTripleJumpBonusServerRpc()
    {
        hasTripleJumpBonus = true;
    }
}
