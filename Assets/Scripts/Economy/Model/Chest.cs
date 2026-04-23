using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public LootRarity chestRarity;

    public void Interact()
    {
        if (ItemFactory.Instance == null) return;

        ItemData lootedItem = ItemFactory.Instance.GenerateLoot(chestRarity);

        if (lootedItem != null)
        {
            Debug.Log($"<color=yellow>[SKRZYNIA {chestRarity}]</color> Znaleziono: {lootedItem.itemName}");
            // Tutaj w przysz³oœci dodasz kod: Inventory.AddItem(lootedItem);
        }
        else
        {
            Debug.Log($"<color=gray>[SKRZYNIA {chestRarity}]</color> Pusto...");
        }

        Destroy(gameObject);
    }
}