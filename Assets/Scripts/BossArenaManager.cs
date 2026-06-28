using UnityEngine;
using Cinemachine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Arena Barriers")]
    [Tooltip("Masukkan GameObject InvisibleWall ke sini")]
    [SerializeField] private GameObject invisibleWall;

    [Header("Camera Settings")]
    [Tooltip("Masukkan komponen Cinemachine Confiner biasa dari Virtual Camera")]
    [SerializeField] private CinemachineConfiner cameraConfiner; 

    [Tooltip("Masukkan Collider batas arena boss yang baru (BossCameraBounds)")]
    [SerializeField] private Collider2D bossArenaBoundary;

    [Tooltip("Masukkan komponen Cinemachine Camera Offset dari Virtual Camera")]
    [SerializeField] private CinemachineCameraOffset cameraOffset;

    // --- TAMBAHAN BARU UNTUK UI ---
    [Header("UI Settings")]
    [Tooltip("Masukkan objek container / parent dari Boss Health Bar UI")]
    [SerializeField] private GameObject bossHealthBarUI;

    private void Start()
    {
        // Pastikan tembok mati di awal permainan
        if (invisibleWall != null) 
        {
            invisibleWall.SetActive(false);
        }

        // Pastikan UI Boss Health Bar mati (tersembunyi) di awal permainan
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek apakah yang menyentuh trigger adalah Player
        if (collision.CompareTag("Player"))
        {
            LockArena();
        }
    }

    private void LockArena()
    {
        // 1. Hidupkan tembok penghalang agar player tidak bisa mundur
        if (invisibleWall != null)
        {
            invisibleWall.SetActive(true);
        }

        // 2. Ganti batas kamera ke area boss saja
        if (cameraConfiner != null && bossArenaBoundary != null)
        {
            cameraConfiner.m_BoundingShape2D = bossArenaBoundary;
            cameraConfiner.InvalidatePathCache(); 
        }

        // 3. Ubah nilai Offset Y pada Cinemachine Camera Offset menjadi 0
        if (cameraOffset != null)
        {
            Vector3 currentOffset = cameraOffset.m_Offset;
            currentOffset.y = -1.2f; 
            cameraOffset.m_Offset = currentOffset;
        }

        // 4. MUNCULKAN UI HEALTH BAR BOSS
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(true);
        }

        // 5. Matikan trigger ini sendiri agar kode tidak tereksekusi berkali-kali
        gameObject.SetActive(false);
    }
}