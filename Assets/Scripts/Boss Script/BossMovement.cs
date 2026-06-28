using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    // ============================================================
    //  CHASE SETTINGS
    // ============================================================
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float losePlayerRange = 12f;
    [SerializeField] private LayerMask playerLayer;

    // ============================================================
    //  ATTACK SETTINGS
    // ============================================================
    [Header("Attack Settings")]
    [Tooltip("Jarak boss berhenti mengejar dan mulai menyerang/menunggu cooldown")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackHitboxRadius = 0.6f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float singleAttackDuration = 0.5f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    // ============================================================
    //  RAYCAST SETTINGS
    // ============================================================
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private Transform wallCheckPoint;

    // ============================================================
    //  COMPONENTS
    // ============================================================
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    // ============================================================
    //  PRIVATE VARIABLES
    // ============================================================
    private int moveDirection = -1; // -1 berarti menghadap ke KIRI dari awal
    private Transform playerTransform;

    // Boss hanya memiliki 3 state utama karena tidak berpatroli
    private enum BossState { Idle, Chase, Attack }
    private BossState currentState = BossState.Idle;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Memaksa visual boss menghadap kiri dari awal permainan
        FlipSprite(); 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Update()
    {
        UpdateDetection();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // Paksa boss diam di tempat jika sedang memukul atau idle
        if (currentState == BossState.Attack || currentState == BossState.Idle)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (currentState == BossState.Chase)
        {
            HandleChase();
        }
    }

    private void UpdateDetection()
    {
        if (playerTransform == null) return;
        if (isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. JIKA MASUK JARAK SERANG (Di depan player persis)
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                // Cooldown habis, siap pukul
                StartCoroutine(AttackRandomSequence());
            }
            else
            {
                // Cooldown belum habis, DIAM DI TEMPAT menunggu (Tidak lari maju)
                currentState = BossState.Idle;
            }
            return;
        }

        // 2. DETEKSI CHASE / IDLE
        if (distanceToPlayer <= detectionRange)
        {
            currentState = BossState.Chase;
        }
        else if (distanceToPlayer > losePlayerRange)
        {
            currentState = BossState.Idle;
        }
    }

    private IEnumerator AttackRandomSequence()
    {
        isAttacking = true;
        currentState = BossState.Attack;

        int chosenAttackIndex = Random.Range(1, 4);
        nextAttackTime = Time.time + singleAttackDuration + attackCooldown;

        if (animator != null) animator.SetInteger("attackIndex", chosenAttackIndex);

        float delayBeforeDamage = singleAttackDuration * 0.3f;
        yield return new WaitForSeconds(delayBeforeDamage);

        Vector3 hitboxPos = attackPoint != null
            ? attackPoint.position
            : transform.position + new Vector3(moveDirection * 0.7f, 0f, 0f);

        Collider2D hitPlayer = Physics2D.OverlapCircle(hitboxPos, attackHitboxRadius, playerLayer);
        if (hitPlayer != null)
        {
            PlayerHealth targetHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (targetHealth != null)
            {
                float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
                targetHealth.TakeDamage(1, new Vector2(directionX, 0f));
            }
        }

        yield return new WaitForSeconds(singleAttackDuration - delayBeforeDamage);

        if (animator != null) animator.SetInteger("attackIndex", 0);
        isAttacking = false;

        // Reset state
        currentState = BossState.Idle;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (currentState == BossState.Attack)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isWalking", false);
        }
        else if (currentState == BossState.Chase)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isWalking", false);
        }
        else // Idle
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isWalking", false);
        }
    }

    private void HandleChase()
    {
        if (playerTransform == null) return;

        float horizontalDiff = playerTransform.position.x - transform.position.x;
        int directionToPlayer = horizontalDiff > 0 ? 1 : -1;

        if (directionToPlayer != moveDirection)
        {
            moveDirection = directionToPlayer;
            FlipSprite();
        }

        if (IsWallAhead()) rb.velocity = new Vector2(0f, rb.velocity.y);
        else rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
    }

    private bool IsWallAhead()
    {
        Vector2 origin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position;
        return Physics2D.Raycast(origin, Vector2.right * moveDirection, wallCheckDistance, groundLayer).collider != null;
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        // FlipX true jika moveDirection bernilai positif (tergantung setup sprite default Anda)
        spriteRenderer.flipX = moveDirection < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 hitboxPos = attackPoint != null ? attackPoint.position : transform.position + new Vector3(moveDirection * 0.7f, 0f, 0f);
        Gizmos.DrawWireSphere(hitboxPos, attackHitboxRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange); // Visual untuk stop distance
    }
}