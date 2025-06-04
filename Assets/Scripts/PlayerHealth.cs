using UnityEngine;
using System;
using UnityEngine.SceneManagement; 

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3; 
    private int currentHealth; 

    // Events yang bisa dipanggil oleh PlayerHealth dan dilanggan oleh script lain
    public static event Action<int> OnHealthChanged;
    public static event Action OnPlayerDied; 
    public static event Action OnPlayerRespawned; 

    private Collider2D playerCollider; 
    private Rigidbody2D playerRigidbody; 

    private Vector3 respawnPosition; 

    void Awake()
    {
        currentHealth = maxHealth; 
        
        // Panggil event ini untuk pertama kali agar UI menampilkan darah penuh di awal
        OnHealthChanged?.Invoke(currentHealth); 

        playerCollider = GetComponent<Collider2D>(); 
        playerRigidbody = GetComponent<Rigidbody2D>(); 

        respawnPosition = transform.position; // Simpan posisi awal sebagai posisi respawn
    }

    // Fungsi getter untuk mendapatkan darah saat ini (digunakan oleh HealthUI)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0) return; // Pastikan damage tidak negatif
        if (currentHealth <= 0) return; // Jangan ambil damage jika sudah mati

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Pastikan darah tidak di bawah 0

        Debug.Log($"Player took {amount} damage. Current HP: {currentHealth}");

        // Panggil event agar UI memperbarui tampilan darah
        OnHealthChanged?.Invoke(currentHealth); 

        if (currentHealth <= 0)
        {
            // --- INI ADALAH KOREKSI PENTING: Panggil fungsi kematian BERHENTI di sini ---
            // Ini dipanggil ketika darah habis akibat damage biasa (musuh, jebakan)
            HandleDeathByStopping(); 
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0) return; // Pastikan heal tidak negatif
        if (currentHealth <= 0) return; // Jangan heal jika sudah mati

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Pastikan darah tidak melebihi maxHealth

        Debug.Log($"Player healed {amount}. Current HP: {currentHealth}");

        // Panggil event agar UI memperbarui tampilan darah
        OnHealthChanged?.Invoke(currentHealth); 
    }

    // --- FUNGSI BARU: Ini adalah logika umum kematian (event, respawn timer) ---
    private void CommonDeathLogic()
    {
        // currentHealth = 0; // Tidak perlu di sini, sudah diatur di TakeDamage atau HandleDeathBySinking
        OnHealthChanged?.Invoke(currentHealth); // Update UI (darah sudah 0 atau nilai sesuai)
        
        OnPlayerDied?.Invoke(); // Panggil event pemain mati

        this.enabled = false; // Menonaktifkan script Health sementara
        Invoke("RespawnPlayer", 2f); // Panggil fungsi RespawnPlayer setelah 2 detik
    }

    // --- FUNGSI BARU: Untuk kematian yang menyebabkan pemain TENGGELAM (Acid, DeathZone) ---
    private void HandleDeathBySinking()
    {
        Debug.Log("Player died by sinking (Acid/DeathZone).");
        currentHealth = 0; // Pastikan darah 0 saat mati karena tenggelam
        CommonDeathLogic(); // Jalankan logika kematian umum

        if (playerRigidbody != null)
        {
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic; // Pastikan Rigidbody tetap Dynamic agar gravitasi bekerja
            playerRigidbody.velocity = Vector2.zero; // Hentikan kecepatan saat ini
            playerRigidbody.angularVelocity = 0f; // Hentikan rotasi
            
            // --- PENTING: JANGAN NONAKTIFKAN COLLIDER UNTUK SINKING! ---
            // Biarkan collider aktif agar pemain berinteraksi dengan permukaan acid
            if (playerCollider != null)
            {
                playerCollider.enabled = true; 
            }

            // --- PENTING: KURANGI SKALA GRAVITASI UNTUK EFEK TENGGELAM PERLAHAN ---
            // Sesuaikan nilai 0.2f ini untuk kecepatan tenggelam yang Anda inginkan (misal: 0.1f sangat lambat, 0.5f agak cepat)
            playerRigidbody.gravityScale = 0.2f; 
        }
    }

    // --- FUNGSI BARU: Untuk kematian yang menyebabkan pemain BERHENTI (Darah habis dari musuh/jebakan) ---
    private void HandleDeathByStopping()
    {
        Debug.Log("Player died by stopping (Damage/Trap).");
        currentHealth = 0; // Pastikan darah 0 saat mati karena berhenti
        CommonDeathLogic(); // Jalankan logika kematian umum

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector2.zero; // Hentikan kecepatan
            playerRigidbody.angularVelocity = 0f; // Hentikan rotasi
            // Jadikan Kinematic agar tidak dipengaruhi gravitasi dan berhenti di tempat
            playerRigidbody.bodyType = RigidbodyType2D.Kinematic; 
            
            // Biarkan collider tetap aktif agar pemain TIDAK TEMBUS KE BAWAH
            if (playerCollider != null)
            {
                playerCollider.enabled = true; 
            }
        }
    }

    private void RespawnPlayer()
    {
        Debug.Log("Player respawning...");
        
        transform.position = respawnPosition; // Pindahkan pemain ke posisi respawn

        currentHealth = maxHealth; // Isi kembali darah pemain
        OnHealthChanged?.Invoke(currentHealth); // Perbarui UI darah

        if (playerRigidbody != null)
        {
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic; // Kembalikan ke Dynamic agar bisa bergerak
            playerRigidbody.velocity = Vector2.zero; // Reset kecepatan
            playerRigidbody.angularVelocity = 0f; // Reset rotasi
            // --- PENTING: KEMBALIKAN SKALA GRAVITASI KE NORMAL SAAT RESPAWN ---
            playerRigidbody.gravityScale = 1f; // Gravity normal
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = true; // Aktifkan kembali collider
        }

        this.enabled = true; // Mengaktifkan kembali script Health
        OnPlayerRespawned?.Invoke(); // Panggil event bahwa pemain telah respawn
    }

    // --- Interaksi Trigger di sini (untuk objek yang Is Trigger-nya DICENTANG) ---
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (currentHealth <= 0) return; // Jangan proses trigger jika sudah mati

        if (other.CompareTag("HealthPickup"))
        {
            Heal(1); 
            Destroy(other.gameObject); // Hancurkan pickup darah
        }
        // Jika masuk ke Acid atau DeathZone, panggil fungsi kematian TENGGELAM
        else if (other.CompareTag("DeathZone") || other.CompareTag("Acid")) 
        {
            // Untuk deathzone/acid, kita ingin pemain TENGGELAM
            HandleDeathBySinking(); 
        }
    }

    // --- Kode untuk Tes (Opsional, Anda bisa aktifkan/nonaktifkan) ---
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.D)) 
    //     {
    //         TakeDamage(1);
    //     }
    //     if (Input.GetKeyDown(KeyCode.H)) 
    //     {
    //         Heal(1);
    //     }
    // }
}