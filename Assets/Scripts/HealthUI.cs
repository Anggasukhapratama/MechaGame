using UnityEngine;
using TMPro;
using UnityEngine.UI; // PENTING: Perlu ini untuk menggunakan komponen Image

public class HealthUI : MonoBehaviour
{
    [Header("Player Health UI")]
    [SerializeField] private Image[] healthIcons; // Array untuk menampung gambar hati (Heart_1, Heart_2, Heart_3)
    [SerializeField] private Sprite fullHeartSprite; // Sprite untuk gambar hati penuh
    [SerializeField] private Sprite emptyHeartSprite; // Sprite untuk gambar hati kosong
    
    // public TextMeshProUGUI healthText; // Jika tidak digunakan lagi, Anda bisa menghapus baris ini

    [Header("Item Count UI")]
    [SerializeField] private TextMeshProUGUI obengCountText;
    [SerializeField] private TextMeshProUGUI tangCountText;
    [SerializeField] private TextMeshProUGUI kunciCountText;

    private int obengCount = 0;
    private int tangCount = 0;
    private int kunciCount = 0;

    // Referensi ke script PlayerHealth (untuk mendapatkan darah saat ini)
    private PlayerHealth playerHealthRef; 

    // Getters untuk jumlah item (jika script lain perlu membaca)
    public int GetObengCount() { return obengCount; }
    public int GetTangCount() { return tangCount; }
    public int GetKunciCount() { return kunciCount; }

    void Awake()
    {
        UpdateItemDisplay(); // Perbarui display item saat game dimulai
        
        // Dapatkan referensi PlayerHealth saat script ini bangun.
        // FindObjectOfType mencari objek pertama di scene yang memiliki komponen PlayerHealth.
        playerHealthRef = FindObjectOfType<PlayerHealth>(); 
        if (playerHealthRef == null)
        {
            Debug.LogError("PlayerHealth script not found in scene. Health UI will not function correctly.", this);
        }
    }

    void OnEnable()
    {
        // Langganan ke event perubahan darah dari PlayerHealth
        PlayerHealth.OnHealthChanged += UpdateHealthDisplay;
        // Langganan ke event pengumpulan item dari CollectibleItem (jika ada)
        CollectibleItem.OnItemCollected += HandleItemCollected;

        // Paksa update UI dengan darah saat ini ketika HealthUI diaktifkan.
        // Ini adalah kunci untuk memastikan UI menampilkan status darah yang benar sejak awal game.
        if (playerHealthRef != null)
        {
            UpdateHealthDisplay(playerHealthRef.GetCurrentHealth());
        }
    }

    void OnDisable()
    {
        // Berhenti berlangganan saat script dinonaktifkan untuk mencegah error atau memori leak
        PlayerHealth.OnHealthChanged -= UpdateHealthDisplay;
        CollectibleItem.OnItemCollected -= HandleItemCollected;
    }

    private void UpdateHealthDisplay(int currentHealth)
    {
        // Pastikan array healthIcons sudah diisi di Inspector dan tidak kosong
        if (healthIcons == null || healthIcons.Length == 0)
        {
            Debug.LogWarning("Health Icons array is not assigned or is empty in HealthUI. Make sure to assign all heart Image GameObjects in the Inspector.", this);
            return;
        }

        // Loop melalui setiap gambar hati yang ada di array
        for (int i = 0; i < healthIcons.Length; i++)
        {
            // Pastikan elemen array itu sendiri tidak null (misal, jika ada slot kosong di Inspector)
            if (healthIcons[i] != null)
            {
                // Jika indeks hati (i) lebih kecil dari jumlah darah saat ini,
                // berarti hati tersebut harus ditampilkan sebagai hati penuh.
                // Contoh: Jika currentHealth = 3:
                // i=0 (0<3) -> fullHeart
                // i=1 (1<3) -> fullHeart
                // i=2 (2<3) -> fullHeart
                // Contoh: Jika currentHealth = 2:
                // i=0 (0<2) -> fullHeart
                // i=1 (1<2) -> fullHeart
                // i=2 (2 BUKAN < 2) -> emptyHeart
                if (i < currentHealth)
                {
                    healthIcons[i].sprite = fullHeartSprite;
                    healthIcons[i].enabled = true; // Pastikan gambar terlihat
                }
                else // Jika tidak, berarti hati itu kosong
                {
                    healthIcons[i].sprite = emptyHeartSprite;
                    healthIcons[i].enabled = true; // Pastikan gambar terlihat
                }
            }
        }
    }

    private void HandleItemCollected(string itemType)
    {
        switch (itemType)
        {
            case "Obeng":
                obengCount++;
                break;
            case "Tang":
                tangCount++;
                break;
            case "Kunci":
                kunciCount++;
                break;
            default:
                Debug.LogWarning($"Tipe item tidak dikenal: {itemType}", this);
                break;
        }
        UpdateItemDisplay(); // Perbarui display item setelah item terkumpul
    }

    private void UpdateItemDisplay()
    {
        if (obengCountText != null)
        {
            obengCountText.text = $": {obengCount}";
        }
        if (tangCountText != null)
        {
            tangCountText.text = $": {tangCount}";
        }
        if (kunciCountText != null)
        {
            kunciCountText.text = $": {kunciCount}";
        }
    }
}