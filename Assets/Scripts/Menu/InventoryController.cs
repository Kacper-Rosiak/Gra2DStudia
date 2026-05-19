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
    public TMPro.TextMeshProUGUI keysText; 
    public TooltipController tooltipController; 
    public Slot weaponSlot;
    public Slot helmetSlot;
    public Slot armorSlot;
    public Slot shieldSlot;
    public Slot glovesSlot;
    public Slot bootsSlot;

    [Header("Testing (Reference only)")]
    public List<ItemData> startingItems; 

    private PlayerManager _playerManager;
    private List<Slot> _backpackSlots = new List<Slot>();
    private bool _initialized = false;

    private void Awake()
    {
        // Przenosimy inicjalizację do Awake, aby sloty były gotowe przed pierwszym OnEnable
        if (!_initialized)
        {
            InitializeBackpackSlots();
            SetupEquipmentSlots();
            _initialized = true;
        }
    }

    private void OnEnable()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
        
        if (_playerManager == null)
        {
            Debug.LogWarning("InventoryController: PlayerManager nie znaleziony w tej scenie!");
            return;
        }

        // Subskrypcja zdarzeń nowego gracza
        _playerManager.Inventory.OnItemAdded += HandleItemChanged;
        _playerManager.Inventory.OnItemRemoved += HandleItemChanged;
        _playerManager.Inventory.OnKeysChanged += HandleKeysChanged; 
        _playerManager.Equipment.OnEquipmentChanged += RefreshUI;
        
        RefreshUI();
    }

    private void OnDisable()
    {
        if (_playerManager != null)
        {
            // Odpięcie zdarzeń
            _playerManager.Inventory.OnItemAdded -= HandleItemChanged;
            _playerManager.Inventory.OnItemRemoved -= HandleItemChanged;
            _playerManager.Inventory.OnKeysChanged -= HandleKeysChanged; 
            _playerManager.Equipment.OnEquipmentChanged -= RefreshUI;
        }
    }

    private void HandleItemChanged(ItemData item)
    {
        Debug.Log($"[INVENTORY UI] Wykryto zmianę przedmiotu: {item.itemName}. Odświeżam...");
        RefreshUI();
    }

    private void HandleKeysChanged(int keys)
    {
        Debug.Log($"[INVENTORY UI] Wykryto zmianę kluczy: {keys}. Odświeżam...");
        RefreshUI();
    }

    private void InitializeBackpackSlots()
    {
        if (backpackSlotContainer == null || slotPrefab == null) return;

        // Szukamy gracza tymczasowo tylko po to, by znać pojemność plecaka (domyślnie 24)
        var pm = FindFirstObjectByType<PlayerManager>();
        int capacity = pm != null ? pm.inventoryCapacity : 24;

        // Czyścimy stare sloty
        foreach (Transform child in backpackSlotContainer) Destroy(child.gameObject);
        _backpackSlots.Clear();

        for (int i = 0; i < capacity; i++)
        {
            GameObject go = Instantiate(slotPrefab, backpackSlotContainer);
            Slot slot = go.GetComponent<Slot>();
            if (slot != null)
            {
                slot.allowedType = null; 
                _backpackSlots.Add(slot);
            }
        }
        Debug.Log($"[INVENTORY UI] Zainicjalizowano {_backpackSlots.Count} slotów plecaka.");
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
        if (_playerManager == null) return;

        RefreshBackpack();
        RefreshEquipment();
        
        if (keysText != null)
        {
            keysText.text = _playerManager.Inventory.Keys.ToString();
        }
    }

    private void RefreshBackpack()
    {
        if (_playerManager == null) return;

        List<ItemData> items = _playerManager.Inventory.GetItems();
        
        // Zabezpieczenie przed brakiem zainicjalizowanych slotów (np. przy DontDestroyOnLoad)
        if (_backpackSlots.Count == 0) InitializeBackpackSlots();

        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            if (i < items.Count)
            {
                _backpackSlots[i].SetItem(items[i], itemVisualPrefab);
                ItemView view = _backpackSlots[i].currentItemVisual.GetComponent<ItemView>();
                if (view != null) view.Setup(items[i], this, tooltipController);
            }
            else
            {
                _backpackSlots[i].ClearSlot();
            }
        }
    }

    private void RefreshEquipment()
    {
        if (_playerManager == null) return;

        UpdateEquipmentSlot(weaponSlot, ItemType.Weapon);
        UpdateEquipmentSlot(helmetSlot, ItemType.Helmet);
        UpdateEquipmentSlot(armorSlot, ItemType.Chestplate);
        UpdateEquipmentSlot(shieldSlot, ItemType.Shield);
        UpdateEquipmentSlot(glovesSlot, ItemType.Gloves);
        UpdateEquipmentSlot(bootsSlot, ItemType.Boots);
    }

    private void UpdateEquipmentSlot(Slot slot, ItemType type)
    {
        if (slot == null || _playerManager == null) return;
        ItemData item = _playerManager.Equipment.GetEquippedItem(type);
        if (item != null)
        {
            slot.SetItem(item, itemVisualPrefab);
            ItemView view = slot.currentItemVisual.GetComponent<ItemView>();
            if (view != null) view.Setup(item, this, tooltipController);
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
            ItemData unequippedItem = _playerManager.Equipment.UnequipItem(item.type);
            if (unequippedItem != null)
            {
                _playerManager.Inventory.AddItem(unequippedItem);
            }
        }
        else
        {
            if (item.type == ItemType.Potion)
            {
                _playerManager.Stats.Heal(item.healAmount);
                _playerManager.Inventory.RemoveItem(item);
            }
            else
            {
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
