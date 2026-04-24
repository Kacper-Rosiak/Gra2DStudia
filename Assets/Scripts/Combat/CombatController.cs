using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    [Header("Referencje UI")]
    [SerializeField] private CombatUIController uiController;

    [Header("Pozycje Walki")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;
    
    [Header("Skalowanie")]
    [SerializeField] private float playerScale = 120f; // Osobna skala dla gracza
    [SerializeField] private float enemyScale = 100f;  // Osobna skala dla wroga
    
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

    void Start()
    {
        _combatManager = new CombatManager();

        if (uiController == null)
        {
            Debug.LogError("CombatController: Nie przypisano CombatUIController!");
        }
    }

    void LateUpdate()
    {
        // Zabezpieczenie przed skalami 0
        float pS = playerScale > 0.001f ? playerScale : 120f;
        float eS = enemyScale > 0.001f ? enemyScale : 100f;

        if (_scalingEnforced && _playerObj != null && _enemyObj != null)
        {
            _playerObj.transform.localScale = new Vector3(pS, pS, 1f);
            _enemyObj.transform.localScale = new Vector3(-eS, eS, 1f);
            
            // WYMAGANIE UŻYTKOWNIKA: Pozycja Z = 10f
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

        // 0. Znajdź parametry warstw i kamerę
        int uiLayer = LayerMask.NameToLayer("UI");
        Camera combatCam = null;
        var rootObjects = gameObject.scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            combatCam = root.GetComponentInChildren<Camera>();
            if (combatCam != null) break;
        }

        // 1. Konfiguracja Canvasa (Tła)
        Canvas canvas = uiController.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = combatCam;
            // Skoro postacie są na Z = 10, a kamera na Z = -10 (odległość 20),
            // to tło musi być dalej niż 20 jednostek od kamery.
            canvas.planeDistance = 30; // Tło ląduje na Z = 20 (Kamera -10 + 30)
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = -100;
        }

        // 2. Przeniesienie postaci
        var pSRs = playerObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach(var sr in pSRs) {
            sr.gameObject.layer = uiLayer;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 1000;
        }

        var eSRs = enemyObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach(var sr in eSRs) {
            sr.gameObject.layer = uiLayer;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 1000;
        }

        // 3. Pozycjonowanie początkowe na Z = 10
        if (playerSpawnPoint != null) playerObj.transform.position = new Vector3(playerSpawnPoint.position.x, playerSpawnPoint.position.y, 10f);
        if (enemySpawnPoint != null) enemyObj.transform.position = new Vector3(enemySpawnPoint.position.x, enemySpawnPoint.position.y, 10f);

        Debug.Log($"CombatController: Ustawiono Z = 10. Tło odsunięte na PlaneDistance = 30.");

        // 4. Pobranie danych i start walki
        EnemyOnMap enemyOnMap = enemyObj.GetComponent<EnemyOnMap>();
        EnemyFactory factory = FindFirstObjectByType<EnemyFactory>();
        PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();

        if (factory != null && playerManager != null && enemyOnMap != null)
        {
            Enemy enemyModel = factory.CreateEnemy(enemyOnMap.databaseEnemyId);
            Entity playerModel = playerManager.GetCombatEntity(playerManager.playerName);

            SetupBattle(playerModel, enemyModel);
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
        
        // Powiadomienie menedżera o zakończeniu po krótkim opóźnieniu
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

    // --- METODY DLA PRZYCISKÓW (On Click w Unity) ---

    public void OnAttackButtonClicked()
    {
        // Sprawdzamy czy to tura gracza
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        // Zabezpieczenie przed brakiem danych
        if (_player == null || _enemy == null) return;

        Debug.Log("UI: Kliknięto przycisk ATAK");

        // Tworzymy komendę ataku (zgodnie z systemem kolegów)
        // message => uiController.ShowMessage(message) przesyła opis ataku do logu walki
        ICombatCommand attack = new AttackCommand(_player, _enemy, message => uiController.ShowMessage(message));

        // Wykonujemy akcję w silniku walki
        _combatManager.ExecuteTurnAction(attack);
    }

    public void OnDefendButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        uiController.ShowMessage("Bohater przyjmuje postawę obronną!");

        // Tutaj można wywołać DefendCommand, jeśli jest zaimplementowany:
        // _combatManager.ExecuteTurnAction(new DefendCommand(_player));
    }

    public void OnEscapeButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        uiController.ShowMessage("Próba ucieczki...");
        _combatManager.TryEscape(40); // 40% szansy na ucieczkę
    }
}