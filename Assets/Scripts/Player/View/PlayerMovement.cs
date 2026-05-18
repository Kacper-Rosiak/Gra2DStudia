using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 3f; // Bazowa prędkość gracza
    private float moveSpeed;                           // Aktualna prędkość (może być modyfikowana)

    private Rigidbody2D rb;
    private Vector2 moveinput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Na starcie aktualna prędkość to prędkość bazowa
        moveSpeed = baseMoveSpeed;
    }

    void Update()
    {
        rb.linearVelocity = moveinput * moveSpeed;
        FlipSprite();
    }

    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("IsWalking", true);

        if (context.canceled)
        {
            animator.SetBool("IsWalking", false);
            animator.SetFloat("LastInputX", moveinput.x);
            animator.SetFloat("LastInputY", moveinput.y);
        }
        moveinput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveinput.x);
        animator.SetFloat("InputY", moveinput.y);
    }

    private void FlipSprite()
    {
        if (moveinput.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveinput.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
    }

    // --- NOWE METODY: KONTROLA SPOLWOLNIENIA ---
    public void ApplySlow(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
        Debug.Log($"<color=blue>[RUCH]</color> Nałożono spowolnienie! Szybkość: {moveSpeed}");
    }

    public void ResetSpeed()
    {
        moveSpeed = baseMoveSpeed;
        Debug.Log($"<color=blue>[RUCH]</color> Prędkość wróciła do normy: {moveSpeed}");
    }
}