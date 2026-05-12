using UnityEngine;

public class LavaDropObstacle : MonoBehaviour
{
    [Header("Pengaturan Jatuh")]
    public float fallSpeed = 5f;        // Kecepatan jatuh lava
    public float maxFallDistance = 8f;  // Jarak tempuh ke bawah sebelum kembali ke atas

    private Vector3 startPosition;

    void Start()
    {
        // Menyimpan posisi awal (koordinat langit-langit) saat game dimulai
        startPosition = transform.position;
    }

    void Update()
    {
        // Menggerakkan lava ke bawah setiap frame
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Jika jarak jatuh sudah melebihi batas maksimal
        if (Vector3.Distance(startPosition, transform.position) >= maxFallDistance)
        {
            // Reset posisi kembali ke langit-langit
            transform.position = startPosition;
        }
    }
}