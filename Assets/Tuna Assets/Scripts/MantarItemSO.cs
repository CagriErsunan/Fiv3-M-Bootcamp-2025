using UnityEngine;
using System.Collections;

namespace Kart.Items
{
    [CreateAssetMenu(menuName = "Kart/Items/Mantar Item")]
    public class MantarItemSO : KartItemSO
    {
        public float boostForce = 30f;
        public float boostDuration = 2f;

        public override void UseItem(GameObject owner)
        {
            Rigidbody rb = owner.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // İleri yönde hızlandır
                Vector3 boostDir = owner.transform.forward;
                rb.AddForce(boostDir * boostForce, ForceMode.VelocityChange);

                // Boost süresi bitince hızı azalt
                owner.GetComponent<KartInventory>().StartCoroutine(ResetSpeed(rb, boostDuration));
            }
        }

        private IEnumerator ResetSpeed(Rigidbody rb, float duration)
        {
            yield return new WaitForSeconds(duration);
            rb.linearVelocity *= 0.5f; // Hızı yarıya düşür
        }
    }
}
