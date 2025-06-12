// File: HealthUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System; 
using UnityEngine.SceneManagement; // <-- TAMBAH INI

public class HealthUI : MonoBehaviour
{
    [Header("Player Health UI")]
    [SerializeField] private Image[] healthIcons; 
    [SerializeField] private Sprite fullHeartSprite; 
    [SerializeField] private Sprite emptyHeartSprite; 
    
    [Header("Item Count UI")]
    [SerializeField] private TextMeshProUGUI obengCountText;
    [SerializeField] private TextMeshProUGUI tangCountText;
    [SerializeField] private TextMeshProUGUI kunciCountText;

    private int obengCount = 0;
    private int tangCount = 0;
    private int kunciCount = 0;

    private PlayerHealth playerHealthRef; 

    public int GetObengCount() { return obengCount; }
    public int GetTangCount() { return tangCount; }
    public int GetKunciCount() { return kunciCount; }

    void Awake()
    {
        // ResetAllItemCounts(); // <-- HAPUS ATAU KOMENTARI PANGGILAN INI DARI AWAKE
        // Jika HealthUI Anda tidak DontDestroyOnLoad, Awake akan dipanggil setiap scene dan reset alami.
        // Namun, kita akan menggunakan SceneManager.sceneLoaded sebagai pemicu utama.

        playerHealthRef = FindObjectOfType<PlayerHealth>(); 
        if (playerHealthRef == null)
        {
            Debug.LogError("HealthUI: PlayerHealth script not found in scene. Health UI will not function correctly.", this);
        }
    }

    void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthDisplay;
        CollectibleItem.OnItemCollected += HandleItemCollected;
        // HAPUS LANGGANAN INI, KARENA INI YANG MEMBUAT RESET SAAT MATI
        // PlayerHealth.OnPlayerRespawned += ResetAllItemCounts; 

        // LANGGANAN KE EVENT SAAT SCENE BARU DIMUAT
        SceneManager.sceneLoaded += OnSceneLoaded; // <-- BARU DITAMBAHKAN
        
        if (playerHealthRef != null)
        {
            UpdateHealthDisplay(playerHealthRef.GetCurrentHealth());
        }
        else // Jika PlayerHealth belum ditemukan di Awake (misal, urutan eksekusi)
        {
            // Lakukan update UI untuk awal scene (misal, 0 item)
            UpdateItemDisplay();
        }
    }

    void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthDisplay;
        CollectibleItem.OnItemCollected -= HandleItemCollected;
        // HAPUS JUGA BERHENTI LANGGANAN INI
        // PlayerHealth.OnPlayerRespawned -= ResetAllItemCounts; 

        // BERHENTI LANGGANAN SAAT SCENE BARU DIMUAT
        SceneManager.sceneLoaded -= OnSceneLoaded; // <-- BARU DITAMBAHKAN
    }

    private void UpdateHealthDisplay(int currentHealth)
    {
        if (healthIcons == null || healthIcons.Length == 0)
        {
            Debug.LogWarning("HealthUI: Health Icons array is not assigned or is empty. Assign heart Image GameObjects in the Inspector.", this);
            return;
        }

        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                if (i < currentHealth)
                {
                    healthIcons[i].sprite = fullHeartSprite;
                    healthIcons[i].enabled = true; 
                }
                else 
                {
                    healthIcons[i].sprite = emptyHeartSprite;
                    healthIcons[i].enabled = true; 
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
                Debug.LogWarning($"HealthUI: Unknown item type collected: {itemType}", this);
                break;
        }
        UpdateItemDisplay(); 
    }

    // FUNGSI BARU: Dipanggil saat scene baru dimuat
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset item counts hanya saat scene baru dimuat (bukan saat player mati di tengah level)
        Debug.Log($"HealthUI: Scene '{scene.name}' loaded. Resetting item counts.");
        ResetAllItemCounts(); 
    }

    // FUNGSI UNTUK MERESET SEMUA JUMLAH ITEM
    private void ResetAllItemCounts()
    {
        obengCount = 0;
        tangCount = 0;
        kunciCount = 0;
        UpdateItemDisplay(); 
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