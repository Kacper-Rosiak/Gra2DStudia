using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Identity")]
    public string playerName = "Bohater";

    [Header("Player Profile (Data-Driven)")]
    public PlayerClassData startingClass; // <--- DODANE: Referencja do danych z edytora Unity

    public PlayerStats Stats { get; private set; }
    public IAbilityStrategy CurrentAbility { get; private set; } // <--- DODANE: Aktywna zdolno�� klasy

    

  
    

    private bool isInCombat = false;

    private void Awake()
    {
        

        // 1. Inicjalizacja statystyk na podstawie podpi�tych danych z edytora
        if (startingClass != null)
        {
            Stats = new PlayerStats(startingClass);
            CurrentAbility = CreateStrategyForClass(startingClass.className);
        }
        else
        {
            Debug.LogError("Brak przypisanej klasy postaci w PlayerManager! Przeci�gnij PlayerClassData do Inspektora.");
        }
    }

    private void OnEnable()
    {
        if (Stats != null)
        {
            Stats.OnHealthChanged += HandleHealthChanged;
            Stats.OnLevelUp += HandleLevelUp;
            Stats.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (Stats != null)
        {
            Stats.OnHealthChanged -= HandleHealthChanged;
            Stats.OnLevelUp -= HandleLevelUp;
            Stats.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (isInCombat) return;

    }

    

    // --- NOWE METODY DO OBS�UGI WALK I KLAS ---

    // Funkcja fabrykuj�ca (Factory Method) - przypisuje strategi� do nazwy klasy
    private IAbilityStrategy CreateStrategyForClass(string className)
    {
        switch (className)
        {
            case "Warrior":
                return new WarriorAbilityStrategy();
            case "Mage":
                return new MageAbilityStrategy();
            case "Assassin":
                return new AssassinAbilityStrategy();
            default:
                Debug.LogWarning($"Nie znaleziono strategii dla klasy: {className}");
                return null;
        }
    }

    // Zwraca obiekt gracza przygotowany do walki, wstrzykuj�c mu statystyki i strategi�
    public Player GetCombatEntity(string playerName)
    {
        return new Player(playerName, Stats, CurrentAbility);
    }

    // ------------------------------------------

    private void HandleHealthChanged(int current, int max)
    {
        Debug.Log($"HP: {current}/{max}");
    }

    private void HandleLevelUp(int level)
    {
        Debug.Log($"LEVEL UP! {level}");
    }

    private void HandleDeath()
    {
        Debug.Log("PLAYER DEAD");
    }

    public void TakeDamage(int dmg)
    {
        Stats?.TakeDamage(dmg);
    }

    public void GainXP(int xp)
    {
        Stats?.AddXP(xp);
    }

    public void SetCombatState(bool state)
    {
        isInCombat = state;
    }
}