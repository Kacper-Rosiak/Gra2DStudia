using UnityEngine;
using System.Collections.Generic;

// Ograniczenie do trzech wymaganych poziomów rzadkoœci
public enum LootRarity { Common, Rare, Epic }

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance { get; private set; }

    [Header("Loot Tables by Rarity")]
    public LootTable commonTable;
    public LootTable rareTable;
    public LootTable epicTable;

    [Header("Item Database")]
    public List<ItemData> allItemsDatabase;
    private Dictionary<string, ItemData> _itemRegistry;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _itemRegistry = new Dictionary<string, ItemData>();
        foreach (var item in allItemsDatabase)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemID))
            {
                _itemRegistry[item.itemID] = item;
            }
        }
    }

    public ItemData GenerateLoot(LootRarity rarity)
    {
        // Wybór tabeli z nowej, wê¿szej puli
        LootTable activeTable = rarity switch
        {
            LootRarity.Epic => epicTable,
            LootRarity.Rare => rareTable,
            LootRarity.Common => commonTable,
            _ => commonTable
        };

        if (activeTable == null) return null;

        string rolledItemId = activeTable.RollLoot();
        if (string.IsNullOrEmpty(rolledItemId)) return null;

        if (_itemRegistry.TryGetValue(rolledItemId, out ItemData rolledItem))
        {
            return rolledItem;
        }

        Debug.LogWarning($"Przedmiot o ID {rolledItemId} nie zosta³ znaleziony!");
        return null;
    }
}