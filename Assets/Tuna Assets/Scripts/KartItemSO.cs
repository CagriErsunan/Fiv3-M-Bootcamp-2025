using UnityEngine;

namespace Kart.Items
{
    public abstract class KartItemSO : ScriptableObject
    {
        [Header("Genel Bilgiler")]
        public string itemName;
        public Sprite icon;

        /// <summary>
        /// Bu item kullanýldýðýnda çalýþacak metod
        /// </summary>
        public abstract void UseItem(GameObject owner);
    }
}
