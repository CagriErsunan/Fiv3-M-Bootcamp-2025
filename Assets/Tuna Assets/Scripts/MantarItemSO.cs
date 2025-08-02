using UnityEngine;

namespace Kart.Items
{
    [CreateAssetMenu(menuName = "Kart/Items/Mantar")]
    public class MantarItemSO : KartItemSO
    {
        public float boostForce = 30f;     // �leriye iti� kuvveti
        public float boostDuration = 2f;  // Ka� saniye s�recek

        public override void UseItem(GameObject owner)
        {
            Rigidbody rb = owner.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Hemen ileri do�ru iti� uygula
                Vector3 boostDir = owner.transform.forward;
                rb.AddForce(boostDir * boostForce, ForceMode.VelocityChange);

                // Boost efekti ba�lat
                owner.GetComponent<MonoBehaviour>().StartCoroutine(ResetSpeed(rb, boostDuration));
            }
        }

        private System.Collections.IEnumerator ResetSpeed(Rigidbody rb, float duration)
        {
            yield return new WaitForSeconds(duration);
            rb.linearVelocity *= 0.5f; // H�z�n yar�s�na d���r, ya da direkt eski h�z�na �ekebilirsin
        }
    }
}
