using System;
using UnityEngine;

public class PlayerStats
{
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnLevelUp;
    public event Action OnDeath;
    public event Action OnStatsChanged;

    public int CurrentHP { get; private set; }

    // Base stats (from level/class)
    private int _baseMaxHP;
    private int _baseAttack;
    private int _baseDefense;

    // Bonus stats (from equipment)
    private int _bonusMaxHP;
    private int _bonusAttack;
    private int _bonusDefense;

    // Calculated properties
    public int MaxHP => _baseMaxHP + _bonusMaxHP;
    public int Attack => _baseAttack + _bonusAttack;
    public int Defense => _baseDefense + _bonusDefense;

    public int Speed { get; private set; }
    public int DodgeChance { get; private set; }

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }

    private int xpToNextLevel;

    public PlayerStats(PlayerClassData classData)
    {
        _baseMaxHP = classData.baseMaxHP;
        CurrentHP = _baseMaxHP;
        _baseAttack = classData.baseAttack;
        _baseDefense = classData.baseDefense;
        Speed = classData.baseSpeed;
        DodgeChance = classData.baseDodgeChance;

        Level = 1;
        CurrentXP = 0;

        xpToNextLevel = CalculateXP();
    }

    public void UpdateEquipmentBonuses(int attack, int defense, int maxHP)
    {
        _bonusAttack = attack;
        _bonusDefense = defense;
        _bonusMaxHP = maxHP;

        if (CurrentHP > MaxHP) CurrentHP = MaxHP;

        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        OnStatsChanged?.Invoke();
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            OnDeath?.Invoke();
        }

        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void AddXP(int amount)
    {
        CurrentXP += amount;

        while (CurrentXP >= xpToNextLevel)
        {
            CurrentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;

        // Bazowy przyrost statystyk
        _baseMaxHP += 10;
        _baseAttack += 2;
        _baseDefense += 2;

        CurrentHP = MaxHP;
        xpToNextLevel = CalculateXP();

        // --- WYWOŁANIE TWOJEGO POP-UPA ---
        // Ponieważ ta klasa nie jest MonoBehaviour, używamy pełnej ścieżki do silnika Unity
        var triggers = UnityEngine.Object.FindFirstObjectByType<GameAchievementTriggers>();
        if (triggers != null)
        {
            triggers.TriggerLevelUp();
        }
        // ---------------------------------

        OnLevelUp?.Invoke(Level);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        OnStatsChanged?.Invoke();
    }

    private int CalculateXP()
    {
        return 100 + Level * 50;
    }
}