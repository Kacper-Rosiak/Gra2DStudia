using UnityEngine;

public enum AchievementCategory
{
    KillCount, LevelReach, BossKills, DeathCount,
    CraftCount, PotionCount, ItemsSold, QuestsDone,
    LocationsFound, CombatsWon, Custom
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Systems/Achievement")]
public class AchievementDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public string description;
    public AchievementCategory type;
    public int requiredValue;
}