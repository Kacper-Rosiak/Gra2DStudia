using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    public enum TrapState { Safe, Warning, Active }

    [Header("Trap Timing")]
    [Tooltip("Jak d³ugo kolce s¹ ca³kowicie schowane pod ziemi¹")]
    [SerializeField] private float safeDuration = 3f;

    [Tooltip("Jak d³ugo trwa ostrze¿enie przed wybuchem (drganie czubków)")]
    [SerializeField] private float warningDuration = 1f;

    [Tooltip("Jak krótko trwa sam cios (tylko w tym u³amku sekundy gracz dostaje obra¿enia!)")]
    [SerializeField] private float activeDuration = 0.4f;

    [Tooltip("NOWOŒÆ: Jak d³ugo po ciosie kolce stercz¹ jeszcze z ziemi, bêd¹c ju¿ CA£KOWICIE BEZPIECZNYMI")]
    [SerializeField] private float cooldownDuration = 1.2f;

    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private TrapState _currentState = TrapState.Safe;
    private bool _isPlayerOnTrap = false;
    private PlayerManager _playerManager;

    private bool _hasDealtDamageThisCycle = false;
    private float _damageTimer = 0f;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        StartCoroutine(TrapLoop());
    }

    private void Update()
    {
        // Jeœli faza aktywnego ciosu trwa odrobinê d³u¿ej i gracz na niej stoi, dostanie kolejne ciosy w interwale
        if (_isPlayerOnTrap && _currentState == TrapState.Active && _playerManager != null)
        {
            _damageTimer += Time.deltaTime;
            if (_damageTimer >= damageInterval)
            {
                DealDamageToPlayer();
                _damageTimer = 0f;
            }
        }
    }

    private IEnumerator TrapLoop()
    {
        while (true)
        {
            // 1. FAZA: CA£KOWICIE SCHOWANE (Bezpieczne, czysta pod³oga peaks_3)
            _currentState = TrapState.Safe;
            _hasDealtDamageThisCycle = false;
            _damageTimer = 0f;
            if (animator != null) animator.Play("Trap_Safe");
            yield return new WaitForSeconds(safeDuration);

            // 2. FAZA: OSTRZE¯ENIE (Bezpieczne, czubki peaks_4 pulsuj¹)
            _currentState = TrapState.Warning;
            if (animator != null) animator.Play("Trap_Warning");
            yield return new WaitForSeconds(warningDuration);

            // 3. FAZA: ATAK (NIEBEZPIECZNE! Kolce wyskakuj¹ na maksa peaks_1 i rani¹)
            _currentState = TrapState.Active;
            if (animator != null) animator.Play("Trap_Active");

            // Natychmiastowy cios, jeœli gracz sta³ na pu³apce w u³amku sekundy aktywacji
            if (_isPlayerOnTrap && !_hasDealtDamageThisCycle)
            {
                DealDamageToPlayer();
                _damageTimer = 0f;
            }
            yield return new WaitForSeconds(activeDuration);

            // 4. FAZA: OPADANIE / COOLDOWN (WIZUALNY EFEKT – CA£KOWICIE BEZPIECZNE!)
            _currentState = TrapState.Safe; // KLUCZ: Zmieniamy stan na Safe! Kod obra¿eñ ignoruje teraz gracza.
            _hasDealtDamageThisCycle = false;

            // Wymuszamy odtworzenie klatki peaks_4 (czubków), ¿eby kolce klimatycznie stercza³y po ataku
            if (animator != null) animator.Play("Trap_Warning");

            yield return new WaitForSeconds(cooldownDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerOnTrap = true;
            _playerManager = collision.GetComponent<PlayerManager>();

            // Gracz dostanie obra¿enia tylko wtedy, gdy wejdzie w trakcie trwania w³aœciwej fazy Active
            if (_currentState == TrapState.Active && !_hasDealtDamageThisCycle)
            {
                DealDamageToPlayer();
                _damageTimer = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerOnTrap = false;
            _playerManager = null;
        }
    }

    private void DealDamageToPlayer()
    {
        if (_playerManager != null)
        {
            _hasDealtDamageThisCycle = true;
            // _playerManager.TakeDamage(damageAmount); 
            Debug.Log($"<color=red>[PU£APKA]</color> Gracz otrzyma³ obra¿enia w fazie AKTYWNEJ kolców!");
        }
    }
}