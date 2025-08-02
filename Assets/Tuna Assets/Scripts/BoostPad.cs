using UnityEngine;

namespace Kart.Items
{
    public class BoostPad : MonoBehaviour
    {
        public float boostForce = 20f; // �tme g�c�
        public float boostDuration = 0.2f; // �tmenin ne kadar s�rece�i

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Arabay� ileri do�ru h�zland�r
                    Vector3 boostDirection = other.transform.forward;
                    rb.linearVelocity = boostDirection * boostForce;
                }
            }
        }
    }
}
