using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Image cutsceneImage;
    public Sprite[] cutscenes;

    public TMP_Text nameText;
    public TMP_Text dialogueText;

    public string[] characterNames;
    public string[] dialogues;

    public AudioSource bgmSource;
    public AudioClip[] cutsceneSounds;

    private int currentIndex = 0;

    void Start()
    {
        UpdateScene();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentIndex++;

            if (currentIndex < cutscenes.Length)
            {
                UpdateScene();
            }
            else
            {
                SceneManager.LoadScene("StageOne");
            }
        }
    }

    void UpdateScene()
    {
        cutsceneImage.sprite = cutscenes[currentIndex];
        nameText.text = characterNames[currentIndex];

        // ganti backsound
        if (cutsceneSounds[currentIndex] != null)
        {
            bgmSource.clip = cutsceneSounds[currentIndex];
            bgmSource.Play();
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(dialogues[currentIndex]));
    }

    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
    }
}