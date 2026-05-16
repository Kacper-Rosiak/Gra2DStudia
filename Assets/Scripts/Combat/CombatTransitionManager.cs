using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CombatTransitionManager : MonoBehaviour
{
    public static CombatTransitionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string combatSceneName = "CombatScene";

    private GameObject _player;
    private GameObject _enemy;
    private Scene _dungeonScene;
    private UnityEngine.EventSystems.EventSystem _dungeonEventSystem;
    private AudioListener _dungeonAudioListener;
    private Vector3 _originalPlayerPos;
    private Vector3 _originalEnemyPos;
    private Vector3 _originalPlayerScale;
    private Vector3 _originalEnemyScale;
    private int _originalPlayerSortingOrder;
    private int _originalEnemySortingOrder;

    private bool _isCombatActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartCombat(GameObject player, GameObject enemy)
    {
        if (_isCombatActive) return;
        StartCoroutine(CombatTransitionRoutine(player, enemy));
    }

    private IEnumerator CombatTransitionRoutine(GameObject player, GameObject enemy)
    {
        _isCombatActive = true;
        _player = player;
        _enemy = enemy;
        _dungeonScene = SceneManager.GetActiveScene();

        // Wyłącz WSZYSTKIE EventSystemy i AudioListenery w aktualnych scenach
        foreach (var es in FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None))
        {
            es.enabled = false;
        }

        foreach (var al in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
        {
            al.enabled = false;
        }
        
        _originalPlayerPos = player.transform.position;
        _originalEnemyPos = enemy.transform.position;
        _originalPlayerScale = player.transform.localScale;
        _originalEnemyScale = enemy.transform.localScale;

        var pSR = player.GetComponent<SpriteRenderer>();
        if (pSR != null) _originalPlayerSortingOrder = pSR.sortingOrder;

        var eSR = enemy.GetComponent<SpriteRenderer>();
        if (eSR != null) _originalEnemySortingOrder = eSR.sortingOrder;

        // 1. Zablokuj gracza i wroga
        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;
        
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.simulated = false;

        // Tutaj można dodać wyłączenie AI wroga jeśli istnieje
        var enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null) enemyRb.simulated = false;

        // 2. Fade Out (Opcjonalnie - jeśli masz ScreenFader)
        yield return new WaitForSeconds(0.1f); 

        // 3. Załaduj scenę walki Additive
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(combatSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        Scene combatScene = SceneManager.GetSceneByName(combatSceneName);

        // 4. Przenieś obiekty do sceny walki (musi to być root object)
        player.transform.SetParent(null);
        enemy.transform.SetParent(null);
        
        SceneManager.MoveGameObjectToScene(player, combatScene);
        SceneManager.MoveGameObjectToScene(enemy, combatScene);

        // 5. Ukryj kamerę lochu ZANIM zainicjujesz walkę
        Camera dungeonCam = Camera.main;
        if (dungeonCam != null && dungeonCam.gameObject.scene == _dungeonScene) 
            dungeonCam.gameObject.SetActive(false);

        // 6. Znajdź CombatController i ustaw walkę
        CombatController controller = FindFirstObjectByType<CombatController>();
        if (controller != null)
        {
            controller.InitializeCombat(player, enemy);
        }
        else
        {
            Debug.LogError("CombatTransitionManager: Nie znaleziono CombatController w scenie walki!");
        }

        Debug.Log("CombatTransitionManager: Walka rozpoczęta!");
    }

    public void EndCombat(bool playerWon)
    {
        StartCoroutine(EndCombatRoutine(playerWon));
    }

    private IEnumerator EndCombatRoutine(bool playerWon)
    {
        // 1. Fade Out
        yield return new WaitForSeconds(0.5f);

        // 2. Przywróć kamerę lochu
        // Musimy ją znaleźć inaczej bo Camera.main może być nullem jeśli główna jest wyłączona
        foreach (var cam in Resources.FindObjectsOfTypeAll<Camera>())
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.scene == _dungeonScene)
            {
                cam.gameObject.SetActive(true);
                break;
            }
        }

        // 3. Przenieś gracza z powrotem do lochu
        SceneManager.MoveGameObjectToScene(_player, _dungeonScene);
        _player.transform.position = _originalPlayerPos;
        _player.transform.localScale = _originalPlayerScale;
        
        var pSR = _player.GetComponent<SpriteRenderer>();
        if (pSR != null) pSR.sortingOrder = _originalPlayerSortingOrder;

        // 4. Przywróć komponenty
        var playerMovement = _player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = true;

        var playerInput = _player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;
        
        var playerRb = _player.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.simulated = true;

        // 5. Obsługa wroga
        if (playerWon)
        {
            Destroy(_enemy);
        }
        else
        {
            SceneManager.MoveGameObjectToScene(_enemy, _dungeonScene);
            _enemy.transform.position = _originalEnemyPos;
            _enemy.transform.localScale = _originalEnemyScale;

            var eSR = _enemy.GetComponent<SpriteRenderer>();
            if (eSR != null) eSR.sortingOrder = _originalEnemySortingOrder;

            var enemyRb = _enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null) enemyRb.simulated = true;
        }

        // 6. Odładuj scenę walki
        SceneManager.UnloadSceneAsync(combatSceneName);

        // Przywróć EventSystem w lochu
        if (_dungeonEventSystem != null) _dungeonEventSystem.enabled = true;

        // Przywróć AudioListener w lochu
        if (_dungeonAudioListener != null) _dungeonAudioListener.enabled = true;

        _isCombatActive = false;
        Debug.Log("CombatTransitionManager: Powrót do lochu!");
    }
}
