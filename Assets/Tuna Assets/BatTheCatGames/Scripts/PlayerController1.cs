using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController1 : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxSpeedMultiplier = 2f;
    public float speedBoostMultiplier = 1.5f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Verificar se o jogador est� no ch�o
        isGrounded = Physics.Raycast(groundCheck.position, -transform.up, 0.1f, groundLayer);

        // Movimenta��o horizontal
        float moveInput = Input.GetAxisRaw("Horizontal");
        Vector3 moveDirection = new Vector3(moveInput, 0f, 0f) * moveSpeed * GetSpeedMultiplier();
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);

        // Pular
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Aumentar a velocidade
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed *= speedBoostMultiplier;
        }
        else
        {
            moveSpeed /= speedBoostMultiplier;
        }

        Debug.Log("Velocity: " + rb.linearVelocity); // Debug para verificar a velocidade do jogador
    }

    // Limitar a velocidade m�xima do jogador
    float GetSpeedMultiplier()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed * maxSpeedMultiplier)
        {
            return maxSpeedMultiplier;
        }
        return 1f;
    }
}
