using UnityEngine;

public class AktorMenu : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public float titikBerhentiX; 
    public float kecepatan = 5f;

    [Header("Pengaturan Animasi")]
    public Animator animator;
    
    [Tooltip("Ketik nama parameter lari yang sesuai (contoh: isRunning atau isChasing)")]
    public string parameterLari = "isRunning"; 

    private bool sudahSampai = false;

    void Start()
    {
        if (animator != null)
        {
            // Menyalakan animasi lari menggunakan variabel nama parameter
            animator.SetBool(parameterLari, true); 
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
                // Mematikan animasi lari menggunakan variabel nama parameter
                animator.SetBool(parameterLari, false); 
            }
        }
    }
}