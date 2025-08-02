using UnityEngine;

namespace Kart.Items
{
    [CreateAssetMenu(menuName = "Kart/Items/Rocket Item")]
    public class RocketItemSO : KartItemSO
    {
        public string poolTag = "Rocket"; // ObjectPool tag
        public float fireForce = 20f;
        public float lifeTime = 5f;

        public override void UseItem(GameObject owner)
        {
            Transform firePoint = owner.transform.Find("FirePoint");
            if (firePoint == null) return;

            var rocket = Kart.Core.ObjectPool.Instance.SpawnFromPool(poolTag, firePoint.position, firePoint.rotation);
            if (rocket == null) return;

            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(firePoint.forward * fireForce, ForceMode.VelocityChange);
            }
        }
    }
}
