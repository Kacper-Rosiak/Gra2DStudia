using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 moveinput;
    private Animator animator;

    private bool isFacingRight = true;
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveinput * moveSpeed;
        FlipSprite();
        //CheckDirection();
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
        // Jeœli wartoœæ x jest na minusie (idziemy w lewo), w³¹czamy odwrócenie
        if (moveinput.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
        // Jeœli wartoœæ x jest na plusie (idziemy w prawo), wy³¹czamy odwrócenie
        else if (moveinput.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
        // Jeœli moveinput.x wynosi 0 (idziemy tylko w górê/dó³ lub stoimy), 
        // nie robimy nic – postaæ zostaje odwrócona tak, jak ostatnio.
    }
}
