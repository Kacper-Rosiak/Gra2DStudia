using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Identity")]
    public string playerName = "Bohater";

    [Header("Player Profile (Data-Driven)")]
    public PlayerClassData startingClass;

    [Header("Save System")]
    public ItemDatabase itemDatabase;

    [Header("Visual Effects (Miotacz & Kolce)")]
    [SerializeField] private SpriteRenderer playerSprite;

    public PlayerStats Stats { get; private set; }
    public Inventory Inventory { get; private set; }
    public Equipment Equipment { get; private set; }
    public int inventoryCapacity = 24;

    public IAbilityStrategy CurrentAbility { get; private set; }

    [Header("Testing")]
    public System.Collections.Generic.List<ItemData> startingItems; 

    private bool isInCombat = false;

    private Coroutine _burnCoroutine;
    private Coroutine _bleedCoroutine; 
    private Color _originalColor = Color.white;

    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Zapewnienie, że obiekt jest w korzeniu (root), aby DontDestroyOnLoad zadziałało
            DontDestroyOnLoad(gameObject);

            if (startingClass != null)
            {
                Stats = new PlayerStats(startingClass);
                Inventory = new Inventory();
                Equipment = new Equipment(Stats);
                CurrentAbility = CreateStrategyForClass(startingClass.className);
            }
            else
            {
                Debug.LogError("Brak przypisanej klasy postaci w PlayerManager!");
            }
        }
        else
        {
            // Przenosimy trwałego gracza w miejsce, gdzie powinien zacząć w nowej scenie
            Instance.transform.position = this.transform.position;
            Instance.transform.rotation = this.transform.rotation;
            
            // Usuwamy duplikat z nowej sceny
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite != null) _originalColor = playerSprite.color;

        if (SaveManager.CurrentSaveData != null)
        {
            LoadFromSave(SaveManager.CurrentSaveData);
            SaveManager.CurrentSaveData = null;
        }
        else
        {
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.SelectedPlayerName))
            {
                playerName = GameManager.Instance.SelectedPlayerName;
            }

            // DODANIE PRZEDMIOTÓW STARTOWYCH (tylko dla nowej gry)
            if (startingItems != null)
            {
                foreach (var item in startingItems)
                {
                    if (item != null) Inventory.AddItem(item);
                }
            }
        }
    }

    private void LoadFromSave(SaveData data)
    {
        playerName = data.playerName;
        transform.position = data.playerPosition;
        Stats.LoadStats(data.level, data.currentXP, data.currentHP);
        Inventory.AddGold(data.gold);
        Inventory.AddKeys(data.keys);

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
    }

    private void OnEnable()
    {
        if (Stats != null)
        {
            Stats.OnLevelUp += HandleLevelUp;
            Stats.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (Stats != null)
        {
            Stats.OnLevelUp -= HandleLevelUp;
            Stats.OnDeath -= HandleDeath;
        }
    }

    private IAbilityStrategy CreateStrategyForClass(string className)
    {
        switch (className)
        {
            case "Warrior": return new WarriorAbilityStrategy();
            case "Mage": return new MageAbilityStrategy();
            case "Assassin": return new AssassinAbilityStrategy();
            default: return null;
        }
    }

    public Player GetCombatEntity(string playerName) => new Player(playerName, Stats, CurrentAbility);
    private void HandleLevelUp(int level) => Debug.Log($"LEVEL UP! {level}");
    private void HandleDeath()
    {
        Debug.Log("PLAYER DEAD");
        GameEvents.TriggerPlayerDeath();
        SceneManager.LoadScene("EndGame");
    }

    public void TakeDamage(int dmg)
    {
        if (Stats != null)
        {
            Stats.TakeDamage(dmg);
            Debug.Log($"<color=red>[OBRAŻENIA]</color> {playerName} otrzymał <color=yellow>{dmg}</color> pkt obrażeń! Aktualne HP: {Stats.CurrentHP}/{Stats.MaxHP}");
        }
    }

    public void ApplyBurning(float duration, int damagePerTick, float tickInterval)
    {
        if (_burnCoroutine != null) StopCoroutine(_burnCoroutine);
        _burnCoroutine = StartCoroutine(BurnCoroutine(duration, damagePerTick, tickInterval));
    }

    private IEnumerator BurnCoroutine(float duration, int damagePerTick, float tickInterval)
    {
        float elapsed = 0f;
        if (playerSprite != null) playerSprite.color = new Color(1f, 0.45f, 0f);

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            TakeDamage(damagePerTick);
            elapsed += tickInterval;
        }

        if (playerSprite != null) playerSprite.color = _originalColor;
        _burnCoroutine = null;
    }

    public void ApplyBleedAndSlow(float duration, int damagePerTick, float tickInterval, float slowMultiplier)
    {
        if (_bleedCoroutine != null) StopCoroutine(_bleedCoroutine);
        _bleedCoroutine = StartCoroutine(BleedAndSlowCoroutine(duration, damagePerTick, tickInterval, slowMultiplier));
    }

    private IEnumerator BleedAndSlowCoroutine(float duration, int damagePerTick, float tickInterval, float slowMultiplier)
    {
        float elapsed = 0f;
        float damageTimer = 0f;
        bool isRed = false;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.ApplySlow(slowMultiplier);

        while (elapsed < duration)
        {
            isRed = !isRed;
            if (playerSprite != null)
            {
                playerSprite.color = isRed ? Color.red : _originalColor;
            }

            yield return new WaitForSeconds(0.3f);
            elapsed += 0.3f;
            damageTimer += 0.3f;

            if (damageTimer >= tickInterval)
            {
                TakeDamage(damagePerTick);
                damageTimer = 0f;
            }
        }

        if (playerSprite != null) playerSprite.color = _originalColor;
        if (movement != null) movement.ResetSpeed();

        _bleedCoroutine = null;
        Debug.Log("<color=green>[STATUS]</color> Krwawienie zatamowane, spowolnienie minęło!");
    }

    public void GainXP(int xp) => Stats?.AddXP(xp);
    public void SetCombatState(bool state) => isInCombat = state;
}
