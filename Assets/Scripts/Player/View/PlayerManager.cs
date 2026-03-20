using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerStats Stats { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private bool isInCombat = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        Stats = new PlayerStats(100, 10, 5);
    }

    private void OnEnable()
    {
        Stats.OnHealthChanged += HandleHealthChanged;
        Stats.OnLevelUp += HandleLevelUp;
        Stats.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        Stats.OnHealthChanged -= HandleHealthChanged;
        Stats.OnLevelUp -= HandleLevelUp;
        Stats.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (isInCombat) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }


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
        Stats.TakeDamage(dmg);
    }

    public void GainXP(int xp)
    {
        Stats.AddXP(xp);
    }

    public void SetCombatState(bool state)
    {
        isInCombat = state;
    }
}