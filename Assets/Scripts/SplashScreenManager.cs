using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    public CanvasGroup logo;

    public float zoomDuration = 0.8f;
    public float stayDuration = 2f;
    public float fadeOutDuration = 1f;

    private RectTransform logoRect;

    void Start()
    {
        logoRect = logo.GetComponent<RectTransform>();

        // Awalnya kecil dan transparan
        logo.alpha = 0f;
        logoRect.localScale = Vector3.one * 0.2f;

        StartCoroutine(PlaySplash());
    }

    IEnumerator PlaySplash()
    {
        // =========================
        // Zoom In + Fade In
        // =========================
        float t = 0;

        while (t < zoomDuration)
        {
            t += Time.deltaTime;

            float progress = Mathf.SmoothStep(0f, 1f, t / zoomDuration);

            logo.alpha = progress;

            // Membesar dari 0.2 ke 1.15
            float scale = Mathf.Lerp(0.2f, 1.15f, progress);
            logoRect.localScale = Vector3.one * scale;

            yield return null;
        }

        // Efek "pop" kembali ke ukuran normal
        t = 0;

        while (t < 0.15f)
        {
            t += Time.deltaTime;

            float progress = t / 0.15f;

            float scale = Mathf.Lerp(1.15f, 1f, progress);
            logoRect.localScale = Vector3.one * scale;

            yield return null;
        }

        // Diam
        yield return new WaitForSeconds(stayDuration);

        // Fade Out
        t = 0;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;

            float progress = t / fadeOutDuration;

            logo.alpha = 1 - progress;

            yield return null;
        }

        SceneManager.LoadScene("MainMenu");
    }
}