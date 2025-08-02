using UnityEngine;

namespace Kart.Items
{
    public class BananaController : MonoBehaviour
    {
        private GameObject owner;

        public void Initialize(GameObject ownerKart)
        {
            owner = ownerKart;
            Destroy(gameObject, 10f); // 10 saniye sonra kaybolsun
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return; // Kendi býrakana etki etmesin

            if (other.CompareTag("Player"))
            {
                Debug.Log("Muz kaydýrdý: " + other.name);

                // Basit kaydýrma efekti: Rigidbody'yi yana savur
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 slipDir = (other.transform.right + Vector3.up).normalized;
                    rb.AddForce(slipDir * 500f);
                }

                Destroy(gameObject); // Kullanýldý
            }
        }
    }
}
