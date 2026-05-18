using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public LootRarity chestRarity;

    private bool _playerInRange = false;
    private PlayerManager _playerManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerInRange = true;
            _playerManager = collision.GetComponent<PlayerManager>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }

    private void Update()
    {
        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryOpenChest();
        }
    }

    private void TryOpenChest()
    {
        Debug.Log("[CHEST] Próba otwarcia skrzyni...");
        if (_playerManager == null) 
        {
            Debug.LogError("[CHEST] _playerManager jest NULLEM!");
            return;
        }

        if (_playerManager.Inventory.TryUseKey())
        {
            Debug.Log("[CHEST] Klucz użyty pomyślnie. Otwieranie...");
            OpenChest();
        }
        else
        {
            Debug.Log("[CHEST] Nieudana próba użycia klucza (brak kluczy).");
            if (GenericPopupController.Instance != null)
            {
                GenericPopupController.Instance.ShowPopup("Skrzynia", "Brak klucza potrzebnego do otwarcia skrzynki!");
            }
        }
    }

    private void OpenChest()
    {
        if (ItemFactory.Instance == null) 
        {
            Debug.LogError("[CHEST] ItemFactory.Instance jest NULLEM! Sprawdź czy obiekt ItemFactory jest na scenie.");
            return;
        }

        Debug.Log($"[CHEST] Losowanie przedmiotu o rzadkości: {chestRarity}");
        ItemData lootedItem = ItemFactory.Instance.GenerateLoot(chestRarity);
        string message = "";

        if (lootedItem != null)
        {
            Debug.Log($"[CHEST] Wylosowano: {lootedItem.itemName}. Próba dodania do Inventory...");
            message = $"Znaleziono przedmiot o jakości {chestRarity}:\n\n{lootedItem.itemName}";
            _playerManager.Inventory.AddItem(lootedItem);
            Debug.Log($"<color=green>[CHEST]</color> Sukces! Przedmiot {lootedItem.itemName} został dodany do listy przedmiotów.");
        }
        else
        {
            Debug.LogWarning("[CHEST] Losowanie zwróciło NULL (pusta skrzynia lub błąd tabeli łupów).");
            message = "Skrzynka okazała się pusta...";
        }

        if (GenericPopupController.Instance != null)
        {
            GenericPopupController.Instance.ShowPopup("Skrzynia Otwarta", message, () => {
                Debug.Log("[CHEST] Gracz kliknął OK. Niszczenie obiektu skrzyni.");
                Destroy(gameObject);
            });
        }
        else
        {
            Debug.LogError("[CHEST] GenericPopupController.Instance jest NULLEM!");
            Destroy(gameObject);
        }
    }

    public void Interact()
    {
        TryOpenChest();
    }
}