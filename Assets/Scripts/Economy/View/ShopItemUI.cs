using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public Image itemIcon;
    public Button buyButton;

    private ItemData _itemData;
    private Action<ItemData> _onBuyRequested;

    public void Setup(ItemData item, Action<ItemData> onBuyRequested)
    {
        _itemData = item;
        _onBuyRequested = onBuyRequested;

        itemNameText.text = item.itemName;
        itemPriceText.text = item.price.ToString() + " G";
        if (item.icon != null) itemIcon.sprite = item.icon;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => _onBuyRequested?.Invoke(_itemData));
    }
}
