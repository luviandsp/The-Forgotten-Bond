using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk pindah scene

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string introSceneName = "IntroCutscene";
    public string gameplaySceneName = "StageOne";

    // Fungsi untuk tombol Play (New Game)
    public void PlayGame()
    {
        // Reset data load dan checkpoint agar murni mulai dari awal
        PlayerPrefs.SetInt("LoadGame", 0);
        PlayerPrefs.DeleteKey("HasCheckpoint");
        PlayerPrefs.Save();

        // Akan memuat scene IntroCutscene
        SceneManager.LoadScene(introSceneName);
    }

    // Fungsi untuk tombol Load Game
    public void LoadSavedGame()
    {
        // Beri tanda ke script PlayerRespawn untuk memuat posisi checkpoint terakhir
        PlayerPrefs.SetInt("LoadGame", 1);
        PlayerPrefs.Save();

        // Langsung lompat ke scene level utama (skip intro)
        SceneManager.LoadScene(gameplaySceneName);
    }

    // Fungsi untuk tombol Quit
    public void QuitGame()
    {
        // Akan menutup game (hanya terlihat efeknya saat game sudah di-build)
        Debug.Log("Game Keluar!"); 
        Application.Quit();
    }
}