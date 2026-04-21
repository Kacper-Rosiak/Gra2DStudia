using UnityEngine;
using System.Collections.Generic;

public class ShopController : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private List<ItemData> initialShopItems;
    [SerializeField] private int startingPlayerGold = 100;

    [Header("UI Reference")]
    [SerializeField] private ShopUIController uiController;

    private ShopManager _shopManager;
    private Inventory _playerInventory;

    void Start()
    {
        // For now, create a local inventory. 
        // In a real game, this would come from a Global PlayerManager.
        _playerInventory = new Inventory(startingPlayerGold);
        
        _shopManager = new ShopManager(initialShopItems, _playerInventory);

        if (uiController != null)
        {
            uiController.Initialize(_shopManager, _playerInventory);
        }
        else
        {
            Debug.LogError("ShopController: Nie przypisano ShopUIController!");
        }
    }

    // This could be called by an NPC or a Trigger
    public void OpenShop()
    {
        if (uiController != null && uiController.shopPanel != null)
        {
            uiController.shopPanel.SetActive(true);
        }
    }
}
