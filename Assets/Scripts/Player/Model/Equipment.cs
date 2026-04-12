using System;
using System.Collections.Generic;

public class Equipment
{
    private Dictionary<ItemType, ItemData> _equippedItems = new Dictionary<ItemType, ItemData>();
    private PlayerStats _stats;

    public event Action OnEquipmentChanged;

    public Equipment(PlayerStats stats)
    {
        _stats = stats;
    }

    public ItemData EquipItem(ItemData item)
    {
        ItemData oldItem = null;
        if (_equippedItems.ContainsKey(item.type))
        {
            oldItem = _equippedItems[item.type];
        }

        _equippedItems[item.type] = item;
        UpdateStats();
        OnEquipmentChanged?.Invoke();
        return oldItem;
    }

    public ItemData UnequipItem(ItemType type)
    {
        if (_equippedItems.ContainsKey(type))
        {
            ItemData item = _equippedItems[type];
            _equippedItems.Remove(type);
            UpdateStats();
            OnEquipmentChanged?.Invoke();
            return item;
        }
        return null;
    }

    public ItemData GetEquippedItem(ItemType type)
    {
        return _equippedItems.ContainsKey(type) ? _equippedItems[type] : null;
    }

    private void UpdateStats()
    {
        int attack = 0;
        int defense = 0;
        int maxHP = 0;

        foreach (var item in _equippedItems.Values)
        {
            attack += item.bonusAttack;
            defense += item.bonusDefense;
            maxHP += item.bonusMaxHP;
        }

        _stats.UpdateEquipmentBonuses(attack, defense, maxHP);
    }
}
