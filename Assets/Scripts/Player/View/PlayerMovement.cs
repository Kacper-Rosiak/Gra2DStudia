using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 moveinput;
    private Animator animator;

    private bool isFacingRight = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveinput * moveSpeed;

        CheckDirection();
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
    private void CheckDirection()
    {
        if(isFacingRight && moveinput.x < 0f || !isFacingRight && moveinput.x > 0f)
        {
            isFacingRight = !isFacingRight;


            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

}
