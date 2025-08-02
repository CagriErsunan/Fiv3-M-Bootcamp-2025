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

            // Roketi ObjectPool'dan al
            GameObject rocket = Kart.Core.ObjectPool.Instance.SpawnFromPool(poolTag, firePoint.position, firePoint.rotation);
            if (rocket == null) return;

            // Roket Controller'� initialize et
            var controller = rocket.GetComponent<RoketController>();
            if (controller != null)
                controller.Initialize(owner);

            // Rigidbody ile h�z ver
            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // linearVelocity yanl��!
                rb.AddForce(firePoint.forward * fireForce, ForceMode.VelocityChange);
            }

            // Roketi belirli s�re sonra yok et
            GameObject.Destroy(rocket, lifeTime);
        }
    }
}
