using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    // ============================================================
    //  HEALTH & SHIELD SETTINGS
    // ============================================================
    [Header("Health & Shield Settings")]
    public int currentHealth;
    public int maxHealth = 15; // Boss biasanya punya HP lebih banyak
    
    [Tooltip("Peluang persen (0 - 100) boss menangkis/block serangan player")]
    [Range(0f, 100f)]
    public float blockChance = 25f; 
    
    public event Action onHealthChanged;

    // ============================================================
    //  KNOCKBACK & INVINCIBILITY
    // ============================================================
    [Header("Knockback & Invincibility")]
    public float knockbackForceX = 3f;
    public float knockbackForceY = 2f;
    public float invincibilityDuration = 0.5f; 
    [SerializeField] private float knockbackDuration = 0.3f;
    private bool isInvincible = false;
    private bool isDead = false;

    [Header("Ending Cutscene")]
    [SerializeField] private BossEndingCutscene endingCutscene;

    // ============================================================
    //  COMPONENTS
    // ============================================================
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BossMovement bossMovement; // Terhubung dengan skrip BossMovement

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        bossMovement = GetComponent<BossMovement>();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;

        // 1. LOGIKA SHIELD / BLOCK
        float randomRoll = UnityEngine.Random.Range(0f, 100f);
        if (randomRoll <= blockChance)
        {
            // Boss berhasil menangkis serangan!
            BlockAttack();
            return; // Hentikan fungsi di sini, nyawa tidak berkurang
        }

        // 2. JIKA GAGAL BLOCK (Take Damage Normal)
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

    private void BlockAttack()
    {
        // Jika Anda punya animasi "block" di animator, jalankan trigger-nya
        if (animator != null) animator.SetTrigger("block");

        // Mainkan efek visual bertahan (misal berkedip warna biru muda/shield)
        StartCoroutine(BlockVisualEffect());
    }

    private IEnumerator BlockVisualEffect()
    {
        // Ubah warna menjadi biru/warna shield untuk sesaat
        spriteRenderer.color = new Color(0.5f, 0.8f, 1f); 
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator HurtSequence(Vector2 knockbackDirection)
    {
        isInvincible = true;

        if (bossMovement != null) bossMovement.enabled = false;

        rb.velocity = Vector2.zero; 
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
        yield return new WaitForSeconds(delay); 
        if (!isDead)
        {
            if (bossMovement != null) bossMovement.enabled = true;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (bossMovement != null) bossMovement.enabled = false;
        
        rb.velocity = Vector2.zero; 
        rb.bodyType = RigidbodyType2D.Kinematic; 

        if (animator != null) animator.SetTrigger("dead");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Trigger ending cutscene if assigned
        if (endingCutscene != null)
        {
            endingCutscene.PlayEndingCutscene();
        }
        else
        {
            Destroy(gameObject, 3f);
        }
    }
}