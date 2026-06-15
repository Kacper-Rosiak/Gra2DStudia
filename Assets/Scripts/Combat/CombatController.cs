using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    [Header("Referencje UI")]
    [SerializeField] private CombatUIController uiController;
    [SerializeField] private EnemyFactory enemyFactory;

    [Header("Pozycje Walki")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Skalowanie")]
    [SerializeField] private float playerScale = 120f;
    [SerializeField] private float enemyScale = 100f;

    // --- ZMIENNE DLA OSIĄGNIĘĆ ---
    private int _initialPlayerHP;
    private bool _playerTookDamageThisCombat = false;
    private int _accumulatedXP = 0;
    private int _playerMovesCount = 0;
    private bool _specialUsed = false;

    private CombatManager _combatManager;
    private Entity _player;
    private Entity _enemy;

    private GameObject _playerObj;
    private GameObject _enemyObj;
    private bool _scalingEnforced = false;

    private void Awake()
    {
        _combatManager = new CombatManager();
    }

    void Start()
    {
        if (uiController == null)
        {
            Debug.LogError("CombatController: Nie przypisano CombatUIController!");
        }
    }

    void LateUpdate()
    {
        float pS = playerScale > 0.001f ? playerScale : 120f;
        float eS = enemyScale > 0.001f ? enemyScale : 100f;

        if (_scalingEnforced && _playerObj != null && _enemyObj != null)
        {
            _playerObj.transform.localScale = new Vector3(pS, pS, 1f);
            _enemyObj.transform.localScale = new Vector3(-eS, eS, 1f);

            _playerObj.transform.position = new Vector3(_playerObj.transform.position.x, _playerObj.transform.position.y, 10f);
            _enemyObj.transform.position = new Vector3(_enemyObj.transform.position.x, _enemyObj.transform.position.y, 10f);
        }
    }

    public void InitializeCombat(GameObject playerObj, GameObject enemyObj)
    {
        if (playerObj == null || enemyObj == null) return;

        _playerObj = playerObj;
        _enemyObj = enemyObj;
        _scalingEnforced = true;

        int uiLayer = LayerMask.NameToLayer("UI");
        Camera combatCam = null;
        var rootObjects = gameObject.scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            combatCam = root.GetComponentInChildren<Camera>();
            if (combatCam != null) break;
        }

        Canvas canvas = uiController.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = combatCam;
            canvas.planeDistance = 30;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = -100;
        }

        var pSRs = playerObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in pSRs)
        {
            sr.gameObject.layer = uiLayer;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 1000;
        }

        var eSRs = enemyObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in eSRs)
        {
            sr.gameObject.layer = uiLayer;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 1000;
        }

        if (playerSpawnPoint != null) playerObj.transform.position = new Vector3(playerSpawnPoint.position.x, playerSpawnPoint.position.y, 10f);
        if (enemySpawnPoint != null) enemyObj.transform.position = new Vector3(enemySpawnPoint.position.x, enemySpawnPoint.position.y, 10f);

        EnemyOnMap enemyOnMap = enemyObj.GetComponent<EnemyOnMap>();
        EnemyFactory factory = enemyFactory != null ? enemyFactory : FindFirstObjectByType<EnemyFactory>();
        PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();

        if (factory != null && playerManager != null && enemyOnMap != null)
        {
            Enemy enemyModel = factory.CreateEnemy(enemyOnMap.databaseEnemyId);
            Entity playerModel = playerManager.GetCombatEntity(playerManager.playerName);
            if (enemyModel != null && playerModel != null) SetupBattle(playerModel, enemyModel);
        }
    }

    public void SetupBattle(Entity player, Entity enemy)
    {
        if (player == null || enemy == null) return;
        _player = player;
        _enemy = enemy;

        // [ACHIEVEMENT] Zapamiętujemy startowe HP do sprawdzania obrażeń
        _initialPlayerHP = _player.CurrentHP;
        _playerTookDamageThisCombat = false;
        _accumulatedXP = 0;
        _specialUsed = false;
        uiController.SetSpecialAbilityButtonInteractable(true);

        if (_enemy is Enemy enemyModel)
        {
            enemyModel.OnEnemyDeath += HandleEnemyDeath;
        }

        _combatManager.OnCombatLog += uiController.ShowMessage;
        _combatManager.OnBattleEnded += HandleBattleEnded;
        _combatManager.OnEnemyTurnReached += (enemy) => StartCoroutine(ExecuteEnemyTurnCoroutine(enemy));
        _combatManager.OnStateChanged += (state) => {
            uiController.UpdateTurnText(state.ToString());
            if (uiController.actionMenu != null)
                uiController.actionMenu.SetActive(state == BattleState.PlayerTurn);

            // [ACHIEVEMENT] Sprawdzanie czy gracz otrzymał obrażenia po każdym stanie
            if (_player.CurrentHP < _initialPlayerHP) _playerTookDamageThisCombat = true;
        };

        uiController.InitializeUI(_player, _enemy);
        _combatManager.StartBattle(new List<Entity> { _player, _enemy });
    }

    private void HandleEnemyDeath(int xpReward)
    {
        _accumulatedXP += xpReward;
    }

    private void HandleBattleEnded(BattleResult result)
    {
        if (_enemy is Enemy enemyModel)
        {
            enemyModel.OnEnemyDeath -= HandleEnemyDeath;
        }

        uiController.ShowMessage($"Walka zakończona: {result}");

        string title = result == BattleResult.Victory ? "VICTORY" : 
                      (result == BattleResult.Defeat ? "DEFEAT" : "Ucieczka");
        string message = "";

        if (result == BattleResult.Victory)
        {
            var triggers = FindFirstObjectByType<GameAchievementTriggers>();
            if (triggers != null)
            {
                triggers.TriggerKillAchievement();
            }

            CheckAchievementsAtVictory();

            // --- POWIADOMIENIA SYSTEMU MISJI ---
            if (QuestManager.Instance != null)
            {
                // 1. Zabij zielonoskórych (Goblina lub Orka)
                string enemyName = _enemy != null ? _enemy.Name.ToLower() : "";
                if (enemyName.Contains("goblin") || enemyName.Contains("orc"))
                {
                    QuestManager.Instance.ZwiekszPostepCelu("Zabij gobliny w okolicy");
                }

                // 2. Błyskawiczna egzekucja (mniej niż 3 ruchy)
                if (_playerMovesCount < 3)
                {
                    QuestManager.Instance.ZwiekszPostepCelu("Zabij przeciwnika w mniej niż 3 ruchach");
                }
            }

            // --- SYSTEM DROPÓW ---
            CombatDropResult drop = DropManager.GenerateCombatDrop();

            PlayerManager playerManager = _playerObj != null ? _playerObj.GetComponent<PlayerManager>() : PlayerManager.Instance;
            if (playerManager != null)
            {
                if (drop.Gold > 0) playerManager.Inventory.AddGold(drop.Gold);
                if (drop.Keys > 0) playerManager.Inventory.AddKeys(drop.Keys);
                playerManager.GainXP(_accumulatedXP);
                
                message = $"Łup:\n{drop.Message}\nZdobyto XP: {_accumulatedXP}\nStan XP: {playerManager.Stats.CurrentXP} / {playerManager.Stats.XPToNextLevel}";
            }
            else
            {
                message = $"Łup:\n{drop.Message}\nZdobyto XP: {_accumulatedXP}";
            }
        }
        else if (result == BattleResult.Defeat)
        {
            message = "Nie udało się zdobyć żadnych nagród.";
        }
        else
        {
            message = "Ucieczka zakończona sukcesem.";
        }

        _scalingEnforced = false;

        // Wyświetlamy popup i czekamy na "OK" przed powrotem
        if (GenericPopupController.Instance != null)
        {
            GenericPopupController.Instance.ShowPopup(title, message, () => ZakonczWalke(result == BattleResult.Victory));
        }
        else
        {
            Debug.LogWarning("GenericPopupController nie znaleziony! Powrót automatyczny.");
            StartCoroutine(EndBattleWithDelay(result == BattleResult.Victory));
        }
    }

    private void ZakonczWalke(bool won)
    {
        if (CombatTransitionManager.Instance != null)
        {
            CombatTransitionManager.Instance.EndCombat(won);
        }
    }

    // --- LOGIKA OSIĄGNIĘĆ PRZY ZWYCIĘSTWIE ---
    private void CheckAchievementsAtVictory()
    {
        // 1. Ogólny sygnał wygranej walki (dla Mistrz Areny)
        GameEvents.TriggerCombatWon();

        // 2. Sygnał zabicia wroga (dla Pierwsza Krew i Łowca Potworów)
        GameEvents.TriggerEnemyKilled();

        // Dodajemy zabezpieczenie przed brakiem systemu osiągnięć
        if (AchievementBootstrapper.Instance == null || AchievementBootstrapper.Instance.Achievements == null)
        {
            Debug.LogWarning("CombatController: AchievementBootstrapper.Instance lub Achievements jest nullem!");
            return;
        }

        var achievements = AchievementBootstrapper.Instance.Achievements;

        // 3. Sprawdzenie czy to był Boss (dla Pogromca Bossów)
        if (_enemy is Enemy e && e.IsBoss) // Zakładam, że w klasie Enemy masz pole IsBoss
        {
            GameEvents.TriggerBossKilled();

            // [ACHIEVEMENT] Boss Slayer / Mistrz Uników (bez obrażeń)
            if (!_playerTookDamageThisCombat)
            {
                achievements.UnlockAchievement("BOSS_NO_DMG");
                achievements.UnlockAchievement("EVADE_MASTER");
            }
        }

        // 4. [ACHIEVEMENT] Nietykalny (wygrana zwykłej walki bez obrażeń)
        if (!_playerTookDamageThisCombat)
        {
            achievements.UnlockAchievement("UNTOUCHABLE");
        }

        // 5. [ACHIEVEMENT] Last Pixel (wygrana z dokładnie 1 HP)
        if (_player.CurrentHP == 1)
        {
            achievements.UnlockAchievement("LAST_PIXEL");
        }
    }

    private System.Collections.IEnumerator EndBattleWithDelay(bool won)
    {
        yield return new WaitForSeconds(2f);
        ZakonczWalke(won);
    }

    private System.Collections.IEnumerator ExecuteEnemyTurnCoroutine(Entity enemy)
    {
        // 3-sekundowe opóźnienie przed ruchem wroga
        yield return new WaitForSeconds(3.0f);


        // Sprawdzenie czy walka nadal trwa i czy to nadal tura wroga
        if (_combatManager.CurrentState != BattleState.EnemyTurn) yield break;

        Entity playerTarget = _combatManager.GetPlayerTarget();

        if (playerTarget != null && playerTarget.IsAlive())
        {
            // Tworzymy komendę zwykłego ataku dla wroga
            ICombatCommand enemyAttack = new AttackCommand(enemy, playerTarget, log => uiController.ShowMessage($"<color=red>[WROGI ATAK]</color> {log}"));

            // Wykonujemy akcję
            _combatManager.ExecuteTurnAction(enemyAttack);
        }
        else
        {
            // Zabezpieczenie
            _combatManager.ExecuteTurnAction(new AttackCommand(enemy, null, log => { })); 
        }
    }

    public void OnAttackButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        _playerMovesCount++;
        ICombatCommand attack = new AttackCommand(_player, _enemy, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
        _combatManager.ExecuteTurnAction(attack);
    }

    public void OnSpecialAttackButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        if (_player == null || _enemy == null) return;

        if (_specialUsed)
        {
            uiController.ShowMessage("<color=orange>[INFO]</color> Możesz użyć umiejętności specjalnej tylko raz na walkę!");
            return;
        }

        if (_player is Player playerInstance && playerInstance.SpecialAbility != null)
        {
            _playerMovesCount++;
            _specialUsed = true;
            uiController.SetSpecialAbilityButtonInteractable(false);
            ICombatCommand special = new UseAbilityCommand(_player, _enemy, playerInstance.SpecialAbility, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
            _combatManager.ExecuteTurnAction(special);

            // --- TRIGGER: LICZNIK SPECJALI (KOLEKCJONER) ---
            var triggers = FindFirstObjectByType<GameAchievementTriggers>();
            if (triggers != null)
            {
                triggers.TriggerSpecialAchievement();
            }
        }
    }

    public void OnDefendButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        _playerMovesCount++;
        ICombatCommand defend = new DefendCommand(_player, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
        _combatManager.ExecuteTurnAction(defend);
    }

    public void OnEscapeButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        _combatManager.TryEscape(40);
    }
}