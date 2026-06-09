using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    // ============================================================
    //  HEALTH SETTINGS
    // ============================================================
    [Header("Health Settings")]
    public int currentHealth;
    public int maxHealth = 3; // Biasanya musuh biasa punya HP lebih kecil dari player
    public event Action onHealthChanged;

    // ============================================================
    //  KNOCKBACK & INVINCIBILITY
    // ============================================================
    [Header("Knockback & Invincibility")]
    public float knockbackForceX = 4f;
    public float knockbackForceY = 3f;
    public float invincibilityDuration = 0.4f; // Lebih singkat dari player agar kombo terasa enak
    [SerializeField] private float knockbackDuration = 0.3f;
    private bool isInvincible = false;
    private bool isDead = false;

    // ============================================================
    //  COMPONENTS
    // ============================================================
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyMovement enemyMovement; // Referensi ke script pergerakan musuhmu

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;

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

        // Matikan AI pergerakan musuh saat terkena hit
        if (enemyMovement != null) enemyMovement.enabled = false;

        rb.velocity = Vector2.zero; // Hentikan laju gerak musuh saat ini
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        StartCoroutine(RestoreControl(knockbackDuration));

        // Efek visual berkedip merah saat terluka
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
        yield return new WaitForSeconds(delay); // Tunggu durasi knockback selesai
        if (!isDead)
        {
            // Hidupkan kembali AI musuh jika masih hidup
            if (enemyMovement != null) enemyMovement.enabled = true;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Matikan pergerakan AI sepenuhnya
        if (enemyMovement != null) enemyMovement.enabled = false;
        
        // 2. KUNCI POSISI: Ubah ke Kinematic & nolkan velocity agar tidak jatuh menembus tanah
        rb.velocity = Vector2.zero; 
        rb.bodyType = RigidbodyType2D.Kinematic; 

        // 3. Jalankan animasi mati
        if (animator != null) animator.SetTrigger("dead");

        // 4. Matikan collider instan agar Player bisa langsung lewat tanpa menabrak jasad
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 5. Hancurkan musuh setelah 1.5 detik (visual sprite otomatis ikut hancur)
        Destroy(gameObject, 1.5f);
    }
}