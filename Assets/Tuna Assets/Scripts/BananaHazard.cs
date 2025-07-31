using UnityEngine;

public class BananaHazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Basit spin efekti
                rb.linearVelocity *= 0.2f;
                rb.AddTorque(Vector3.up * 500f);
            }
            Destroy(gameObject);
        }
    }
}
