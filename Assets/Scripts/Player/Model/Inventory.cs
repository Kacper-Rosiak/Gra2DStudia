using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public event Action<int> OnGoldChanged;
    public event Action OnItemsUpdated;

    public int Gold { get; private set; }
    private List<ItemData> items = new List<ItemData>();

    public Inventory(int startingGold)
    {
        Gold = startingGold;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }
        return false;
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        OnItemsUpdated?.Invoke();
        Debug.Log($"Added {item.itemName} to inventory.");
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            OnItemsUpdated?.Invoke();
            Debug.Log($"Removed {item.itemName} from inventory.");
        }
    }

    public List<ItemData> GetItems() => new List<ItemData>(items);
}
