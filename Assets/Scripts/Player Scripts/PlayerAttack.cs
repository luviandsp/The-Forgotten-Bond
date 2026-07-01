using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackOffsetDistance = 0.7f;

    [Header("Combo Settings")]
    [Tooltip("Waktu maksimal antar pukulan agar combo tidak ter-reset ke awal")]
    [SerializeField] private float comboResetTime = 1.0f;

    [Header("Audio")]
    [SerializeField] private AudioClip swordEffect1;
    [SerializeField] private AudioClip swordEffect2;
    [SerializeField] private AudioClip swordEffect3;

    private AudioSource audioSource;
    private float nextAttackTime = 0f;
    private float lastAttackTime = 0f;
    private int currentCombo = 1; // Mulai dari serangan 1

    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // 1. Jika game di-pause (waktu berhenti), hentikan pembacaan input
        if (Time.timeScale == 0f) return;

        // 2. Jika kursor mouse sedang menunjuk/mengklik UI, hentikan pembacaan input
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Kembalikan combo ke 1 jika jeda serangan terlalu lama
        if (Time.time - lastAttackTime > comboResetTime && !playerMovement.isAttacking)
        {
            currentCombo = 1;
        }

        if (Time.time >= nextAttackTime)
        {
            // Player tidak bisa menyerang jika sedang menahan shield
            if (Input.GetButtonDown("Fire1") && !playerMovement.isShielding)
            {
                lastAttackTime = Time.time;
                StartCoroutine(AttackSequence());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        if (playerMovement != null) playerMovement.isAttacking = true;

        // Play sword sound berdasarkan combo saat ini
        PlaySwordSound(currentCombo);

        if (animator != null)
        {
            // Kirim angka combo saat ini ke Animator, lalu trigger animasi
            animator.SetInteger("attackIndex", currentCombo);
            animator.SetTrigger("attack");
        }

        // Siapkan angka combo untuk pukulan berikutnya
        currentCombo++;
        if (currentCombo > 3) currentCombo = 1; // Loop kembali ke 1

        int currentDir = playerMovement != null ? playerMovement.moveDirection : 1;
        Vector2 hitPosition = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + new Vector2(currentDir * attackOffsetDistance, 0f);

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(hitPosition, attackRange);

        foreach (Collider2D obj in hitObjects)
        {
            float directionX = transform.position.x < obj.transform.position.x ? 1f : -1f;
            Vector2 knockbackDir = new Vector2(directionX, 0f);

            EnemyHealth enemyHP = obj.GetComponent<EnemyHealth>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(attackDamage, knockbackDir);
                continue;
            }

            BossHealth bossHP = obj.GetComponent<BossHealth>();
            if (bossHP != null)
            {
                bossHP.TakeDamage(attackDamage, knockbackDir);
            }
        }

        yield return new WaitForSeconds(attackDuration);

        if (playerMovement != null) playerMovement.isAttacking = false;
    }

    private void PlaySwordSound(int combo)
    {
        if (audioSource == null) return;

        AudioClip clip = null;
        switch (combo)
        {
            case 1: clip = swordEffect1; break;
            case 2: clip = swordEffect2; break;
            case 3: clip = swordEffect3; break;
        }

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        int currentDir = playerMovement != null ? playerMovement.moveDirection : 1;
        Vector3 hitPosition = attackPoint != null
            ? attackPoint.position
            : transform.position + new Vector3(currentDir * attackOffsetDistance, 0f, 0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPosition, attackRange);
    }
}
