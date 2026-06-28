using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public GameObject mainMenuContainer; // Wadah baru untuk menu utama
    public Slider volumeSlider;

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Pastikan menu utama menyala saat game dimulai
        if (mainMenuContainer != null)
        {
            mainMenuContainer.SetActive(true);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f);
        }
        
        AudioListener.volume = PlayerPrefs.GetFloat("GameVolume", 1f);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);      // Nyalakan panel setting
        mainMenuContainer.SetActive(false); // Matikan tombol-tombol menu utama
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);     // Matikan panel setting
        mainMenuContainer.SetActive(true);  // Nyalakan lagi tombol-tombol menu utama
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
    }
}