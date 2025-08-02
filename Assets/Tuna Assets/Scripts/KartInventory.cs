using UnityEngine;
using System;

namespace Kart.Items
{
    public class KartInventory : MonoBehaviour
    {
        public KartItemSO currentItem;
        public event Action<KartItemSO> OnItemChanged; // UI'ye haber verir

        public void ReceiveItem(KartItemSO item)
        {
            if (currentItem == null)
            {
                currentItem = item;
                Debug.Log("Aldığın item: " + item.itemName);

                OnItemChanged?.Invoke(currentItem); // UI'yi güncelle
            }
        }

        public void UseItem()
        {
            if (currentItem == null) return;

            // Item davranışını ScriptableObject'ten çalıştır
            currentItem.UseItem(gameObject);

            // Item bitti
            currentItem = null;
            OnItemChanged?.Invoke(null); // UI temizlensin
        }
    }
}
