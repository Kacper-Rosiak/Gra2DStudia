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

    // --- TRIGGER METHODS ---
    public static void TriggerEnemyKilled() => OnEnemyKilled?.Invoke();
    public static void TriggerBossKilled() => OnBossKilled?.Invoke();
    public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();
    public static void TriggerItemCrafted() => OnItemCrafted?.Invoke();
    public static void TriggerPotionBrewed() => OnPotionBrewed?.Invoke();
    public static void TriggerItemSold() => OnItemSold?.Invoke();
    public static void TriggerQuestCompleted() => OnQuestCompleted?.Invoke();
    public static void TriggerLocationDiscovered() => OnLocationDiscovered?.Invoke();
    public static void TriggerCombatWon() => OnCombatWon?.Invoke();
    public static void TriggerLevelReached(int level) => OnLevelReached?.Invoke(level);
    public static void TriggerCustomEvent(string customId) => OnCustomTrigger?.Invoke(customId);
}