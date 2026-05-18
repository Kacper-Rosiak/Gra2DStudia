using System;
using System.Collections.Generic;

public class Inventory
{
    private List<ItemData> _items = new List<ItemData>();
    private int _gold;
    private int _keys;

    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnKeysChanged;

    public int Gold => _gold;
    public int Keys => _keys;

    public Inventory(int startingGold = 0)
    {
        _gold = startingGold;
        _keys = 0;
    }

    public void AddGold(int amount)
    {
        _gold += amount;
        OnGoldChanged?.Invoke(_gold);
    }

    public void AddKeys(int amount)
    {
        _keys += amount;
        OnKeysChanged?.Invoke(_keys);
    }

    public bool TryUseKey()
    {
        if (_keys > 0)
        {
            _keys--;
            OnKeysChanged?.Invoke(_keys);
            return true;
        }
        return false;
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
