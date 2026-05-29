using NUnit.Framework;
using System.Collections.Generic;
using System;

// ============================================================================
// SEKCKJA 1: KOMPLETNY ZESTAW TESTÓW EKWIPUNKU (NUnit + EditMode)
// ============================================================================
[TestFixture]
public class InventoryUnitTests
{
    private PureInventory _inventory;
    private bool _eventFired;

    [SetUp]
    public void Setup()
    {
        // Setup wykonuje siê przed ka¿dym pojedynczym testem - daje nam "czyst¹ kartê"
        _inventory = new PureInventory(maxSlots: 3, maxWeight: 50f);
        _eventFired = false;

        // Nas³uchiwanie zdarzenia
        _inventory.OnInventoryChanged += () => _eventFired = true;
    }

    // --- TESTY DODAWANIA PRZEDMIOTÓW ---

    [Test]
    public void AddItem_ValidItem_AddedToListAndTriggersEvent()
    {
        // Given
        PureItem sword = new PureItem("Sword_01", 5f);

        // When
        bool success = _inventory.AddItem(sword);

        // Then (Sprawdzenie czy trafi³ na listê i czy wywo³a³ zdarzenie)
        Assert.IsTrue(success, "B£¥D: System powinien zwróciæ true przy poprawnym dodaniu.");
        Assert.IsTrue(_inventory.HasItem("Sword_01"), "B£¥D: Przedmiot nie znajduje siê w pamiêci plecaka.");
        Assert.IsTrue(_eventFired, "B£¥D: Zdarzenie OnInventoryChanged nie zosta³o wywo³ane!");
    }

    // --- TESTY LIMITÓW (UDWIG I MIEJSCE) ---

    [Test]
    public void AddItem_InventoryFull_BlocksAdditionAndReturnsFalse()
    {
        // Given (Zape³niamy plecak do limitu 3 slotów)
        _inventory.AddItem(new PureItem("Item_1", 1f));
        _inventory.AddItem(new PureItem("Item_2", 1f));
        _inventory.AddItem(new PureItem("Item_3", 1f));

        PureItem extraItem = new PureItem("Extra_Item", 1f);

        // When
        bool success = _inventory.AddItem(extraItem);

        // Then
        Assert.IsFalse(success, "B£¥D: System pozwoli³ na dodanie przedmiotu do pe³nego plecaka!");
        Assert.IsFalse(_inventory.HasItem("Extra_Item"), "B£¥D: Nadmiarowy przedmiot trafi³ do listy!");
    }

    [Test]
    public void AddItem_ItemTooHeavy_BlocksAdditionAndReturnsFalse()
    {
        // Given (Limit udŸwigu to 50f)
        PureItem heavyAnvil = new PureItem("Anvil_01", 100f);

        // When
        bool success = _inventory.AddItem(heavyAnvil);

        // Then
        Assert.IsFalse(success, "B£¥D: System nie zablokowa³ przedmiotu, który przekracza udŸwig!");
    }

    // --- TESTY USUWANIA I EDGE CASES ---

    [Test]
    public void RemoveItem_ItemExists_RemovesFromDataAndTriggersEvent()
    {
        // Given
        _inventory.AddItem(new PureItem("Potion_01", 0.5f));
        _eventFired = false; // Resetujemy flagê po dodaniu

        // When
        bool success = _inventory.RemoveItem("Potion_01");

        // Then
        Assert.IsTrue(success, "B£¥D: System nie zwróci³ true przy usuwaniu istniej¹cego przedmiotu.");
        Assert.IsFalse(_inventory.HasItem("Potion_01"), "B£¥D: Przedmiot nadal widnieje w danych!");
        Assert.IsTrue(_eventFired, "B£¥D: Zdarzenie nie zosta³o wywo³ane po usuniêciu przedmiotu.");
    }

