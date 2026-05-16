using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AchievementBootstrapper : MonoBehaviour
{
    public static AchievementBootstrapper Instance { get; private set; }

    [Header("Databases")]
    [SerializeField] private List<AchievementDefinition> achievementDatabase;

    public AchievementManager Achievements { get; private set; }

    private string SaveFilePath => Application.persistentDataPath + "/achievements_save.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var savedData = LoadAchievements();
        Achievements = new AchievementManager(achievementDatabase, savedData);
    }

    private void OnApplicationQuit()
    {
        SaveAchievements();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Achievements?.Destroy();
        }
    }

    private void SaveAchievements()
    {
        if (Achievements == null) return;
        var progressList = Achievements.GetProgressForSave().Values.ToList();
        SaveDataWrapper wrapper = new SaveDataWrapper { progressList = progressList };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(SaveFilePath, json);
    }

    private Dictionary<string, AchievementProgress> LoadAchievements()
    {
        if (!File.Exists(SaveFilePath)) return null;
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
            return wrapper.progressList.ToDictionary(p => p.id, p => p);
        }
        catch { return null; }
    }

    [System.Serializable]
    private class SaveDataWrapper
    {
        public List<AchievementProgress> progressList;
    }
}