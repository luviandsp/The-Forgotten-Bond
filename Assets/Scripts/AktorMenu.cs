using UnityEngine;

public class AktorMenu : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public float titikBerhentiX; 
    public float kecepatan = 5f;

    [Header("Pengaturan Animasi")]
    public Animator animator;

    private bool sudahSampai = false;

    // TAMBAHKAN BAGIAN INI
    void Start()
    {
        if (animator != null)
        {
            // Menyalakan animasi lari sejak awal scene dimulai
            animator.SetBool("isRunning", true); 
        }
    }

    void Update()
    {
        if (sudahSampai) return;

        float posisiXBaru = Mathf.MoveTowards(transform.position.x, titikBerhentiX, kecepatan * Time.deltaTime);
        transform.position = new Vector3(posisiXBaru, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - titikBerhentiX) < 0.01f)
        {
            sudahSampai = true;
            
            if (animator != null)
            {
                // Mematikan animasi lari (kembali ke Idle)
                animator.SetBool("isRunning", false); 
            }
        }
    }
}