    [Test]
    public void RemoveItem_ItemDoesNotExist_DoesNotCrashAndReturnsFalse()
    {
        // Given (Pusty ekwipunek)

        // When & Then (Sprawdzamy czy system powstrzyma crash)
        Assert.DoesNotThrow(() =>
        {
            bool success = _inventory.RemoveItem("Ghost_Item");
            Assert.IsFalse(success, "B£¥D: System zwróci³ true przy usuwaniu przedmiotu widmo.");
        }, "B£¥D: Próba usuniêcia nieistniej¹cego przedmiotu wywo³a³a Crash (Exception)!");
    }

    // --- TESTY WALUTY ---

    [Test]
    public void AddGold_NegativeAmount_IsBlocked()
    {
        // Given
        _inventory.SetGold(100);

        // When
        _inventory.AddGold(-50);

        // Then
        Assert.AreEqual(100, _inventory.Gold, "B£¥D: System pozwoli³ na dodanie ujemnego z³ota!");
    }

    [Test]
    public void BuyItem_SufficientFunds_DeductsFundsCorrectly()
    {
        // Given
        _inventory.SetGold(100);
        int itemCost = 45;

        // When
        bool success = _inventory.BuyItem(itemCost);

        // Then
        Assert.IsTrue(success, "B£¥D: Transakcja powinna siê powieœæ.");
        Assert.AreEqual(55, _inventory.Gold, "B£¥D: System Ÿle odj¹³ z³oto po zakupie.");
    }

    [Test]
    public void BuyItem_InsufficientFunds_BlocksPurchase()
    {
        // Given
        _inventory.SetGold(10);
        int itemCost = 50;

        // When
        bool success = _inventory.BuyItem(itemCost);

        // Then
        Assert.IsFalse(success, "B£¥D: System pozwoli³ na zakup bez wystarczaj¹cych œrodków!");
        Assert.AreEqual(10, _inventory.Gold, "B£¥D: System pobra³ z³oto mimo nieudanej transakcji!");
    }
}

// ============================================================================
// SEKCKJA 2: IZOLOWANA LOGIKA BIZNESOWA EKWIPUNKU (Zgodnoœæ z architektur¹ 5.0)
// ============================================================================

public class PureItem
{
    public string Id { get; private set; }
    public float Weight { get; private set; }

    public PureItem(string id, float weight)
    {
        Id = id;
        Weight = weight;
    }
}

public class PureInventory
{
    private List<PureItem> _items = new List<PureItem>();
    private int _maxSlots;
    private float _maxWeight;

    public int Gold { get; private set; }
    public float CurrentWeight { get; private set; }

    // Zdarzenie wymagane w treœci zadania
    public Action OnInventoryChanged;

    public PureInventory(int maxSlots, float maxWeight)
    {
        _maxSlots = maxSlots;
        _maxWeight = maxWeight;
        Gold = 0;
        CurrentWeight = 0f;
    }

    public bool AddItem(PureItem item)
    {
        // Warunki brzegowe (Edge cases)
        if (_items.Count >= _maxSlots) return false;
        if (CurrentWeight + item.Weight > _maxWeight) return false;

        _items.Add(item);
        CurrentWeight += item.Weight;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(string itemId)
    {
        PureItem itemToRemove = _items.Find(i => i.Id == itemId);

        // Zabezpieczenie przed crashem przy braku przedmiotu
        if (itemToRemove == null) return false;

        _items.Remove(itemToRemove);
        CurrentWeight -= itemToRemove.Weight;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId)
    {
        return _items.Exists(i => i.Id == itemId);
    }

    public void SetGold(int amount)
    {
        if (amount >= 0) Gold = amount;
    }

    public void AddGold(int amount)
    {
        // Zabezpieczenie przed ujemnymi wartoœciami (Edge case)
        if (amount < 0) return;
        Gold += amount;
    }

    public bool BuyItem(int cost)
    {
        if (Gold < cost) return false;

        Gold -= cost;
        return true;
    }
}