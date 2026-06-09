using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float doubleJumpForce = 6f;
    
    private bool isGrounded;
    private bool canDoubleJump;

    void Update()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump(jumpForce);
        } else if (Input.GetButtonDown("Jump") && !isGrounded && canDoubleJump)
        {
            Jump(doubleJumpForce);
            canDoubleJump = false;
        }

        // Update animator dengan kondisi lompatan
        animator.SetBool("isGrounded", isGrounded);
    }

    private void Jump(float force)
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0); 
        
        rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        animator.SetTrigger("jump");
        isGrounded = false;
        canDoubleJump = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}