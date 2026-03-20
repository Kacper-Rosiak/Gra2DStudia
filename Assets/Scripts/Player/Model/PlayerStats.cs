using System;
using UnityEngine;


public class PlayerStats
{

    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnLevelUp;
    public event Action OnDeath;

    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }

    private int xpToNextLevel;

    public PlayerStats(int hp, int atk, int def)
    {
        MaxHP = hp;
        CurrentHP = hp;
        Attack = atk;
        Defense = def;

        Level = 1;
        CurrentXP = 0;

        xpToNextLevel = CalculateXP();
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Math.Max(0, damage - Defense);
        CurrentHP -= finalDamage;

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

        MaxHP += 10;
        Attack += 2;
        Defense += 2;

        CurrentHP = MaxHP;

        xpToNextLevel = CalculateXP();

        OnLevelUp?.Invoke(Level);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    private int CalculateXP()
    {
        return 100 + Level * 50;
    }
}

