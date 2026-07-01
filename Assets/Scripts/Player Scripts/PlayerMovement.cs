using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip defendEffect;
    private AudioSource audioSource;
    private bool wasShielding = false;

    private Vector2 movement;
    private float xPosLastFrame;

    public int moveDirection { get; private set; } = 1;
    public bool isAttacking = false;
    
    // Penanda status shield
    public bool isShielding = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Deteksi input tahan tombol shield (misal: klik kanan / "Fire2")
        isShielding = Input.GetButton("Fire2");

        // Play defend sound sekali saat mulai shielding
        if (isShielding && !wasShielding)
        {
            if (audioSource != null && defendEffect != null)
                audioSource.PlayOneShot(defendEffect);
        }
        wasShielding = isShielding;
        
        // Kirim status shield ke Animator
        if (animator != null) animator.SetBool("isShielding", isShielding);

        // Jika player sedang menyerang ATAU menahan shield, hentikan gerakan
        if (isAttacking || isShielding)
        {
            animator.SetBool("isRunning", false);
            return; 
        }

        HandleMovement();
        FlipCharacterX();
    }

    private void FlipCharacterX()
    {
        float input = Input.GetAxis("Horizontal");

        if (input > 0 && (transform.position.x > xPosLastFrame))
        {
            spriteRenderer.flipX = false;
            moveDirection = 1;
        }
        else if (input < 0 && (transform.position.x < xPosLastFrame))
        {
            spriteRenderer.flipX = true;
            moveDirection = -1;
        }

        xPosLastFrame = transform.position.x;
    }

    private void HandleMovement()
    {
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);

        animator.SetBool("isRunning", input != 0);
    }
}