using UnityEngine;
using System.Collections;

public class FlamethrowerTrap : MonoBehaviour
{
    public enum TrapState { Idle, Warning, Active }

    [Header("Timing")]
    [SerializeField] private float idleDuration = 4f;     // Jak d³ugo miotacz czeka uœpiony
    [SerializeField] private float warningDuration = 1f;  // Czas ostrze¿enia (np. iskry)
    [SerializeField] private float activeDuration = 2f;   // Jak d³ugo bucha ogieñ

    [Header("Combat Settings")]
    [SerializeField] private int damagePerSecond = 15;
    [SerializeField] private float damageInterval = 0.5f; // Czêstotliwoœæ zadawania obra¿eñ (w sekundach)

    [Header("References")]
    [SerializeField] private Animator animator;

    private TrapState _currentState = TrapState.Idle;
    private bool _isPlayerInFire = false;
    private PlayerManager _playerManager;
    private Coroutine _damageCoroutine;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        StartCoroutine(TrapLoop());
    }

    private IEnumerator TrapLoop()
    {
        while (true)
        {
            // 1. FAZA: BEZPIECZNA (IDLE)
            _currentState = TrapState.Idle;
            if (animator != null) animator.Play("Flamethrower_Idle");
            StopDamageOverTime();
            yield return new WaitForSeconds(idleDuration);

            // 2. FAZA: OSTRZE¯ENIE (WARNING)
            _currentState = TrapState.Warning;
            if (animator != null) animator.Play("Flamethrower_Warning");
            yield return new WaitForSeconds(warningDuration);

            // 3. FAZA: AKTUALNY WYBUCH (ACTIVE)
            _currentState = TrapState.Active;
            if (animator != null) animator.Play("Flamethrower_Active");

            // Jeœli gracz ju¿ sta³ w miejscu wybuchu ognia, zacznij zadawaæ obra¿enia
            if (_isPlayerInFire && _playerManager != null)
            {
                _damageCoroutine = StartCoroutine(ApplyDamageOverTime());
            }

            yield return new WaitForSeconds(activeDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInFire = true;
            _playerManager = collision.GetComponent<PlayerManager>();

            if (_currentState == TrapState.Active && _damageCoroutine == null)
            {
                _damageCoroutine = StartCoroutine(ApplyDamageOverTime());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInFire = false;
            _playerManager = null;
            StopDamageOverTime();
        }
    }

    private IEnumerator ApplyDamageOverTime()
    {
        while (_isPlayerInFire && _currentState == TrapState.Active && _playerManager != null)
        {
            // Przyk³adowe wywo³anie zadania obra¿eñ:
            // _playerManager.TakeDamage(Mathf.RoundToInt(damagePerSecond * damageInterval));
            Debug.Log($"<color=orange>[OGIEÑ]</color> Gracz otrzymuje obra¿enia od miotacza ognia!");
            yield return new WaitForSeconds(damageInterval);
        }
        _damageCoroutine = null;
    }

    private void StopDamageOverTime()
    {
        if (_damageCoroutine != null)
        {
            StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }
    }
}