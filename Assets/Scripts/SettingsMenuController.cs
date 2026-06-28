using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public GameObject mainMenuContainer;
    public Slider volumeSlider;

    void Start()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.SetActive(true);
        }

        // Set the slider value based on saved volume, default is 1 (Max Volume)
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f);
        }
        
        // Apply the saved volume immediately when the game starts
        AudioListener.volume = PlayerPrefs.GetFloat("GameVolume", 1f);
    }

    // Function to open the settings panel
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        mainMenuContainer.SetActive(false);
    }

    // Function to close the settings panel
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuContainer.SetActive(true);
    }

    // Function to adjust global volume (Called by the Slider)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        
        // Save the setting so it remains the same when the player reopens the game
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
    }
}