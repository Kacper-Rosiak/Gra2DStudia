using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Identity")]
    public string playerName = "Bohater";

    [Header("Player Profile (Data-Driven)")]
    public PlayerClassData startingClass; // <--- DODANE: Referencja do danych z edytora Unity

    [Header("Save System")]
    public ItemDatabase itemDatabase;

    public PlayerStats Stats { get; private set; }
    public Inventory Inventory { get; private set; }
    public Equipment Equipment { get; private set; }
    public int inventoryCapacity = 24;

    public IAbilityStrategy CurrentAbility { get; private set; } // <--- DODANE: Aktywna zdolno klasy


    

  
    

    private bool isInCombat = false;

    private void Awake()
    {


        // 1. Inicjalizacja statystyk na podstawie podpitych danych z edytora
        if (startingClass != null)
        {
            Stats = new PlayerStats(startingClass);
            Inventory = new Inventory();
            Equipment = new Equipment(Stats);
            CurrentAbility = CreateStrategyForClass(startingClass.className);
        }

        else
        {
            Debug.LogError("Brak przypisanej klasy postaci w PlayerManager! Przecignij PlayerClassData do Inspektora.");
        }
    }

    private void Start()
    {
        // 2. Wczytywanie zapisu jeśli istnieje
        if (SaveManager.CurrentSaveData != null)
        {
            LoadFromSave(SaveManager.CurrentSaveData);
            SaveManager.CurrentSaveData = null; // Czyścimy po wczytaniu
        }
        else
        {
            // Nowa gra - pobierz imię z GameManager jeśli zostało wpisane
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.SelectedPlayerName))
            {
                playerName = GameManager.Instance.SelectedPlayerName;
                Debug.Log($"Nowa gra rozpoczęta jako: {playerName}");
            }
        }
    }

    private void LoadFromSave(SaveData data)
    {
        playerName = data.playerName;
        transform.position = data.playerPosition;

        // Przywracanie statystyk
        Stats.LoadStats(data.level, data.currentXP, data.currentHP);
        Inventory.AddGold(data.gold); // Inventory startuje z 0 gold, więc dodajemy wczytany

        // Przywracanie ekwipunku
        if (itemDatabase != null)
        {
            foreach (string id in data.inventoryItemIDs)
            {
                ItemData item = itemDatabase.GetItemByID(id);
                if (item != null) Inventory.AddItem(item);
            }

            foreach (string id in data.equippedItemIDs)
            {
                ItemData item = itemDatabase.GetItemByID(id);
                if (item != null) Equipment.EquipItem(item);
            }
        }

        Debug.Log($"Wczytano postęp gracza: {playerName}, Level: {data.level}");
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