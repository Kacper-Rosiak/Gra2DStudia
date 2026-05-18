using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    public enum TrapState { Safe, Warning, Active, Cooldown }

    [Header("Trap Timing (Zsynchronizowane z Animatorem)")]
    [SerializeField] private float safeDuration = 3f;       // Kolce ca³kowicie schowane
    [SerializeField] private float warningDuration = 1f;    // Czubki drgaj¹ / ostrze¿enie
    [SerializeField] private float activeDuration = 0.4f;   // Krótki u³amek sekundy na cios
    [SerializeField] private float cooldownDuration = 1.2f; // Sterczenie kolców przed opadniêciem

    [Header("Direct Damage")]
    [SerializeField] private int spikeDamage = 15;          // Obra¿enia za nadepniêcie

    [Header("Bleed & Slow Settings (Efekty Kolców)")]
    [SerializeField] private float bleedDuration = 4f;
    [SerializeField] private int bleedDamagePerTick = 4;
    [SerializeField] private float bleedTickInterval = 0.8f;
    [SerializeField] private float slowMultiplier = 0.5f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private TrapState _currentState = TrapState.Safe;
    private bool _isPlayerInTrap = false;
    private PlayerManager _playerManager;
    private bool _hasDealtDamageThisCycle = false;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        StartCoroutine(TrapLoop());
    }

    private void Update()
    {
        if (_isPlayerInTrap && _currentState == TrapState.Active && !_hasDealtDamageThisCycle)
        {
            TryDealSpikeDamage();
        }
    }

    private IEnumerator TrapLoop()
    {
        // Czekamy dok³adnie 1 klatkê przed rozpoczêciem pêtli dla stabilizacji warstw Unity 6
        yield return null;

        while (true)
        {
            // 1. SAFE (Schowane kolce - POPRAWIONA NAZWA ANIMACJI)
            _currentState = TrapState.Safe;
            SafePlayAnimation("Trap_Safe");
            yield return new WaitForSeconds(safeDuration);

            // 2. WARNING (Ostrze¿enie)
            _currentState = TrapState.Warning;
            SafePlayAnimation("Trap_Warning");
            yield return new WaitForSeconds(warningDuration);

            // 3. ACTIVE (Wyskoczenie - zadawanie obra¿eñ)
            _currentState = TrapState.Active;
            _hasDealtDamageThisCycle = false;
            SafePlayAnimation("Trap_Active");

            if (_isPlayerInTrap)
            {
                TryDealSpikeDamage();
            }
            yield return new WaitForSeconds(activeDuration);

            // 4. COOLDOWN (Wysuniête, ale ju¿ bezpieczne)
            _currentState = TrapState.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
        }
    }

    private void SafePlayAnimation(string stateName)
    {
        if (animator != null && animator.runtimeAnimatorController != null && animator.layerCount > 0)
        {
            animator.Play(stateName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInTrap = true;
            _playerManager = collision.GetComponent<PlayerManager>();

            if (_currentState == TrapState.Active)
            {
                TryDealSpikeDamage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInTrap = false;
            _playerManager = null;
        }
    }

    private void TryDealSpikeDamage()
    {
        if (_playerManager != null && !_hasDealtDamageThisCycle)
        {
            _hasDealtDamageThisCycle = true;
            _playerManager.TakeDamage(spikeDamage);
            _playerManager.ApplyBleedAndSlow(bleedDuration, bleedDamagePerTick, bleedTickInterval, slowMultiplier);
            Debug.Log("<color=red>[PU£APKA]</color> Gracz otrzyma³ obra¿enia w fazie AKTYWNEJ kolców!");
        }
    }
}