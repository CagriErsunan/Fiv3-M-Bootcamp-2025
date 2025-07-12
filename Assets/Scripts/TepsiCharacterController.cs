using UnityEngine;
using Photon.Pun;

public class TepsiCharacterController : MonoBehaviour
{

    [SerializeField] private float speed = 5f; // Speed of the character
    private Rigidbody rb;
    private PhotonView photonView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // FixedUpdate is called at a fixed interval and is used for physics calculations
    void FixedUpdate()
    {
        // Check if this PhotonView is owned by the local player
        if (photonView.IsMine)
        {
            // Get input from the user
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Create a movement vector based on input
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            // Apply the movement to the Rigidbody
            rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);
        }
        else
        {
            // If this PhotonView is not owned by the local player, disable the Rigidbody's physics updates
            //rb.isKinematic = true;
            Debug.Log("This character is controlled by another player, physics updates are disabled for this client.");
        }

    }
}
