using Unity.Netcode;
using UnityEngine;

public class PlayerFollowCamera_v1LineA : NetworkBehaviour
{
    public Camera playerCamera;
    public Vector3 cameraOffset = new Vector3(0, 5, -7);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Kamera oluştur
            GameObject camObj = new GameObject("PlayerCamera");
            playerCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
    }

    void LateUpdate()
    {
        if (IsOwner && playerCamera != null)
        {
            playerCamera.transform.position = transform.position + cameraOffset;
            playerCamera.transform.LookAt(transform);
        }
    }
}
