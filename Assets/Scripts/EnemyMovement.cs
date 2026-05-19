using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // ============================================================
    //  PATROL SETTINGS
    // ============================================================
    [Header("Patrol Settings")]

    [Tooltip("Kecepatan enemy saat berjalan patrol")]
    [SerializeField] private float patrolSpeed = 2f;

    [Tooltip("Jarak maksimal patrol ke kiri dan kanan dari posisi spawn")]
    [SerializeField] private float patrolDistance = 5f;

    [Tooltip("Berapa lama musuh diam (Idle) di ujung patroli sebelum balik arah (dalam detik)")]
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

    private enum EnemyState
    {
        Patrol,
        Chase,
        Waiting // State baru saat musuh diam di ujung
    }

    private EnemyState currentState = EnemyState.Patrol;
    private bool isWaitingAtEdge = false; // Flag biar Co-routine gak kepanggil berkali-kali


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
        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;

            case EnemyState.Chase:
                HandleChase();
                break;

            case EnemyState.Waiting:
                // Saat menunggu, pastikan kecepatan horizontal bener-bener nol
                rb.velocity = new Vector2(0f, rb.velocity.y);
                break;
        }
    }

    private void UpdateDetection()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Player masuk ke detection range -> switch ke mode Chase
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != EnemyState.Chase)
            {
                // SOLUSI BUG: Hentikan co-routine nunggu secara paksa dan reset flag-nya langsung!
                StopAllCoroutines();
                isWaitingAtEdge = false;

                currentState = EnemyState.Chase;
            }
        }
        // Player keluar dari lose range -> switch kembali ke mode Patrol
        else if (currentState == EnemyState.Chase && distanceToPlayer > losePlayerRange)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (currentState == EnemyState.Chase)
        {
            // SOLUSI BUG: Saat mengejar, paksa 'isChasing' TRUE dan kunci 'isWalking' wajib FALSE
            animator.SetBool("isChasing", true);
            animator.SetBool("isWalking", false);
        }
        else if (currentState == EnemyState.Patrol)
        {
            // Saat patroli, pastikan lari mati
            animator.SetBool("isChasing", false);

            // 'isWalking' HANYA boleh true kalau dia emang lagi gerak pas patroli
            if (Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
        else if (currentState == EnemyState.Waiting)
        {
            // Saat diam menunggu di ujung, matikan semua arah gerakan biar balik ke IDLE
            animator.SetBool("isChasing", false);
            animator.SetBool("isWalking", false);
        }
    }

    private void HandlePatrol()
    {
        bool shouldWait = false;

        // Cek hambatan
        if (!IsGroundAhead() || IsWallAhead())
            shouldWait = true;

        // Cek batas jarak
        float offsetFromStart = transform.position.x - startPosition.x;
        bool reachedRightLimit = moveDirection > 0 && offsetFromStart >= patrolDistance;
        bool reachedLeftLimit  = moveDirection < 0 && offsetFromStart <= -patrolDistance;

        if (reachedRightLimit || reachedLeftLimit)
            shouldWait = true;

        // Jika menyentuh ujung dan belum masuk mode nunggu
        if (shouldWait && !isWaitingAtEdge)
        {
            StartCoroutine(WaitAtEdgeRoutine());
            return;
        }

        // Jalankan musuh jika sedang tidak menunggu
        if (currentState == EnemyState.Patrol)
        {
            rb.velocity = new Vector2(moveDirection * patrolSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// Co-routine untuk menahan musuh di posisi ujung
    /// </summary>
    private IEnumerator WaitAtEdgeRoutine()
    {
        isWaitingAtEdge = true;
        currentState = EnemyState.Waiting; // Pindah ke state diam
        rb.velocity = new Vector2(0f, rb.velocity.y); // Stop rem mendadak

        // Tunggu sesuai detik yang diinput di Inspector
        yield return new WaitForSeconds(patrolWaitTime);

        // Setelah selesai nunggu, balik arah baru jalan lagi
        moveDirection *= -1;
        FlipSprite();

        currentState = EnemyState.Patrol; // Balik ke mode jalan
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

        if (IsWallAhead())
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
        }
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
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private bool IsWallAhead()
    {
        Vector2 origin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position;
        Vector2 direction = Vector2.right * moveDirection;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = moveDirection < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 patrolCenter = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
        Gizmos.color = Color.cyan;
        Vector3 leftBound  = patrolCenter + Vector3.left  * patrolDistance;
        Vector3 rightBound = patrolCenter + Vector3.right * patrolDistance;
        Gizmos.DrawLine(leftBound, rightBound);
    }
}
