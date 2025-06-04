using UnityEngine;
using System.Collections; // Digunakan untuk Coroutine

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SawTrap : MonoBehaviour
{
    // Deklarasi enum sebaiknya diletakkan di sini,
    // di dalam kelas tetapi di luar atribut [Header] untuk field lain.
    public enum MovementType { None, Horizontal, Vertical } // <--- PINDAHKAN INI KE SINI

    [Header("Rotation Settings")]
    [Tooltip("Kecepatan gergaji berputar dalam derajat per detik.")]
    [SerializeField] private float rotationSpeed = 300f; // Kecepatan rotasi

    [Header("Damage Settings")]
    [Tooltip("Jumlah darah yang akan dikurangi dari pemain setiap kali terkena jebakan.")]
    [SerializeField] private int damageAmount = 1; // Jumlah damage

    [Header("Movement Settings")] // <--- Header ini sekarang benar di atas field 'movementType'
    [Tooltip("Tipe pergerakan jebakan: Tidak bergerak, Horizontal, atau Vertikal.")]
    [SerializeField] private MovementType movementType = MovementType.None;

    [Tooltip("Jarak total pergerakan dari titik awal ke satu sisi. (misal: 2 = 2 unit ke kiri/kanan atau atas/bawah dari posisi awal)")]
    [SerializeField] private float moveDistance = 2f; // Jarak bergerak dari posisi awal
    [Tooltip("Kecepatan pergerakan bolak-balik.")]
    [SerializeField] private float moveSpeed = 3f; // Kecepatan bergerak

    [Tooltip("Jeda sebelum jebakan mulai bergerak (opsional).")]
    [SerializeField] private float startDelay = 0f;
    [Tooltip("Jeda waktu (dalam detik) ketika jebakan mencapai ujung pergerakannya.")]
    [SerializeField] private float pauseAtEndPoints = 0.5f;

    private Vector3 initialPosition; // Posisi awal jebakan
    private Vector3 targetPosition; // Posisi tujuan saat ini
    private bool movingToEnd = true; // Menentukan apakah sedang bergerak ke posisi akhir atau kembali

    private Rigidbody2D rb;
    private Collider2D trapCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trapCollider = GetComponent<Collider2D>();

        // Pastikan Rigidbody2D diatur ke Kinematic agar tidak terpengaruh fisika lain
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Pastikan Collider2D diatur ke Trigger agar bisa dideteksi tanpa tabrakan fisik
        if (trapCollider != null && !trapCollider.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set as Is Trigger. Setting it now.", this);
            trapCollider.isTrigger = true;
        }

        initialPosition = transform.position; // Simpan posisi awal

        // Tentukan posisi target awal berdasarkan tipe pergerakan
        if (movementType == MovementType.Horizontal)
        {
            targetPosition = initialPosition + Vector3.right * moveDistance;
        }
        else if (movementType == MovementType.Vertical)
        {
            targetPosition = initialPosition + Vector3.up * moveDistance;
        }
    }

    void Start()
    {
        // Mulai coroutine pergerakan jika tipe pergerakan bukan None
        if (movementType != MovementType.None)
        {
            StartCoroutine(MoveTrapRoutine());
        }
    }

    void Update()
    {
        // Rotasi gergaji setiap frame
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator MoveTrapRoutine()
    {
        yield return new WaitForSeconds(startDelay); // Jeda awal

        while (true) // Loop terus-menerus untuk pergerakan
        {
            Vector3 startOfPath;
            Vector3 endOfPath;

            if (movementType == MovementType.Horizontal)
            {
                startOfPath = initialPosition - Vector3.right * moveDistance;
                endOfPath = initialPosition + Vector3.right * moveDistance;
            }
            else // Vertical
            {
                startOfPath = initialPosition - Vector3.up * moveDistance;
                endOfPath = initialPosition + Vector3.up * moveDistance;
            }

            Vector3 currentTarget = movingToEnd ? endOfPath : startOfPath;

            // Pindahkan jebakan menuju target saat ini
            while (Vector3.Distance(transform.position, currentTarget) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
                yield return null; // Tunggu satu frame
            }

            // Jebakan telah mencapai target, balikkan arah dan jeda
            transform.position = currentTarget; // Pastikan posisi tepat di target
            movingToEnd = !movingToEnd; // Balikkan arah
            yield return new WaitForSeconds(pauseAtEndPoints); // Jeda di ujung
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Deteksi jika yang menyentuh adalah pemain
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Player terkena jebakan gergaji! Damage: {damageAmount}");
            }
            else
            {
                Debug.LogWarning("Player entered Saw Trap but no PlayerHealth script found!", this);
            }
        }
    }
}