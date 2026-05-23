using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyMovement : MonoBehaviour
{
    // ============================================================
    //  PATROL SETTINGS
    // ============================================================
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float patrolWaitTime = 2f;

    // ============================================================
    //  CHASE SETTINGS
    // ============================================================
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float losePlayerRange = 9f;
    [SerializeField] private LayerMask playerLayer;

    // ============================================================
    //  ATTACK SETTINGS (RANDOM VERSION)
    // ============================================================
    [Header("Attack Settings")]
    [Tooltip("Jarak musuh mulai berhenti mengejar dan bersiap memukul")]
    [SerializeField] private float attackRange = 1.2f;

    [Tooltip("Jeda waktu istirahat musuh setelah selesai memukul (detik)")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Tooltip("Radius lingkaran titik pukul hitbox musuh")]
    [SerializeField] private float attackHitboxRadius = 0.5f;

    [Tooltip("Titik depan musuh tempat hantaman pukulan keluar")]
    [SerializeField] private Transform attackPoint;

    [Tooltip("Durasi satu animasi pukulan (detik). Hitung: jumlah frame ÷ Samples. Contoh Attack_Onre_1: 4 frame ÷ 12 fps = 0.35f")]
    [SerializeField] private float singleAttackDuration = 0.35f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    // ============================================================
    //  RAYCAST SETTINGS
    // ============================================================
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeCheckDistance = 1.2f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private Transform edgeCheckPoint;
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
    private Vector2 startPosition;
    private int moveDirection = 1;
    private Transform playerTransform;

    private enum EnemyState { Patrol, Chase, Waiting, Attack }
    private EnemyState currentState = EnemyState.Patrol;
    private bool isWaitingAtEdge = false;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPosition = transform.position;
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
        // Jika sedang memukul, paksa fisik musuh diam total di tempat
        if (currentState == EnemyState.Attack)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Waiting:
                rb.velocity = new Vector2(0f, rb.velocity.y);
                break;
        }
    }

    private void UpdateDetection()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. CEK REAL-TIME: JIKA MASUK JARAK SERANG & COOLDOWN HABIS -> MASUK STATE ATTACK
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackRandomSequence());
            return;
        }

        if (isAttacking) return;

        // 2. DETEKSI CHASE / PATROL
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != EnemyState.Chase)
            {
                StopAllCoroutines();
                isWaitingAtEdge = false;
                currentState = EnemyState.Chase;
            }
        }
        else if (currentState == EnemyState.Chase && distanceToPlayer > losePlayerRange)
        {
            currentState = EnemyState.Patrol;
        }
    }

    /// <summary>
    /// Coroutine untuk mengacak jenis pukulan, memutar animasi, dan mengirim damage.
    /// Timeline: [0%] Trigger animasi -> [30%] Hitbox aktif -> [100%] Reset & selesai.
    /// </summary>
    private IEnumerator AttackRandomSequence()
    {
        isAttacking = true;
        currentState = EnemyState.Attack;

        // Pilih animasi serangan secara acak (1, 2, atau 3)
        int chosenAttackIndex = Random.Range(1, 4);

        // Tentukan kapan musuh boleh menyerang lagi setelah coroutine ini selesai
        nextAttackTime = Time.time + singleAttackDuration + attackCooldown;

        // Trigger animasi serangan di Animator
        if (animator != null)
        {
            animator.SetInteger("attackIndex", chosenAttackIndex);
        }

        // Jeda tunggal: hitbox aktif tepat di 30% dari total durasi animasi
        float delayBeforeDamage = singleAttackDuration * 0.3f;
        yield return new WaitForSeconds(delayBeforeDamage);

        // Cek dan aplikasikan damage ke player via OverlapCircle
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

        // Tunggu sisa 70% durasi animasi hingga benar-benar selesai diputar
        yield return new WaitForSeconds(singleAttackDuration - delayBeforeDamage);

        // Reset parameter Animator setelah animasi selesai
        if (animator != null)
        {
            animator.SetInteger("attackIndex", 0);
        }

        isAttacking = false;

        // Tentukan state berikutnya berdasarkan jarak ke player
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            currentState = distanceToPlayer <= losePlayerRange ? EnemyState.Chase : EnemyState.Patrol;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Jika sedang menyerang, kunci parameter agar tidak dibajak oleh Idle/Walk
        if (currentState == EnemyState.Attack)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isWalking", false);
            return;
        }

        if (currentState == EnemyState.Chase)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isWalking", false);
        }
        else if (currentState == EnemyState.Patrol)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.1f);
        }
        else if (currentState == EnemyState.Waiting)
        {
            // Saat nunggu di ujung patroli, kembali ke Idle sepenuhnya
            animator.SetBool("isChasing", false);
            animator.SetBool("isWalking", false);
        }
    }

    private void HandlePatrol()
    {
        bool shouldWait = false;
        if (!IsGroundAhead() || IsWallAhead()) shouldWait = true;

        float offsetFromStart = transform.position.x - startPosition.x;
        bool reachedRightLimit = moveDirection > 0 && offsetFromStart >= patrolDistance;
        bool reachedLeftLimit  = moveDirection < 0 && offsetFromStart <= -patrolDistance;

        if (reachedRightLimit || reachedLeftLimit) shouldWait = true;

        if (shouldWait && !isWaitingAtEdge)
        {
            StartCoroutine(WaitAtEdgeRoutine());
            return;
        }

        if (currentState == EnemyState.Patrol)
        {
            rb.velocity = new Vector2(moveDirection * patrolSpeed, rb.velocity.y);
        }
    }

    private IEnumerator WaitAtEdgeRoutine()
    {
        isWaitingAtEdge = true;
        currentState = EnemyState.Waiting;
        rb.velocity = new Vector2(0f, rb.velocity.y);
        yield return new WaitForSeconds(patrolWaitTime);

        moveDirection *= -1;
        FlipSprite();
        currentState = EnemyState.Patrol;
        isWaitingAtEdge = false;
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

    private bool IsGroundAhead()
    {
        Vector2 origin;
        if (edgeCheckPoint != null) origin = edgeCheckPoint.position;
        else
        {
            float halfWidth = spriteRenderer != null ? spriteRenderer.bounds.extents.x : 0.5f;
            origin = (Vector2)transform.position + new Vector2(moveDirection * halfWidth, -0.1f);
        }
        return Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer).collider != null;
    }

    private bool IsWallAhead()
    {
        Vector2 origin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position;
        return Physics2D.Raycast(origin, Vector2.right * moveDirection, wallCheckDistance, groundLayer).collider != null;
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = moveDirection < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 patrolCenter = Application.isPlaying ? (Vector3)startPosition : transform.position;

        Gizmos.color = Color.red;
        Vector3 hitboxPos = attackPoint != null ? attackPoint.position : transform.position + new Vector3(moveDirection * 0.7f, 0f, 0f);
        Gizmos.DrawWireSphere(hitboxPos, attackHitboxRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(patrolCenter + Vector3.left * patrolDistance, patrolCenter + Vector3.right * patrolDistance);
    }
}
