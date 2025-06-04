using UnityEngine;
using System; // Untuk Action
using System.Collections; // Untuk Coroutine jika perlu, tapi bobbing di Update

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))] // Meskipun Kinematic, perlu Rigidbody2D untuk Trigger
public class CollectibleItem : MonoBehaviour
{
    // Enum untuk mengidentifikasi tipe item
    public enum ItemType { Obeng, Tang, Kunci }

    [Header("Item Type")]
    [Tooltip("Pilih tipe item ini.")]
    [SerializeField] private ItemType itemType;

    [Header("Floating Animation Settings")]
    [SerializeField] private float floatHeight = 0.1f; // Ketinggian maksimal mengambang (amplitudo)
    [SerializeField] private float bobSpeed = 2f;     // Kecepatan mengambang

    private Vector3 startPosition; // Posisi awal item

    // Event statis yang akan dipanggil saat item diambil
    // string: nama item (misal "Obeng", "Tang", "Kunci")
    public static event Action<string> OnItemCollected;

    void Awake()
    {
        startPosition = transform.position; // Simpan posisi awal saat Awake
        
        // Pastikan collider adalah trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set as Is Trigger. Setting it now.", this);
            col.isTrigger = true;
        }

        // Pastikan Rigidbody2D adalah Kinematic
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning($"Rigidbody2D on {gameObject.name} is not set to Kinematic. Setting it now.", this);
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        // Efek mengambang (bobbing) menggunakan fungsi sinus
        // Mathf.Sin(Time.time * bobSpeed) akan menghasilkan nilai antara -1 dan 1
        // Dikalikan floatHeight untuk menentukan ketinggian
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * floatHeight);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Deteksi jika yang menyentuh adalah pemain
        if (other.CompareTag("Player"))
        {
            // Panggil event untuk memberitahu UI atau sistem lain
            OnItemCollected?.Invoke(itemType.ToString()); // Mengirim nama item sebagai string

            Debug.Log($"Collected: {itemType}");
            Destroy(gameObject); // Hancurkan item setelah diambil
        }
    }
}