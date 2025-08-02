using UnityEngine;

namespace Kart.Items
{
    public class ItemBox : MonoBehaviour
    {
        public KartItemSO[] possibleItems;
        public float respawnTime = 3f; // Kaç saniye sonra tekrar çıkacak

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            KartInventory inv = other.GetComponent<KartInventory>();
            if (inv == null) return;

            // 1️⃣ Envanter doluysa item vermez, box kaybolmaz
            if (inv.HasItem) return;

            // 2️⃣ Rastgele item ver
            int rand = Random.Range(0, possibleItems.Length);
            inv.ReceiveItem(possibleItems[rand]);

            // 3️⃣ ItemBox kaybolur ve respawn bekler
            gameObject.SetActive(false);
            Invoke(nameof(RespawnBox), respawnTime);
        }

        void RespawnBox()
        {
            gameObject.SetActive(true);
        }
    }
}
