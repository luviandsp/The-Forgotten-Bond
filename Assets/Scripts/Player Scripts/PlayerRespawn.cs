using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [HideInInspector] public Vector2 respawnPoint;

    void Start()
    {
        // Titik awal bawaan
        respawnPoint = transform.position;

        // Cek apakah pemain menekan tombol Load dari Main Menu, DAN apakah ada data checkpoint
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1 && PlayerPrefs.GetInt("HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            
            respawnPoint = new Vector2(x, y);
            transform.position = respawnPoint; // Langsung pindahkan karakter ke checkpoint
            Debug.Log("Berhasil Load Checkpoint dari Main Menu!");
        }
        else
        {
            Debug.Log("Mulai game dari titik awal.");
        }
    }

    public void UpdateRespawnPoint(Vector2 newPoint)
    {
        respawnPoint = newPoint;
        
        // Simpan koordinat X dan Y secara permanen ke memori perangkat
        PlayerPrefs.SetFloat("CheckpointX", newPoint.x);
        PlayerPrefs.SetFloat("CheckpointY", newPoint.y);
        PlayerPrefs.SetInt("HasCheckpoint", 1); // Tanda bahwa sudah ada checkpoint yang tersimpan
        PlayerPrefs.Save();
        
        Debug.Log("Checkpoint Saved at: " + respawnPoint);
    }

    public void DieAndRespawn()
    {
        transform.position = respawnPoint;
        Debug.Log("Player respawned at Checkpoint!");
    }
}