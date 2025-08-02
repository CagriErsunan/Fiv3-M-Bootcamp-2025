using UnityEngine;

namespace Kart.Items
{
    public class BananaController : MonoBehaviour
    {
        private GameObject owner;

        public void Initialize(GameObject ownerKart)
        {
            owner = ownerKart;
            Destroy(gameObject, 10f); // 10 saniye sonra yok olsun
        }

        private void OnTriggerEnter(Collider other)
        {
            // Kendi muzuna basınca etkilenmez
            if (other.gameObject == owner) return;

            if (other.CompareTag("Player"))
            {
                Debug.Log("Muz kaydırdı: " + other.name);

                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 1️⃣ Hızı azalt (Mario Kart hissi)
                    rb.linearVelocity *= 0.3f;

                    // 2️⃣ Yan kaydırma ve hafif zıplatma
                    Vector3 slipDir = (other.transform.right + Vector3.up * 0.3f).normalized;
                    rb.AddForce(slipDir * 300f, ForceMode.VelocityChange);
                }

                Destroy(gameObject); // Kullanıldı
            }
        }
    }
}
