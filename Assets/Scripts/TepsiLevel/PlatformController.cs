using UnityEngine;
using Unity.Netcode;

public class PlatformController : NetworkBehaviour
{
    public float kaymaGucu = 5f; // Kayma kuvveti (ne kadar büyükse o kadar hızlı kayar)
    
    private Rigidbody rb;

    void Start()
    {
        // Sadece sunucu işlesin
        if (!IsServer) enabled = false;
        
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 1. Aşağıya ışın gönder
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
        {
            // 2. Eğim normalini al
            Vector3 egimNormal = hit.normal;
            
            // 3. Kayma yönünü hesapla (eğimin aşağısı)
            Vector3 kaymaYonu = Vector3.ProjectOnPlane(Vector3.down, egimNormal).normalized;
            
            // 4. Kayma kuvvetini uygula
            rb.AddForce(kaymaYonu * kaymaGucu, ForceMode.Acceleration);
        }
    }
}