using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager
{
    private List<ItemData> itemsForSale;
    private Inventory playerInventory;

    public event Action<string> OnShopMessage;
    public event Action OnShopUpdated;

    public ShopManager(List<ItemData> initialItems, Inventory inventory)
    {
        itemsForSale = initialItems;
        playerInventory = inventory;
    }

    public void BuyItem(ItemData item)
    {
        if (playerInventory.TrySpendGold(item.price))
        {
            playerInventory.AddItem(item);
            OnShopMessage?.Invoke($"Kupiono: {item.itemName}!");
            OnShopUpdated?.Invoke();
        }
        else
        {
            OnShopMessage?.Invoke("Za mało złota!");
        }
    }

    public void SellItem(ItemData item)
    {
        int sellPrice = Mathf.FloorToInt(item.price * 0.5f);
        playerInventory.AddGold(sellPrice);
        playerInventory.RemoveItem(item);
        OnShopMessage?.Invoke($"Sprzedano: {item.itemName} za {sellPrice} złota!");
        OnShopUpdated?.Invoke();
    }

    public List<ItemData> GetItemsForSale() => new List<ItemData>(itemsForSale);
}
