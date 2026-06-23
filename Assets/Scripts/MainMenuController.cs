using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk pindah scene

public class MainMenuController : MonoBehaviour
{
    // Fungsi untuk tombol Play
    public void PlayGame()
    {

        // Akan memuat scene IntroCutscene
        SceneManager.LoadScene("IntroCutscene");
    }

    // Fungsi untuk tombol Quit
    public void QuitGame()
    {
        // Akan menutup game (hanya terlihat efeknya saat game sudah di-build)
        Debug.Log("Game Keluar!"); 
        Application.Quit();
    }
}