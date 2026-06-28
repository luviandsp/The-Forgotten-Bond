using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object touching the checkpoint has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            // Get the PlayerRespawn script attached to the player
            PlayerRespawn respawnSystem = collision.GetComponent<PlayerRespawn>();
            
            if (respawnSystem != null)
            {
                // Send the checkpoint's position to the player's respawn system
                respawnSystem.UpdateRespawnPoint(transform.position);
            }
        }
    }
}