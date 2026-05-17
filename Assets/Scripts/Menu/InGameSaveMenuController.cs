using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class InGameSaveMenuController : MonoBehaviour
{
    [Header("Slots")]
    public SaveSlotUI[] saveSlots;

    private SaveManager _saveManager;
    private PlayerManager _playerManager;

    private void Awake()
    {
        _saveManager = new SaveManager();
    }

    private void OnEnable()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveData data = null;
            string path = _saveManager.GetSavePath(i);
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<SaveData>(json);
            }

            // In-game we always allow clicking slots (to overwrite)
            saveSlots[i].Setup(i, data, OnSlotSelected);
            saveSlots[i].slotButton.interactable = true; 
        }
    }

    public void QuitToMainMenu()
    {
        Debug.Log("Powrót do menu głównego...");
        Time.timeScale = 1f; // Upewnij się, że czas płynie normalnie przed zmianą sceny
        SceneManager.LoadScene(0); // Scena 0 to zazwyczaj MenuGlowne
    }

    private async void OnSlotSelected(int slot)
    {
        if (_playerManager == null)
        {
            Debug.LogError("Nie znaleziono PlayerManager!");
            return;
        }

        SaveData data = new SaveData();
        
        // 1. Tożsamość
        data.playerName = _playerManager.playerName;
        data.className = _playerManager.startingClass != null ? _playerManager.startingClass.className : "";

        // 2. Statystyki
        data.level = _playerManager.Stats.Level;
        data.currentXP = _playerManager.Stats.CurrentXP;
        data.currentHP = _playerManager.Stats.CurrentHP;
        data.attack = _playerManager.Stats.Attack;
        data.defense = _playerManager.Stats.Defense;
        data.gold = _playerManager.Inventory.Gold;

        // 3. Ekwipunek (ID przedmiotów)
        data.inventoryItemIDs = _playerManager.Inventory.GetItems().Select(i => i.itemID).ToList();
        
        // Wyposażenie (pobieramy ze wszystkich slotów)
        data.equippedItemIDs = new List<string>();
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemData equipped = _playerManager.Equipment.GetEquippedItem(type);
            if (equipped != null) data.equippedItemIDs.Add(equipped.itemID);
        }

        // 4. Świat
        data.playerPosition = _playerManager.transform.position;
        data.sceneName = SceneManager.GetActiveScene().name;

        // Zapis
        await _saveManager.SaveGameAsync(slot, data);
        
        // Odśwież widok po zapisie
        RefreshSlots();
    }
}
