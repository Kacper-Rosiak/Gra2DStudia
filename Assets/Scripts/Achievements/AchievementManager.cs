using System;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager
{
    private readonly List<AchievementDefinition> _database;
    private Dictionary<string, AchievementProgress> _progressMap;

    public event Action<AchievementDefinition> OnAchievementUnlocked;

    public AchievementManager(List<AchievementDefinition> database, Dictionary<string, AchievementProgress> savedProgress = null)
    {
        _database = database;
        _progressMap = savedProgress ?? new Dictionary<string, AchievementProgress>();

        foreach (var ach in _database)
        {
            if (!_progressMap.ContainsKey(ach.id))
            {
                _progressMap[ach.id] = new AchievementProgress(ach.id);
            }
        }

        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnBossKilled += HandleBossKilled;
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnItemCrafted += HandleItemCrafted;
        GameEvents.OnPotionBrewed += HandlePotionBrewed;
        GameEvents.OnItemSold += HandleItemSold;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
        GameEvents.OnLocationDiscovered += HandleLocationDiscovered;
        GameEvents.OnCombatWon += HandleCombatWon;
        GameEvents.OnLevelReached += HandleLevelReached;
        GameEvents.OnCustomTrigger += HandleCustomTrigger;
    }

    public void Destroy()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnBossKilled -= HandleBossKilled;
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnItemCrafted -= HandleItemCrafted;
        GameEvents.OnPotionBrewed -= HandlePotionBrewed;
        GameEvents.OnItemSold -= HandleItemSold;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
        GameEvents.OnLocationDiscovered -= HandleLocationDiscovered;
        GameEvents.OnCombatWon -= HandleCombatWon;
        GameEvents.OnLevelReached -= HandleLevelReached;
        GameEvents.OnCustomTrigger -= HandleCustomTrigger;
    }

    private void HandleEnemyKilled() => AddProgress(AchievementCategory.KillCount, 1);
    private void HandleBossKilled() => AddProgress(AchievementCategory.BossKills, 1);
    private void HandlePlayerDeath() => AddProgress(AchievementCategory.DeathCount, 1);
    private void HandleItemCrafted() => AddProgress(AchievementCategory.CraftCount, 1);
    private void HandlePotionBrewed() => AddProgress(AchievementCategory.PotionCount, 1);
    private void HandleItemSold() => AddProgress(AchievementCategory.ItemsSold, 1);
    private void HandleQuestCompleted() => AddProgress(AchievementCategory.QuestsDone, 1);
    private void HandleLocationDiscovered() => AddProgress(AchievementCategory.LocationsFound, 1);
    private void HandleCombatWon() => AddProgress(AchievementCategory.CombatsWon, 1);
    private void HandleLevelReached(int level) => SetProgress(AchievementCategory.LevelReach, level);

    private void HandleCustomTrigger(string customId)
    {
        var specificAchievement = _database.FirstOrDefault(a => a.type == AchievementCategory.Custom && a.id == customId);
        if (specificAchievement != null)
        {
            var progress = _progressMap[specificAchievement.id];
            if (!progress.isUnlocked)
            {
                progress.currentValue = specificAchievement.requiredValue;
                progress.isUnlocked = true;
                OnAchievementUnlocked?.Invoke(specificAchievement);
            }
        }
    }

    private void AddProgress(AchievementCategory type, int amount)
    {
        foreach (var data in _database.Where(a => a.type == type))
        {
            var progress = _progressMap[data.id];
            if (progress.isUnlocked) continue;

            progress.currentValue += amount;
            CheckCompletion(data, progress);
        }
    }

    private void SetProgress(AchievementCategory type, int newValue)
    {
        foreach (var data in _database.Where(a => a.type == type))
        {
            var progress = _progressMap[data.id];
            if (progress.isUnlocked) continue;

            if (newValue > progress.currentValue)
            {
                progress.currentValue = newValue;
                CheckCompletion(data, progress);
            }
        }
    }

    private void CheckCompletion(AchievementDefinition data, AchievementProgress progress)
    {
        if (progress.currentValue >= data.requiredValue)
        {
            progress.currentValue = data.requiredValue;
            progress.isUnlocked = true;
            OnAchievementUnlocked?.Invoke(data);
        }
    }

    public Dictionary<string, AchievementProgress> GetProgressForSave()
    {
        return _progressMap;
    }
}