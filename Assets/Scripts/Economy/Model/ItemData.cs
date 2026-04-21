using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Economy/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int price;
    
    [Header("Stats (Optional)")]
    public int hpBonus;
    public int attackBonus;
    public int defenseBonus;
}
