using UnityEngine;

namespace Kart.Items
{
    [CreateAssetMenu(menuName = "Kart/Items/Boost Item")]
    public class BoostItemSO : KartItemSO
    {
        public float boostForce = 20f; // Hızlandırma kuvveti

        public override void UseItem(GameObject owner)
        {
            Rigidbody rb = owner.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // VelocityChange  anlık hız artışı
                Vector3 boost = owner.transform.forward * boostForce;
                rb.AddForce(boost, ForceMode.VelocityChange);
            }

            Debug.Log("Boost aktif! (particle yok)");
        }
    }
}
