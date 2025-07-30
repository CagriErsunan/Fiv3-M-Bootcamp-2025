using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Cosmetic Database", menuName = "Inventory/Cosmetic Database")]
public class CosmeticDatabase : ScriptableObject
{
    public List<CosmeticItem> allHats;
    public List<CosmeticItem> allPants;
    public List<CosmeticItem> allShoes;
    public List<CosmeticItem> allTshirts;
    public List<CosmeticItem> allCostumes;
    public List<CosmeticItem> allGlasses;
    public List<CosmeticItem> allFaces;
    public List<CosmeticItem> allGloves;
}