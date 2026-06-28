using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk pindah scene

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Ketik nama scene tujuan dengan tepat persis seperti nama file-nya")]
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah yang menyentuh kotak trigger adalah player
        if (collision.CompareTag("Player"))
        {
            // Memuat scene baru berdasarkan nama yang diketik di Inspector
            SceneManager.LoadScene(targetSceneName);
        }
    }
}