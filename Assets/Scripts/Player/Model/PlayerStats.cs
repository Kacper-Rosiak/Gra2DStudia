using System;
using UnityEngine;

public class PlayerStats
{
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnLevelUp;
    public event Action OnDeath;
    public event Action OnStatsChanged;

    // --- NOWY EVENT: Żeby UI sklepu albo HUD wiedziały, że zmieniła się ilość kasy ---
    public event Action<int> OnGoldChanged;
    // ---------------------------------------------------------------------------------

    public int CurrentHP { get; private set; }

    // --- NOWA WŁAŚCIWOŚĆ: Portfel gracza ---
    public int Zloto { get; private set; }
    // ---------------------------------------

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
    public int XPToNextLevel => xpToNextLevel;
    public int PendingStatPoints { get; set; }

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

        // --- NOWOŚĆ: Startujesz z zerem złota (lub zmień na ile chcesz) ---
        Zloto = 0;
        // ------------------------------------------------------------------

        xpToNextLevel = CalculateXP();

        // --- NOWOŚĆ: Podpinamy portfel pod system misji przy narodzinach statystyk ---
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestProgressChanged += SprawdzNagrodeZaMisje;
        }
    }

    // --- NOWA METODA: Automatyczne odbieranie złota po ukończeniu misji ---
    private void SprawdzNagrodeZaMisje(QuestData misja)
    {
        if (misja == null || misja.objectives == null) return;

        bool czyWszystkoUkonczone = true;
        foreach (var cel in misja.objectives)
        {
            if (cel.currentAmount < cel.targetAmount)
            {
                czyWszystkoUkonczone = false;
                break;
            }
        }

        // Jeśli misja została właśnie wbita na 100%, dodaj złoto jako nagrodę!
        if (czyWszystkoUkonczone)
        {
            if (PlayerManager.Instance != null && PlayerManager.Instance.Inventory != null)
            {
                PlayerManager.Instance.Inventory.AddGold(misja.rewardGold);
                Debug.Log($"<color=gold>[PORTFEL]</color> Nagroda za misję '{misja.questName}' odebrana! +{misja.rewardGold}G.");
            }
            else
            {
                // Fallback do starego systemu jeśli Inventory nie istnieje
                AddGold(misja.rewardGold);
            }
        }
    }

    public void AddGold(int amount)
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.Inventory != null)
        {
            PlayerManager.Instance.Inventory.AddGold(amount);
        }
        else
        {
            Zloto += amount;
            OnGoldChanged?.Invoke(Zloto);
            OnStatsChanged?.Invoke();
        }
    }

    public bool SpendGold(int amount)
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.Inventory != null)
        {
            return PlayerManager.Instance.Inventory.TrySpendGold(amount);
        }

        if (Zloto >= amount)
        {
            Zloto -= amount;
            OnGoldChanged?.Invoke(Zloto);
            OnStatsChanged?.Invoke();
            return true; // Zakupy udane
        }
        return false; // Za biedny jesteś
    }
    // ----------------------------------------------------------------------

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

        if (CurrentXP >= xpToNextLevel)
        {
            CurrentXP = 0;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        PendingStatPoints++;

        CurrentHP = MaxHP;
        xpToNextLevel = CalculateXP();

        var triggers = UnityEngine.Object.FindFirstObjectByType<GameAchievementTriggers>();
        if (triggers != null)
        {
            triggers.TriggerLevelUp();
        }

        OnLevelUp?.Invoke(Level);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        OnStatsChanged?.Invoke();

        // POWIADOMIENIE SYSTEMU MISJI
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ZwiekszPostepCelu("Zdobądź 5 poziom doświadczenia");
        }
    }

    public void AddBaseAttack(int amount)
    {
        _baseAttack += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddBaseDefense(int amount)
    {
        _baseDefense += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddBaseMaxHP(int amount)
    {
        _baseMaxHP += amount;
        CurrentHP += amount;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        OnStatsChanged?.Invoke();
    }

    public void LoadStats(int level, int xp, int hp)
    {
        Level = level;
        CurrentXP = xp;

        int levelsGained = Level - 1;
        _baseMaxHP += levelsGained * 10;
        _baseAttack += levelsGained * 2;
        _baseDefense += levelsGained * 2;

        xpToNextLevel = CalculateXP();

        CurrentHP = hp;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;

        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        OnStatsChanged?.Invoke();
    }

    private int CalculateXP()
    {
        return 100 + Level * 50;
    }
}