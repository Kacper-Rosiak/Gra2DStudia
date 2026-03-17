using System.IO;
using UnityEngine;

public class SaveManager
{
    private string saveDirectory;

    public SaveManager()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    public bool HasAnySave()
    {
        if (!Directory.Exists(saveDirectory)) return false;
        string[] files = Directory.GetFiles(saveDirectory);
        return files.Length > 0;
    }

    public void LoadLatestSave()
    {
        Debug.Log("Wczytywanie najnowszego zapisu z dysku...");
    }

    public void InitializeNewGame()
    {
        Debug.Log("Nowa gra - przygotowanie czystego zapisu...");
    }
}