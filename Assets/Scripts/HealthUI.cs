using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Animator[] heartAnimators;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged += UpdateHearts;
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged -= UpdateHearts;
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartAnimators.Length; i++)
        {
            if (i < playerHealth.currentHealth)
            {
                heartAnimators[i].SetBool("isFull", true);
            }
            else
            {
                heartAnimators[i].SetBool("isFull", false);
            }
        }
    }
}
