using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class LoadGamePanelController : MonoBehaviour
{
    [Header("Slots")]
    public SaveSlotUI[] saveSlots;
    public Button backButton;

    private SaveManager _saveManager;

    private void Awake()
    {
        _saveManager = new SaveManager();
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
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

            saveSlots[i].Setup(i, data, OnSlotSelected);
        }
    }

    private async void OnSlotSelected(int slot)
    {
        SaveData data = await _saveManager.LoadGameAsync(slot);
        if (data != null)
        {
            Debug.Log($"Wczytywanie sceny: {data.sceneName}");
            // Jeśli sceneName jest pusty (stary zapis), ładujemy Obozowisko (index 1)
            if (string.IsNullOrEmpty(data.sceneName))
            {
                SceneManager.LoadSceneAsync(1);
            }
            else
            {
                SceneManager.LoadSceneAsync(data.sceneName);
            }
        }
    }
}
