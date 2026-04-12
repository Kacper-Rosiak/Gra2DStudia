using UnityEngine;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject backpackPanel;
    public GameObject equipmentPanel;

    [Header("Prefabs")]
    public GameObject slotPrefab;
    public GameObject itemVisualPrefab;

    [Header("Slots")]
    public Transform backpackSlotContainer;
    public TooltipController tooltipController; // <--- DODANE
    public Slot weaponSlot;
    public Slot helmetSlot;
    public Slot armorSlot;
    public Slot shieldSlot;
    public Slot glovesSlot;
    public Slot bootsSlot;

    [Header("Testing")]
    public List<ItemData> startingItems; // <--- DODANE DO TESTÓW

    private PlayerManager _playerManager;
    private List<Slot> _backpackSlots = new List<Slot>();

    private void Start()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
        
        if (_playerManager == null)
        {
            Debug.LogError("InventoryController: PlayerManager nie znaleziony!");
            return;
        }

        InitializeBackpackSlots();
        SetupEquipmentSlots();
        
        // DODANIE PRZEDMIOTÓW TESTOWYCH
        if (startingItems != null)
        {
            foreach (var item in startingItems)
            {
                if (item != null) _playerManager.Inventory.AddItem(item);
            }
        }

        _playerManager.Inventory.OnItemAdded += (item) => RefreshUI();
        _playerManager.Inventory.OnItemRemoved += (item) => RefreshUI();
        _playerManager.Equipment.OnEquipmentChanged += RefreshUI;

        RefreshUI();
    }

    private void OnEnable()
    {
        // Odśwież widok przy każdym otwarciu okna na klawisz "I"
        if (_playerManager != null) RefreshUI();
    }

    private void InitializeBackpackSlots()
    {
        // Czyścimy stare sloty jeśli istnieją
        foreach (Transform child in backpackSlotContainer) Destroy(child.gameObject);
        _backpackSlots.Clear();

        int capacity = _playerManager.inventoryCapacity;
        for (int i = 0; i < capacity; i++)
        {
            Slot slot = Instantiate(slotPrefab, backpackSlotContainer).GetComponent<Slot>();
            slot.allowedType = null; // Backpack slots allow anything
            _backpackSlots.Add(slot);
        }
    }

    private void SetupEquipmentSlots()
    {
        if (weaponSlot) weaponSlot.allowedType = ItemType.Weapon;
        if (helmetSlot) helmetSlot.allowedType = ItemType.Helmet;
        if (armorSlot) armorSlot.allowedType = ItemType.Chestplate;
        if (shieldSlot) shieldSlot.allowedType = ItemType.Shield;
        if (glovesSlot) glovesSlot.allowedType = ItemType.Gloves;
        if (bootsSlot) bootsSlot.allowedType = ItemType.Boots;
    }

    public void RefreshUI()
    {
        RefreshBackpack();
        RefreshEquipment();
    }

    private void RefreshBackpack()
    {
        List<ItemData> items = _playerManager.Inventory.GetItems();
        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            if (i < items.Count)
            {
                _backpackSlots[i].SetItem(items[i], itemVisualPrefab);
                _backpackSlots[i].currentItemVisual.GetComponent<ItemView>().Setup(items[i], this, tooltipController);
            }
            else
            {
                _backpackSlots[i].ClearSlot();
            }
        }
    }

    private void RefreshEquipment()
    {
        UpdateEquipmentSlot(weaponSlot, ItemType.Weapon);
        UpdateEquipmentSlot(helmetSlot, ItemType.Helmet);
        UpdateEquipmentSlot(armorSlot, ItemType.Chestplate);
        UpdateEquipmentSlot(shieldSlot, ItemType.Shield);
        UpdateEquipmentSlot(glovesSlot, ItemType.Gloves);
        UpdateEquipmentSlot(bootsSlot, ItemType.Boots);
    }

    private void UpdateEquipmentSlot(Slot slot, ItemType type)
    {
        if (slot == null) return;
        ItemData item = _playerManager.Equipment.GetEquippedItem(type);
        if (item != null)
        {
            slot.SetItem(item, itemVisualPrefab);
            slot.currentItemVisual.GetComponent<ItemView>().Setup(item, this, tooltipController);
        }
        else
        {
            slot.ClearSlot();
        }
    }

    public void HandleItemDoubleClick(ItemView itemView)
    {
        ItemData item = itemView.GetData();
        Slot slot = itemView.GetComponentInParent<Slot>();
        
        bool isFromEquipment = slot != null && slot.allowedType != null;

        if (isFromEquipment)
        {
            // Zdejmij sprzęt
            ItemData unequippedItem = _playerManager.Equipment.UnequipItem(item.type);
            if (unequippedItem != null)
            {
                _playerManager.Inventory.AddItem(unequippedItem);
            }
        }
        else
        {
            // Akcja z plecaka
            if (item.type == ItemType.Potion)
            {
                _playerManager.Stats.Heal(item.healAmount);
                _playerManager.Inventory.RemoveItem(item);
                Debug.Log($"Uleczono o {item.healAmount}. Aktualne HP: {_playerManager.Stats.CurrentHP}");
            }
            else
            {
                // Załóż zbroję/broń
                ItemData oldItem = _playerManager.Equipment.EquipItem(item);
                _playerManager.Inventory.RemoveItem(item);
                if (oldItem != null)
                {
                    _playerManager.Inventory.AddItem(oldItem);
                }
            }
        }
    }
}
