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

    [Header("Warstwy i Widoczność")]
    [SerializeField] private int combatSortingOrder = 10000;
    [SerializeField] private string characterSortingLayer = "Default";
    [SerializeField] private string backgroundSortingLayer = "Background";

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

        _combatManager.OnCombatLog += uiController.ShowMessage;
        _combatManager.OnBattleEnded += HandleBattleEnded;
        _combatManager.OnStateChanged += (state) => {
            uiController.UpdateTurnText(state.ToString());
            if (uiController.actionMenu != null)
                uiController.actionMenu.SetActive(state == BattleState.PlayerTurn);
        };

        uiController.InitializeUI(_player, _enemy);
        _combatManager.StartBattle(new List<Entity> { _player, _enemy });
    }

    private void HandleBattleEnded(BattleResult result)
    {
        uiController.ShowMessage($"Walka zakończona: {result}");

        if (result == BattleResult.Victory)
        {
            var triggers = FindFirstObjectByType<GameAchievementTriggers>();
            if (triggers != null)
            {
                triggers.TriggerKillAchievement();
            }
        }

        _scalingEnforced = false;
        StartCoroutine(EndBattleWithDelay(result == BattleResult.Victory));
    }

    private System.Collections.IEnumerator EndBattleWithDelay(bool won)
    {
        yield return new WaitForSeconds(2f);
        if (CombatTransitionManager.Instance != null)
        {
            CombatTransitionManager.Instance.EndCombat(won);
        }
    }

    public void OnAttackButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        ICombatCommand attack = new AttackCommand(_player, _enemy, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
        _combatManager.ExecuteTurnAction(attack);
    }

    public void OnSpecialAttackButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        if (_player == null || _enemy == null) return;

        if (_player is Player playerInstance && playerInstance.SpecialAbility != null)
        {
            Debug.Log($"UI: Kliknięto ATAK SPECJALNY ({playerInstance.SpecialAbility.GetType().Name})");
            ICombatCommand special = new UseAbilityCommand(_player, _enemy, playerInstance.SpecialAbility, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
            _combatManager.ExecuteTurnAction(special);

            // --- TRIGGER: LICZNIK SPECJALI (KOLEKCJONER) ---
            var triggers = FindFirstObjectByType<GameAchievementTriggers>();
            if (triggers != null)
            {
                triggers.TriggerSpecialAchievement();
            }
        }
        else
        {
            uiController.ShowMessage("<color=green>[Akcja gracza]</color> Ta postać nie posiada umiejętności specjalnej!");
        }
    }

    public void OnDefendButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        ICombatCommand defend = new DefendCommand(_player, message => uiController.ShowMessage($"<color=green>[Akcja gracza]</color> {message}"));
        _combatManager.ExecuteTurnAction(defend);
    }

    public void OnEscapeButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;
        _combatManager.TryEscape(40);
    }
}