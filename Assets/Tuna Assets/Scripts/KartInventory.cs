using UnityEngine;
using System;

namespace Kart.Items
{
    public class KartInventory : MonoBehaviour
    {
        public KartItemSO currentItem;
        public event Action<KartItemSO> OnItemChanged;

        [Header("Cooldown")]
        public float useCooldown = 1f; // 1 saniye cooldown
        private float lastUseTime = -999f;

        public bool HasItem => currentItem != null;

        public void ReceiveItem(KartItemSO item)
        {
            if (HasItem) return;

            currentItem = item;
            Debug.Log("Aldığın item: " + item.itemName);

            OnItemChanged?.Invoke(currentItem); // UI güncelle
        }

        public void UseItem()
        {
            if (!HasItem) return;

            // 2️⃣ Cooldown kontrolü
            if (Time.time - lastUseTime < useCooldown) return;

            // Item davranışı
            currentItem.UseItem(gameObject);

            // Zamanı kaydet
            lastUseTime = Time.time;

            // Item bitti
            currentItem = null;
            OnItemChanged?.Invoke(null);
        }
    }
}
