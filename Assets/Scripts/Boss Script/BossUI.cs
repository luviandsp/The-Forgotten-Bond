using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk mengakses UI Image

public class BossUI : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Masukkan objek Boss yang memiliki skrip BossHealth")]
    [SerializeField] private BossHealth bossHealth;
    
    [Tooltip("Masukkan objek BossHealthFill (bar merah) dari Canvas")]
    [SerializeField] private Image healthFillImage;

    private void OnEnable()
    {
        // Berlangganan (subscribe) ke event onHealthChanged milik boss
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        // Berhenti berlangganan saat objek mati/nonaktif agar tidak error memory leak
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged -= UpdateHealthBar;
        }
    }

    private void Start()
    {
        // Update tampilan health bar saat game baru dimulai
        if (bossHealth != null)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthFillImage == null || bossHealth == null) return;
        if (bossHealth.maxHealth <= 0) return; // Mencegah crash jika maxHealth 0

        // Hitung persentase nyawa (0.0f hingga 1.0f)
        float fillAmount = (float)bossHealth.currentHealth / bossHealth.maxHealth;
        
        healthFillImage.fillAmount = fillAmount;
    }
}