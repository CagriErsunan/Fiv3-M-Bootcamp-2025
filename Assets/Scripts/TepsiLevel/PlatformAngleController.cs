using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlatformAngleController : MonoBehaviour
{
    public float maxTiltAngle = 45f;

    [Header("Friction Settings")]
    public float normalFriction = 0.6f;
    public float slipperyFriction = 0.05f;
    public float slipStartAngle = 35f;

    private Rigidbody rb;
    private Collider col;
    private PhysicsMaterial dynamicPhysicMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Runtime friction material
        dynamicPhysicMaterial = new PhysicsMaterial("DynamicFrictionMaterial");
        dynamicPhysicMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        dynamicPhysicMaterial.bounciness = 0f;
        col.material = dynamicPhysicMaterial;
    }

    private void FixedUpdate()
    {
        if (!IsServer()) return;

        ClampRotation();
        AdjustFriction();
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

    private void AdjustFriction()
    {
        Vector3 angles = rb.rotation.eulerAngles;
        float tiltMagnitude = Mathf.Max(Mathf.Abs(NormalizeAngle(angles.x)), Mathf.Abs(NormalizeAngle(angles.z)));

        float targetFriction = tiltMagnitude >= slipStartAngle ? slipperyFriction : normalFriction;
        if (Mathf.Abs(dynamicPhysicMaterial.dynamicFriction - targetFriction) > 0.01f)
        {
            dynamicPhysicMaterial.dynamicFriction = targetFriction;
            dynamicPhysicMaterial.staticFriction = targetFriction;
        }
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
