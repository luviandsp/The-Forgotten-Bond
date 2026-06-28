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
    private bool isDead = false;
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

        if (currentHealth > 0)
        {
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

        if (PlayerMovement != null) PlayerMovement.enabled = false;
        if (PlayerJump != null) PlayerJump.enabled = false;

        rb.velocity = Vector2.zero; // Stop movement
        animator.SetTrigger("dead");

        Invoke(nameof(Respawn), 2f);
    }

    void Respawn()
    {
        // 1. Reset status mati dan darah kembali penuh
        isDead = false;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();

        // 2. Nyalakan kembali kontrol pergerakan
        if (PlayerMovement != null) PlayerMovement.enabled = true;
        if (PlayerJump != null) PlayerJump.enabled = true;

        // 3. Reset sistem animasi agar tidak tersangkut di gaya mati
        animator.Rebind();
        animator.Update(0f);

        // 4. Panggil sistem checkpoint untuk pindah lokasi
        PlayerRespawn respawnSystem = GetComponent<PlayerRespawn>();
        if (respawnSystem != null)
        {
            respawnSystem.DieAndRespawn();
        }
        else
        {
            // Cadangan: kalau script PlayerRespawn belum dipasang, baru restart level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
