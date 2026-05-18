using UnityEngine;
using System.Collections;

public class FlamethrowerTrap : MonoBehaviour
{
    public enum TrapState { Idle, Warning, Active }

    [Header("Trap Timing")]
    [SerializeField] private float idleDuration = 4f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float activeDuration = 2f;

    [Header("Damage Settings")]
    [SerializeField] private int directDamage = 15;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Burn Effect Settings")]
    [SerializeField] private float burnDuration = 3f;
    [SerializeField] private int burnDamagePerTick = 3;
    [SerializeField] private float burnTickInterval = 0.6f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private TrapState _currentState = TrapState.Idle;
    private bool _isPlayerInFlame = false;
    private PlayerManager _playerManager;
    private float _damageTimer = 0f;
    private bool _hasDealtDirectDamageThisCycle = false;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        StartCoroutine(TrapLoop());
    }

    private void Update()
    {
        if (_isPlayerInFlame && _currentState == TrapState.Active && !_hasDealtDirectDamageThisCycle)
        {
            TryDealDirectDamage();
        }
    }

    private IEnumerator TrapLoop()
    {
        // Czekamy 1 klatkê na start, aby daæ czas Animatorowi na za³adowanie warstw
        yield return null;

        while (true)
        {
            // 1. IDLE
            _currentState = TrapState.Idle;
            SafePlayAnimation("Flamethrower_Idle");
            yield return new WaitForSeconds(idleDuration);

            // 2. WARNING
            _currentState = TrapState.Warning;
            SafePlayAnimation("Flamethrower_Warning");
            yield return new WaitForSeconds(warningDuration);

            // 3. ACTIVE
            _currentState = TrapState.Active;
            _hasDealtDirectDamageThisCycle = false;
            SafePlayAnimation("Flamethrower_Active");

            if (_isPlayerInFlame)
            {
                TryDealDirectDamage();
            }

            yield return new WaitForSeconds(activeDuration);
        }
    }

    // BEZPIECZNE ODPALANIE ANIMACJI: Chroni przed b³êdem "Invalid Layer Index -1"
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
            _isPlayerInFlame = true;
            _playerManager = collision.GetComponent<PlayerManager>();

            if (_currentState == TrapState.Active)
            {
                TryDealDirectDamage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInFlame = false;
            _playerManager = null;
        }
    }

    private void TryDealDirectDamage()
    {
        if (_playerManager != null && !_hasDealtDirectDamageThisCycle)
        {
            _hasDealtDirectDamageThisCycle = true;
            _playerManager.TakeDamage(directDamage);
            _playerManager.ApplyBurning(burnDuration, burnDamagePerTick, burnTickInterval);
            Debug.Log($"<color=orange>[MIOTACZ]</color> {_playerManager.playerName} trafiony bezpoœrednim podmuchem!");
        }
    }
}