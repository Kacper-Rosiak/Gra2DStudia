using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private PlayerManager _playerManager;
    private List<Slot> _backpackSlots = new List<Slot>();
    private bool _initialized = false;

    private void Awake()
    {
        if (!_initialized)
        {
            InitializeBackpackSlots();
            SetupEquipmentSlots();
            _initialized = true;
        }
    }

    private void OnEnable()
    {
        // Rejestrujemy się na zdarzenie zmiany sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupForCurrentScene();
    }

    private void OnDisable()
    {
        // Wyrejestrowanie zdarzeń
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wywoływane automatycznie po załadowaniu nowej sceny
        SetupForCurrentScene();
    }

    private void SetupForCurrentScene()
    {
        UnsubscribeFromPlayer(); 
        
        // Zawsze używamy statycznej instancji, która przetrwała zmianę sceny
        _playerManager = PlayerManager.Instance;
        
        if (_playerManager != null)
        {
            _playerManager.Inventory.OnItemAdded += HandleItemChanged;
            _playerManager.Inventory.OnItemRemoved += HandleItemChanged;
            _playerManager.Inventory.OnKeysChanged += HandleKeysChanged; 
            _playerManager.Equipment.OnEquipmentChanged += RefreshUI;
            Debug.Log($"[INVENTORY UI] Pomyślnie podpięto pod instancję gracza: {_playerManager.playerName}");
        }
        
        RefreshUI();
    }

    private void UnsubscribeFromPlayer()
    {
        if (_playerManager != null && _playerManager.Inventory != null)
        {
            _playerManager.Inventory.OnItemAdded -= HandleItemChanged;
            _playerManager.Inventory.OnItemRemoved -= HandleItemChanged;
            _playerManager.Inventory.OnKeysChanged -= HandleKeysChanged; 
            _playerManager.Equipment.OnEquipmentChanged -= RefreshUI;
        }
    }

    private void HandleItemChanged(ItemData item) => RefreshUI();
    private void HandleKeysChanged(int keys) => RefreshUI();

    private void InitializeBackpackSlots()
    {
        if (backpackSlotContainer == null || slotPrefab == null) return;

        int capacity = _playerManager != null ? _playerManager.inventoryCapacity : 24;

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
        if (_playerManager == null || _playerManager.Inventory == null) return;

        List<ItemData> items = _playerManager.Inventory.GetItems();
        // Debug.Log($"[INVENTORY UI] Odświeżanie plecaka. Liczba przedmiotów: {items.Count}");

        if (_backpackSlots.Count == 0) InitializeBackpackSlots();

        for (int i = 0; i < _backpackSlots.Count; i++)
        {
            if (i < items.Count)
            {
                _backpackSlots[i].SetItem(items[i], itemVisualPrefab);
                if (_backpackSlots[i].currentItemVisual != null)
                {
                    ItemView view = _backpackSlots[i].currentItemVisual.GetComponent<ItemView>();
                    if (view != null) view.Setup(items[i], this, tooltipController);
                }
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
            if (unequippedItem != null) _playerManager.Inventory.AddItem(unequippedItem);
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
                if (oldItem != null) _playerManager.Inventory.AddItem(oldItem);
            }
        }
    }
}
