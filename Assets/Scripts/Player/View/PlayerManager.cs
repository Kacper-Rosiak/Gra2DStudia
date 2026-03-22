using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Profile (Data-Driven)")]
    public PlayerClassData startingClass; // <--- DODANE: Referencja do danych z edytora Unity

    public PlayerStats Stats { get; private set; }
    public IAbilityStrategy CurrentAbility { get; private set; } // <--- DODANE: Aktywna zdolnoœæ klasy

    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim; // DODANE
    private Vector2 movement;

    private bool isInCombat = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. Inicjalizacja statystyk na podstawie podpiêtych danych z edytora
        if (startingClass != null)
        {
            Stats = new PlayerStats(startingClass);
            CurrentAbility = CreateStrategyForClass(startingClass.className);
        }
        else
        {
            Debug.LogError("Brak przypisanej klasy postaci w PlayerManager! Przeci¹gnij PlayerClassData do Inspektora.");
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

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // --- NOWE METODY DO OBS£UGI WALK I KLAS ---

    // Funkcja fabrykuj¹ca (Factory Method) - przypisuje strategiê do nazwy klasy
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

    // Zwraca obiekt gracza przygotowany do walki, wstrzykuj¹c mu statystyki i strategiê
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