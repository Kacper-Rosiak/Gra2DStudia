using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIController : MonoBehaviour
{
    [Header("Shop References")]
    public GameObject shopPanel;
    public Transform itemContainer;
    public GameObject itemPrefab;

    [Header("UI Elements")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI messageText;
    public Button closeButton;

    private ShopManager _shopManager;
    private Inventory _playerInventory;

    public void Initialize(ShopManager shopManager, Inventory playerInventory)
    {
        _shopManager = shopManager;
        _playerInventory = playerInventory;

        _shopManager.OnShopMessage += UpdateMessage;
        _shopManager.OnShopUpdated += RefreshUI;
        _playerInventory.OnGoldChanged += UpdateGold;

        closeButton.onClick.AddListener(() => shopPanel.SetActive(false));

        UpdateGold(_playerInventory.Gold);
        RefreshUI();
    }

    private void RefreshUI()
    {
        // Clear old items
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn new items
        foreach (var item in _shopManager.GetItemsForSale())
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            itemUI.Setup(item, (it) => _shopManager.BuyItem(it));
        }
    }

    private void UpdateGold(int gold)
    {
        goldText.text = "Złoto: " + gold.ToString();
    }

    private void UpdateMessage(string message)
    {
        messageText.text = message;
    }
}
