using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector2 movement;
    private float xPosLastFrame;

    public int moveDirection { get; private set; } = 1;

    void Update()
    {
        HandleMovement();
        FlipCharacterX();
    }

    private void FlipCharacterX()
    {
        float input = Input.GetAxis("Horizontal");

        if (input > 0 && (transform.position.x > xPosLastFrame))
        {
            spriteRenderer.flipX = false;
            moveDirection = 1; // Update arah ke kanan
        }
        else if (input < 0 && (transform.position.x < xPosLastFrame))
        {
            spriteRenderer.flipX = true;
            moveDirection = -1; // Update arah ke kiri
        }

        xPosLastFrame = transform.position.x;
    }

    private void HandleMovement()
    {
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);

        if (input != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
}