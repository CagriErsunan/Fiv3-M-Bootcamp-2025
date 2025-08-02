using UnityEngine;
using System;

namespace Kart.Items
{
    public class KartInventory : MonoBehaviour
    {
        [Header("Item Durumu")]
        public KartItemSO currentItem;
        public event Action<KartItemSO> OnItemChanged;

        [Header("Kullanım Ayarları")]
        public float useCooldown = 0.3f; // Daha hızlı tepki için kısalttık
        private float lastUseTime = -999f;

        public bool HasItem => currentItem != null;

        private void Update()
        {
            // Oyuncu Space'e bastığında itemi kullan
            if (HasItem && Input.GetKeyDown(KeyCode.Space))
            {
                UseItem();
            }
        }

        /// <summary>
        /// ItemBox'tan item alır
        /// </summary>
        public void ReceiveItem(KartItemSO item)
        {
            if (HasItem) return;

            currentItem = item;
            lastUseTime = Time.time; // yeni item için cooldown sıfırdan başlar
            Debug.Log("Aldığın item: " + item.itemName);

            OnItemChanged?.Invoke(currentItem); // UI güncelle
        }

        /// <summary>
        /// Itemi kullanır ve envanteri boşaltır
        /// </summary>
        public void UseItem()
        {
            if (!HasItem) return;

            // Cooldown kontrolü
            if (Time.time - lastUseTime < useCooldown) return;

            // Item davranışını çalıştır
            currentItem.UseItem(gameObject);

            // Cooldown ve item reset
            lastUseTime = Time.time;
            currentItem = null;

            OnItemChanged?.Invoke(null); // UI temizle
        }
    }
}
