using UnityEngine;
using Unity.Netcode;

public class CameraFollow_Orbital : MonoBehaviour
{
    [Header("Target")]
    private Transform target; // The player we will follow

    [Header("Camera Controls")]
    [Tooltip("How far the camera is from the player.")]
    [SerializeField] private float distance = 5.0f;
    [Tooltip("How fast the camera orbits with the mouse.")]
    [SerializeField] private float sensitivity = 3.0f;
    [Tooltip("How high (in degrees) the camera can look up.")]
    [SerializeField] private float yMaxLimit = 80f;
    [Tooltip("How low (in degrees) the camera can look down.")]
    [SerializeField] private float yMinLimit = -40f;

    // Private variables to store the current rotation
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void LateUpdate()
    {
        // --- Step 1: Find the Local Player (if we don't have one) ---
        if (target == null)
        {
            // Check if the network is ready and the local player object exists
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                target = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
                // Set the initial camera rotation so it doesn't snap
                Vector3 angles = transform.eulerAngles;
                currentX = angles.y;
                currentY = angles.x;
            }
            else
            {
                // If we can't find the player yet, wait for the next frame.
                return;
            }
        }

        // --- Step 2: Get Mouse Input ---
        // Get the horizontal and vertical movement of the mouse
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity; // Subtract to invert the Y-axis (standard for orbit cameras)

        // --- Step 3: Clamp the Vertical Rotation ---
        // This prevents the camera from flipping over the top or going under the ground.
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

        // --- Step 4: Calculate the Camera's Position and Rotation ---
        // Create a rotation based on our current angles
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Calculate the camera's position: Start at the target's position, then move backward
        // along the new rotation by the specified distance.
        Vector3 position = target.position - (rotation * Vector3.forward * distance);

        // --- Step 5: Apply the new Position and Rotation ---
        transform.position = position;
        transform.rotation = rotation;
    }
}