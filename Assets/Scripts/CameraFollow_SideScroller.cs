using UnityEngine;
using Unity.Netcode;

public class CameraFollow_SideScroller : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float xOffset = 3f; // Allows you to shift the player slightly off-center if you want
    [SerializeField] private float smoothSpeed = 0.125f;

    private float lockedYPosition;
    private float lockedZPosition;

    void Start()
    {
        // When the camera starts, lock its Y and Z positions.
        // It will only ever move on the X-axis.
        lockedYPosition = transform.position.y;
        lockedZPosition = transform.position.z;
    }

    void LateUpdate()
    {
        // Keep trying to find the local player.
        if (target == null)
        {
            if (NetworkManager.Singleton?.LocalClient?.PlayerObject != null)
            {
                target = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            }
            return;
        }

        // Calculate the desired X position based on the player's position and our offset.
        float desiredXPosition = target.position.x + xOffset;

        // Create the full desired position vector, using our locked Y and Z values.
        Vector3 desiredPosition = new Vector3(desiredXPosition, lockedYPosition, lockedZPosition);

        // Smoothly move the camera towards the desired position.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply the new position.
        transform.position = smoothedPosition;

        // We don't need LookAt anymore, as the camera rotation is fixed at (0,0,0).
    }
}