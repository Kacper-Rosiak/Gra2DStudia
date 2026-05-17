using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager
{
    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    
    // Static reference to data being carried between scenes
    public static SaveData CurrentSaveData { get; set; }

    public SaveManager()
    {
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }
    }

    public string GetSavePath(int slot) => Path.Combine(SaveDirectory, $"save_{slot}.json");
    public string GetTempSavePath(int slot) => Path.Combine(SaveDirectory, $"save_{slot}.tmp");

    public bool HasAnySave()
    {
        if (!Directory.Exists(SaveDirectory)) return false;
        return Directory.GetFiles(SaveDirectory, "save_*.json").Length > 0;
    }

    public bool SaveExists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public async Task SaveGameAsync(int slot, SaveData data)
    {
        data.saveSlot = slot;
        data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(slot);
        string tempPath = GetTempSavePath(slot);

        try
        {
            // Safe Save: Write to temp file first
            await File.WriteAllTextAsync(tempPath, json);
            
            // If write is successful, replace the original file
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
            
            Debug.Log($"Gra zapisana pomyślnie w slocie {slot}!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Błąd podczas zapisywania gry: {e.Message}");
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    public async Task<SaveData> LoadGameAsync(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        try
        {
            string json = await File.ReadAllTextAsync(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            CurrentSaveData = data;
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Błąd podczas wczytywania gry: {e.Message}");
            return null;
        }
    }

    // Methods for compatibility with MainMenuController
    public void LoadLatestSave()
    {
        // For now, let's find the one with the newest timestamp
        int newestSlot = -1;
        DateTime newestTime = DateTime.MinValue;

        for (int i = 0; i < 3; i++)
        {
            string path = GetSavePath(i);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path); // Sync for simplicity in this specific call
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (DateTime.TryParse(data.timestamp, out DateTime dt))
                {
                    if (dt > newestTime)
                    {
                        newestTime = dt;
                        newestSlot = i;
                        CurrentSaveData = data;
                    }
                }
            }
        }
        
        if (newestSlot != -1)
        {
            Debug.Log($"Wczytano najnowszy zapis ze slotu {newestSlot}");
        }
    }

    public void InitializeNewGame()
    {
        CurrentSaveData = null; // Clear any pending load data
        Debug.Log("Nowa gra - przygotowanie czystego zapisu...");
    }
}
