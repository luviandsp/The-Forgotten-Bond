using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Scene Configuration")]
    public string mainMenuSceneName = "MainMenu"; // Make sure this matches your Main Menu scene name

    public static bool isGamePaused = false;

    void Start()
    {
        // Make sure the pause menu is hidden when the level starts
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    void Update()
    {
        // Toggle pause when pressing the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Unfreeze the game world
        isGamePaused = false;
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze everything (physics, animations, updates)
        isGamePaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // CRUCIAL: Always reset timeScale back to 1 before changing scenes!
        isGamePaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}