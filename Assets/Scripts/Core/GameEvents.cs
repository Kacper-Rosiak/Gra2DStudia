using System;

public static class GameEvents
{
    public static event Action OnEnemyKilled;
    public static event Action OnBossKilled;
    public static event Action OnPlayerDeath;
    public static event Action OnItemCrafted;
    public static event Action OnPotionBrewed;
    public static event Action OnItemSold;
    public static event Action OnQuestCompleted;
    public static event Action OnLocationDiscovered;
    public static event Action OnCombatWon;
    public static event Action<int> OnLevelReached;

    public static event Action<string> OnCustomTrigger;
}