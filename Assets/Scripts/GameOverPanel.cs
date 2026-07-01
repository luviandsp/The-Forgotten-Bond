using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;

    [Header("Scene Configuration")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    public AudioClip gameoverSound;
    private AudioSource audioSource;

    private bool isGameOver = false;

    void Start()
    {
        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Auto-find panel jika belum di-assign di Inspector
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
            Debug.Log("[GameOverPanel] Auto-found panel: " + (gameOverPanel != null ? "OK" : "NULL"));
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        Debug.Log("[GameOverPanel] ShowGameOver called!");
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null)
        {
            // Reset posisi panel ke center layar
            RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            gameOverPanel.SetActive(true);
        }

        // Play gameover sound (loop)
        if (audioSource != null && gameoverSound != null)
        {
            audioSource.clip = gameoverSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        Time.timeScale = 0f;
    }

    // Tombol Try Again
    public void TryAgain()
    {
        Debug.Log("[GameOverPanel] Try Again pressed!");
        Time.timeScale = 1f;
        isGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Stop gameover sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Cari player
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        PlayerRespawn respawn = FindObjectOfType<PlayerRespawn>();

        if (playerHealth != null)
        {
            // 1. Reset health + isDead
            playerHealth.HealthReset();

            // 2. Nyalakan kembali kontrol
            PlayerMovement movement = playerHealth.GetComponent<PlayerMovement>();
            PlayerJump jump = playerHealth.GetComponent<PlayerJump>();
            if (movement != null) movement.enabled = true;
            if (jump != null) jump.enabled = true;

            // 3. Reset animator dari state "dead"
            Animator animator = playerHealth.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            // 4. Reset rigidbody
            Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }

        // 5. Pindahkan ke checkpoint
        if (respawn != null)
        {
            respawn.DieAndRespawn();
        }

        // 6. Resume BGM
        GameObject bgmObj = GameObject.Find("BGM_Manager");
        if (bgmObj != null)
        {
            AudioSource bgm = bgmObj.GetComponent<AudioSource>();
            if (bgm != null && !bgm.isPlaying)
            {
                bgm.Play();
            }
        }
    }

    // Tombol Main Menu
    public void MainMenu()
    {
        Debug.Log("[GameOverPanel] MainMenu pressed!");
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Tombol Quit
    public void QuitGame()
    {
        Debug.Log("[GameOverPanel] QuitGame pressed!");
        Application.Quit();
    }
}
