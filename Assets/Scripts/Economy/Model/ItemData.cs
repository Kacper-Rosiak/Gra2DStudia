using UnityEngine;

public enum ItemType
{
    Helmet,
    Chestplate,
    Gloves,
    Weapon,
    Shield,
    Boots,
    Potion
}

[CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Visuals")]
    public Sprite icon;

    [Header("Type")]
    public ItemType type;

    [Header("Stats Bonus")]
    public int bonusAttack;
    public int bonusDefense;
    public int bonusMaxHP;

    [Header("Usage (Potions)")]
    public int healAmount;
}
