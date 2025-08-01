using UnityEngine;

namespace Kart.Items
{
    public class ItemBox : MonoBehaviour
    {
        public KartItemSO[] possibleItems;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                KartInventory inv = other.GetComponent<KartInventory>();
                if (inv != null && inv.currentItem == null)
                {
                    int rand = Random.Range(0, possibleItems.Length);
                    inv.ReceiveItem(possibleItems[rand]);
                }

                gameObject.SetActive(false);
            }
        }
    }
}
