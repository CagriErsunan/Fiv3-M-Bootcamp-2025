using UnityEngine;
using Unity.Netcode;
//using Cinemachine; // This might be a leftover, can be removed if not using Cinemachine

public class CameraFollow : MonoBehaviour
{
    // The target is private because this script will find it itself.
    private Transform target;

    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -15);
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float cameraTiltX = 15.0f;

    void LateUpdate()
    {
        // --- THE FIX ---
        // If we don't have a target, try to find the local player.
        if (target == null)
        {
            // Check if the network is active and if the local client's player object exists.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                // If it exists, assign it as our target.
                target = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            }
            else
            {
                // If we can't find it yet, just wait until the next frame.
                return;
            }
        }

        // If we have a target, proceed with the follow logic.
        Vector3 desiredPosition = target.position + cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        Quaternion tilt = Quaternion.Euler(cameraTiltX, 0, 0);
        transform.rotation = targetRotation * tilt;
    }
}