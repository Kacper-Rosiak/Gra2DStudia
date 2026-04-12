using System;
using UnityEngine;

public class PlayerStats
{
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnLevelUp;
    public event Action OnDeath;

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
        
        // Ensure HP doesn't exceed new MaxHP, but keep it at its current value if it's lower
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
        
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(int damage)
    {
        // USUNI�TO ODEJMOWANIE PANCERZA. 
        // Obliczenia pancerza (i jego ignorowanie przez magi�) zachodz� w logice walki/strategiach.
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

        // Bazowy przyrost statystyk przy awansie na wyszy poziom
        _baseMaxHP += 10;
        _baseAttack += 2;
        _baseDefense += 2;
        // Opcjonalnie mona tu rwnie zwiksza Speed: Speed += 1;

        CurrentHP = MaxHP; // Pe ne leczenie przy awansie

        xpToNextLevel = CalculateXP();

        OnLevelUp?.Invoke(Level);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }


    private int CalculateXP()
    {
        return 100 + Level * 50;
    }
}