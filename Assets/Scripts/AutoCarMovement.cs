// File: AutoCarMovement.cs
using UnityEngine;

public class AutoCarMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [Tooltip("Jarak maksimal mobil bergerak dari titik tengahnya ke satu arah (misal: 2 = 2 unit ke kiri dan 2 unit ke kanan).")]
    [SerializeField] private float moveDistance = 2f; // Jarak total gerakan akan menjadi 2 * moveDistance
    [Tooltip("Kecepatan gerakan horizontal mobil.")]
    [SerializeField] private float moveSpeed = 1f;

    private float startXPosition; // Menyimpan posisi X awal mobil

    void Awake()
    {
        // Simpan posisi X awal dari GameObject ini (AssembledCar)
        startXPosition = transform.position.x; 
    }

    void Update()
    {
        // --- Pilihan 1: Menggunakan Mathf.PingPong (Gerakan lebih linear) ---
        // Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) akan menghasilkan nilai
        // yang bergerak dari 0 hingga (moveDistance * 2) dan kembali lagi ke 0.
        // Kita kurangi moveDistance untuk menggeser rentang nilai menjadi dari -moveDistance hingga +moveDistance.
        float offsetX = Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;

        // --- Pilihan 2: Menggunakan Mathf.Sin (Gerakan lebih halus, seperti "nafas") ---
        // Anda bisa mengganti baris di atas dengan ini jika ingin efek yang berbeda
        // float offsetX = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        // Terapkan posisi X yang baru, Y dan Z tetap sama
        transform.position = new Vector3(startXPosition + offsetX, transform.position.y, transform.position.z);
    }
}