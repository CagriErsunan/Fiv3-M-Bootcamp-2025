using UnityEngine;

// This enum will define what slot the item goes into.
public enum CosmeticType { Hat, Pants, Shoes, Tshirt,
    Costume,
    Glasses,
    Face,
    Gloves
}

// This line allows us to create instances of this object from the Asset menu.
[CreateAssetMenu(fileName = "New Cosmetic Item", menuName = "Inventory/Cosmetic Item")]
public class CosmeticItem : ScriptableObject
{
    public string itemName = "New Item";
    public CosmeticType itemType;
    public GameObject itemPrefab; // This will hold the actual mesh prefab.
}