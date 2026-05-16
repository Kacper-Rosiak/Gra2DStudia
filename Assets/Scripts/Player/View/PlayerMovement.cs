using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Vector2 moveinput;
    private Animator animator;

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
        // Je�li warto�� x jest na minusie (idziemy w lewo), w��czamy odwr�cenie
        if (moveinput.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
        // Je�li warto�� x jest na plusie (idziemy w prawo), wy��czamy odwr�cenie
        else if (moveinput.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
        // Je�li moveinput.x wynosi 0 (idziemy tylko w g�r�/d� lub stoimy), 
        // nie robimy nic � posta� zostaje odwr�cona tak, jak ostatnio.
    }
}
