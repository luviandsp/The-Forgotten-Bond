using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class BossEndingCutscene : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject cutsceneCanvas;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;

    [Header("Black Screen")]
    public Image blackOverlay;

    [Header("Texts")]
    public GameObject storyText;
    public GameObject continueText;

    [Header("Image")]
    public Image cutsceneImage;

    [Header("Audio")]
    [Tooltip("GameObject BGM_Manager untuk stop BGM")]
    public GameObject bgmManager;
    [Tooltip("AudioClip ViolinEnd.mp3 untuk diputar saat cutscene")]
    public AudioClip cutsceneAudioClip;

    [Header("Timing")]
    public float blackScreenDuration = 2f;
    public float textDisplayDuration = 4f;
    public string nextSceneName = "MainMenu";

    private AudioSource bgmAudioSource;
    private AudioSource cutsceneAudioSource;

    private void Start()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

        SetOverlay(false);
        SetText(storyText, false);
        SetText(continueText, false);
        SetImage(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }

        // Cache BGM AudioSource
        if (bgmManager != null)
        {
            bgmAudioSource = bgmManager.GetComponent<AudioSource>();
        }

        // Create AudioSource for cutscene audio (ViolinEnd.mp3)
        cutsceneAudioSource = gameObject.AddComponent<AudioSource>();
        cutsceneAudioSource.playOnAwake = false;
        cutsceneAudioSource.loop = false;
        if (cutsceneAudioClip != null)
        {
            cutsceneAudioSource.clip = cutsceneAudioClip;
        }
    }

    public void PlayEndingCutscene()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(true);

        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // Stop BGM
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        // Freeze game
        Time.timeScale = 0f;

        // Black screen first
        SetOverlay(true);
        yield return new WaitForSecondsRealtime(1f);

        // === PLAY VIDEO ===
        // Hide overlay BEHIND video — overlay off, video on
        SetOverlay(false);
        if (videoPlayer != null && videoRawImage != null)
        {
            videoRawImage.gameObject.SetActive(true);

            // Ensure video renders ON TOP of overlay
            Canvas videoCanvas = videoRawImage.GetComponentInParent<Canvas>();
            if (videoCanvas != null)
            {
                videoCanvas.sortingOrder = 200;
            }

            videoPlayer.Play();

            // Wait until video is prepared
            float timeout = 5f;
            while (!videoPlayer.isPrepared && timeout > 0)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            // Wait for video to finish
            bool videoDone = false;
            videoPlayer.loopPointReached += (vp) => { videoDone = true; };
            while (!videoDone)
            {
                yield return null;
            }

            videoRawImage.gameObject.SetActive(false);
        }

        // === PLAY ViolinEnd.mp3 DURING TEXT SEQUENCES ===
        if (cutsceneAudioSource != null && cutsceneAudioClip != null)
        {
            cutsceneAudioSource.Play();
        }

        // Black screen after video
        SetOverlay(true);
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        // Show story text
        SetText(storyText, true);
        yield return new WaitForSecondsRealtime(textDisplayDuration);

        // Black screen
        SetText(storyText, false);
        SetOverlay(true);
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        // Show Cutscene_end.jpeg — hide overlay so image visible
        SetOverlay(false);
        SetImage(true);
        yield return new WaitForSecondsRealtime(textDisplayDuration);

        // Black screen
        SetImage(false);
        SetOverlay(true);
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        // Show "bersambung..." — hide overlay so text visible
        SetOverlay(true);
        SetText(continueText, true);
        yield return new WaitForSecondsRealtime(textDisplayDuration);

        // Stop cutscene audio
        if (cutsceneAudioSource != null && cutsceneAudioSource.isPlaying)
        {
            cutsceneAudioSource.Stop();
        }

        // Final black screen then load scene
        SetText(continueText, false);
        SetOverlay(true);
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    private void SetOverlay(bool show)
    {
        if (blackOverlay != null)
            blackOverlay.gameObject.SetActive(show);
    }

    private void SetText(GameObject textObj, bool show)
    {
        if (textObj != null)
            textObj.SetActive(show);
    }

    private void SetImage(bool show)
    {
        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(show);
    }
}
