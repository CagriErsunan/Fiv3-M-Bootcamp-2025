using UnityEngine;

namespace Kart.Items
{
    [CreateAssetMenu(menuName = "Kart/Items/Banana Item")]
    public class BananaItemSO : KartItemSO
    {
        public string poolTag = "Banana";

        public override void UseItem(GameObject owner)
        {
            Vector3 dropPos = owner.transform.position - owner.transform.forward * 2f;
            dropPos.y = 0.1f; // yere yak�n b�rak

            Kart.Core.ObjectPool.Instance.SpawnFromPool(poolTag, dropPos, Quaternion.identity);
        }
    }
}
