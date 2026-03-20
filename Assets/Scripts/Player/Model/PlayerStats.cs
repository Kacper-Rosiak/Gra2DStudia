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

    // Dodane statystyki wymagane przez system walki
    public int Speed { get; private set; }
    public int DodgeChance { get; private set; }

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }

    private int xpToNextLevel;

    // KONSTRUKTOR Oparty o Data-Driven Design (wzorzec Strategy/Profil klas)
    public PlayerStats(PlayerClassData classData)
    {
        MaxHP = classData.baseMaxHP;
        CurrentHP = classData.baseMaxHP;
        Attack = classData.baseAttack;
        Defense = classData.baseDefense;
        Speed = classData.baseSpeed;
        DodgeChance = classData.baseDodgeChance;

        Level = 1;
        CurrentXP = 0;

        xpToNextLevel = CalculateXP();
    }

    public void TakeDamage(int damage)
    {
        // USUNIÊTO ODEJMOWANIE PANCERZA. 
        // Obliczenia pancerza (i jego ignorowanie przez magiê) zachodz¹ w logice walki/strategiach.
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

        // Bazowy przyrost statystyk przy awansie na wy¿szy poziom
        MaxHP += 10;
        Attack += 2;
        Defense += 2;
        // Opcjonalnie mo¿na tu równie¿ zwiêkszaæ Speed: Speed += 1;

        CurrentHP = MaxHP; // Pe³ne leczenie przy awansie

        xpToNextLevel = CalculateXP();

        OnLevelUp?.Invoke(Level);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    private int CalculateXP()
    {
        return 100 + Level * 50;
    }
}