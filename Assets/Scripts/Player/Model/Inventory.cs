using System;
using System.Collections.Generic;

public class Inventory
{
    private List<ItemData> _items = new List<ItemData>();
    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;

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
