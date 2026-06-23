using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public Image cutsceneImage;
    public Sprite[] cutscenes;

    private int currentIndex = 0;

    void Start()
    {
        cutsceneImage.sprite = cutscenes[0];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentIndex++;

            if (currentIndex < cutscenes.Length)
            {
                cutsceneImage.sprite = cutscenes[currentIndex];
            }
            else
            {
                SceneManager.LoadScene("StageOne");
            }
        }
    }
}