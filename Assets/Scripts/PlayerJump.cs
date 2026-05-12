using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private float jumpForce = 10f; // Biasanya butuh angka lebih kecil (misal 10-20) jika digabungkan dengan rb.velocity
    
    private bool isGrounded;

    void Update()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Update animator dengan kondisi lompatan
        animator.SetBool("isGrounded", isGrounded);
    }

    void Jump()
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0); 
        
        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        animator.SetTrigger("jump");
        isGrounded = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}