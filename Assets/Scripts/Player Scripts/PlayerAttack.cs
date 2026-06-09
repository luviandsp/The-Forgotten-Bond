using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // ============================================================
    //  KOMPONEN
    // ============================================================
    [Header("Components")]
    [SerializeField] private Animator animator;
    [Tooltip("Titik pusat lingkaran area serangan (buat empty GameObject di depan player)")]
    [SerializeField] private Transform attackPoint;

    // ============================================================
    //  PENGATURAN SERANGAN
    // ============================================================
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.5f;

    private float nextAttackTime = 0f;

    void Update()
    {
        // Cek apakah jeda cooldown sudah selesai
        if (Time.time >= nextAttackTime)
        {
            // Cek input tombol "Fire1" dari Input Manager
            if (Input.GetButtonDown("Fire1"))
            {
                Attack();
                // Set waktu kapan player bisa menyerang lagi
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void Attack()
    {
        // 1. Jalankan trigger animasi menyerang
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        // 2. Tentukan titik pusat serangan (fallback ke posisi player jika attackPoint belum diisi)
        Vector2 hitPosition = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        // 3. Deteksi semua collider 2D yang berada di dalam lingkaran serangan
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(hitPosition, attackRange);

        // 4. Lakukan loop dan seleksi hanya objek dengan tag "Enemy"
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyHealth enemyHP = obj.GetComponent<EnemyHealth>();
                if (enemyHP != null)
                {
                    // Hitung arah knockback: jika musuh di sebelah kanan player, lempar ke kanan (1f), jika di kiri lempar ke kiri (-1f)
                    float directionX = transform.position.x < obj.transform.position.x ? 1f : -1f;
                    Vector2 knockbackDir = new Vector2(directionX, 0f);

                    // Berikan damage dan efek knockback ke musuh
                    enemyHP.TakeDamage(attackDamage, knockbackDir);
                }
            }
        }
    }

    // Fitur visual untuk membantu melihat jarak pukul (hitbox) di dalam Unity Editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}