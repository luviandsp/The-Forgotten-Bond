using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan agar Unity mengenali komponen 'Image'
using TMPro;

// Membuat struktur data khusus untuk setiap baris dialog
[System.Serializable]
public class DialogueLine
{
    public string speakerName;         // Nama karakter yang bicara
    [TextArea(3, 5)]
    public string sentence;            // Isi teks dialog
    public Sprite speakerFace;         // Gambar wajah karakter
    public bool isMainCharacter;       // Centang jika ini tokoh utama (gambar di kiri)
}

public class DialogueManager : MonoBehaviour
{
    [Header("Referensi UI Dialog")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Referensi Gambar Potret")]
    public Image leftPortrait;  // Masukkan objek LeftPortrait ke sini
    public Image rightPortrait; // Masukkan objek RightPortrait ke sini

    [Header("Referensi Tutorial")]
    public GameObject canvasTutorial;

    [Header("Daftar Percakapan")]
    // Array dari struktur data yang kita buat di atas
    public DialogueLine[] dialogueLines;

    private int currentIndex = 0;
    private bool isDialogueActive = false;

    void Start()
    {
        if (canvasTutorial != null)
        {
            canvasTutorial.SetActive(false);
        }

        StartDialogue();
    }

    void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        currentIndex = 0;

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Jika percakapan sudah habis
        if (currentIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        // Ambil data baris dialog saat ini
        DialogueLine currentLine = dialogueLines[currentIndex];

        // Set nama dan teks
        if (nameText != null) nameText.text = currentLine.speakerName;
        if (dialogueText != null) dialogueText.text = currentLine.sentence;

        // Atur posisi gambar wajah (kiri atau kanan)
        if (currentLine.isMainCharacter)
        {
            // Tampilkan di kiri, matikan yang kanan
            leftPortrait.gameObject.SetActive(true);
            rightPortrait.gameObject.SetActive(false);
            leftPortrait.sprite = currentLine.speakerFace;
        }
        else
        {
            // Tampilkan di kanan, matikan yang kiri
            leftPortrait.gameObject.SetActive(false);
            rightPortrait.gameObject.SetActive(true);
            rightPortrait.sprite = currentLine.speakerFace;
        }

        currentIndex++;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // Munculkan Canvas Tutorial setelah dialog selesai
        if (canvasTutorial != null)
        {
            canvasTutorial.SetActive(true);
        }
    }
}