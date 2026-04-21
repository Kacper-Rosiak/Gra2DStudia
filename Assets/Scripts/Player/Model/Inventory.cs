using System;
using System.Collections.Generic;

public class Inventory
{
    private List<ItemData> _items = new List<ItemData>();
    private int _gold;

    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;
    public event Action<int> OnGoldChanged;

    public int Gold => _gold;

    public Inventory(int startingGold = 0)
    {
        _gold = startingGold;
    }

    public void AddGold(int amount)
    {
        _gold += amount;
        OnGoldChanged?.Invoke(_gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (_gold >= amount)
        {
            _gold -= amount;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }
        return false;
    }

    public void AddItem(ItemData item)
    {
        _items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public void RemoveItem(ItemData item)
    {
        if (_items.Remove(item))
        {
            OnItemRemoved?.Invoke(item);
        }
    }

    public List<ItemData> GetItems() => new List<ItemData>(_items);
}
