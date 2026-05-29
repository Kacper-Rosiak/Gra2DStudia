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

// 1. Dodajemy enum określający, dla kogo jest dany przedmiot
public enum PlayerClass
{
    Warrior, // Wojownik
    Mage     // Mag
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

    [Header("Type & Class Restriction")] // <-- Zaktualizowany nagłówek
    public ItemType type;
    public PlayerClass allowedClass;     // <-- 2. NOWE POLE: Kto może to kupić/nosić

    [Header("Price")]
    public int price;

    [Header("Stats Bonus")]
    public int bonusAttack;
    public int bonusDefense;
    public int bonusMaxHP;

    [Header("Usage (Potions)")]
    public int healAmount;
}