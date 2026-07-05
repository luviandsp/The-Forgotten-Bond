using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 5;
    public event Action onHealthChanged;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement PlayerMovement;
    private PlayerJump PlayerJump;
    private Animator animator;

    public float knockbackForceX = 5f;
    public float knockbackForceY = 5f;
    public float invincibilityDuration = 1.5f;
    private float knockbackDuration = 0.6f;
    private bool isInvincible = false;
    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip hurtSound;
    private AudioSource audioSource;
    private bool isDead = false;

    public bool IsDead => isDead;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerJump = GetComponent<PlayerJump>();
        animator = GetComponent<Animator>();

        // Setup AudioSource untuk death sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1, Vector2.zero);
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;

        // --- TAMBAHKAN LOGIKA SHIELD DI SINI ---
        if (PlayerMovement != null && PlayerMovement.isShielding)
        {
            // Tahan damage. Anda bisa menambahkan efek suara "ting" atau partikel shield di sini.
            return; 
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        onHealthChanged?.Invoke();

        // Play hurt sound
        if (currentHealth > 0)
        {
            // Play hurt sound hanya jika masih hidup
            if (audioSource != null && hurtSound != null)
            {
                audioSource.PlayOneShot(hurtSound);
            }
        
            StartCoroutine(HurtSequence(knockbackDirection));
        }
        else
        {
            Die();
        }
    }

    private IEnumerator HurtSequence(Vector2 knockbackDirection)
    {
        isInvincible = true;

        if (PlayerMovement != null) PlayerMovement.enabled = false;
        if (PlayerJump != null) PlayerJump.enabled = false;

        rb.velocity = Vector2.zero; // Stop current movement
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        StartCoroutine(RestoreControl(knockbackDuration));

        for (float i = 0; i < invincibilityDuration; i += 0.2f)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    private IEnumerator RestoreControl(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for knockback to finish
        if (!isDead)
        {
            if (PlayerMovement != null) PlayerMovement.enabled = true;
            if (PlayerJump != null) PlayerJump.enabled = true;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[PlayerHealth] Die() called!");

        if (PlayerMovement != null) PlayerMovement.enabled = false;
        if (PlayerJump != null) PlayerJump.enabled = false;

        rb.velocity = Vector2.zero; // Stop movement
        animator.SetTrigger("dead");

        // --- AUDIO ---
        // 1. Stop BGM
        GameObject bgmObj = GameObject.Find("BGM_Manager");
        if (bgmObj != null)
        {
            AudioSource bgm = bgmObj.GetComponent<AudioSource>();
            if (bgm != null) bgm.Stop();
        }

        // 2. Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Delay sebelum tampilkan panel, supaya animasi mati sempat jalan
        Debug.Log("[PlayerHealth] Invoking ShowGameOverPanel in 1.5s...");
        Invoke(nameof(ShowGameOverPanel), 1.5f);
    }

    void ShowGameOverPanel()
    {
        Debug.Log("[PlayerHealth] ShowGameOverPanel() called!");
        GameOverPanel gameOverPanel = FindObjectOfType<GameOverPanel>();
        Debug.Log("[PlayerHealth] FindObjectOfType<GameOverPanel> = " + (gameOverPanel != null ? "FOUND" : "NULL"));
        if (gameOverPanel != null)
        {
            gameOverPanel.ShowGameOver();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckTrap(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckTrap(collision);
    }

    private void CheckTrap(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") && !isDead)
        {
            // float contactPointX = collision.GetContact(0).point.x;
            // float directionX = transform.position.x < contactPointX ? -1f : 1f;
            // Vector2 knockbackDirection = new Vector2(directionX, 0);
            // TakeDamage(1, knockbackDirection);

            float directionX = 0.1f;
            if (rb.velocity.x > 0.1f)
            {
                directionX = -1f;
            }
            else if (rb.velocity.x < -0.1f)
            {
                directionX = 1f;
            } 
            else
            {
                directionX = spriteRenderer.flipX ? 1f : -1f;
            }

            Vector2 knockbackDirection = new Vector2(directionX, 0);
            TakeDamage(1, knockbackDirection);
        }
    }

    public void HealthReset()
    {
        currentHealth = maxHealth;
        isDead = false; // Memastikan status kematian dibatalkan
        
        // Memanggil UI agar menggambar ulang hatinya menjadi penuh
        onHealthChanged?.Invoke(); 
    }
}
