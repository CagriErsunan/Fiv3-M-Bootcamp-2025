using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlatformAngleController : MonoBehaviour
{
    public float maxTiltAngle = 45f;

    [Header("Friction Settings")]

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (!IsServer()) return;

        ClampRotation();
    }

    private void ClampRotation()
    {
        Vector3 currentEuler = rb.rotation.eulerAngles;
        currentEuler.x = NormalizeAngle(currentEuler.x);
        currentEuler.z = NormalizeAngle(currentEuler.z);

        float clampedX = Mathf.Clamp(currentEuler.x, -maxTiltAngle, maxTiltAngle);
        float clampedZ = Mathf.Clamp(currentEuler.z, -maxTiltAngle, maxTiltAngle);
        float y = rb.rotation.eulerAngles.y; // Y serbest

        Quaternion clampedRot = Quaternion.Euler(clampedX, y, clampedZ);
        rb.MoveRotation(clampedRot);
    }


    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    private bool IsServer()
    {
        return Unity.Netcode.NetworkManager.Singleton == null || Unity.Netcode.NetworkManager.Singleton.IsServer;
    }
}
