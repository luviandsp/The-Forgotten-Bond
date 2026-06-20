using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    void Update()
    {
        // Jika pemain menekan W, A, S, D atau panah, tutorial menghilang
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            gameObject.SetActive(false); // Menyembunyikan gambar tutorial
        }
    }
}
