using UnityEngine;
using System.Collections; // Perlu ini untuk Coroutine

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private int enemyHealth = 1; 
    [SerializeField] private Animator anim; 

    // Durasi animasi kematian. ISI INI DI INSPECTOR setelah tahu durasi klip animasi enemy_dead.
    [Tooltip("Durasi total animasi mati (enemy_dead.anim). ISI INI DI INSPECTOR!")]
    [SerializeField] private float deathAnimationDuration = 1.0f; // Sesuaikan di Inspector!

    // Pengaturan Efek Kedip-kedip
    [Header("Blink Effect on Hit")]
    [SerializeField] private float blinkDuration = 0.5f; // Total durasi kedip-kedip sebelum menghilang
    [SerializeField] private Color flashColor = Color.white; // Warna saat kedip (misal: putih atau merah)
    [SerializeField] private int blinkCount = 3; // Berapa kali akan berkedip

    // --- BARU: Pengaturan Animasi Serangan ---
    [Header("Attack Animation Settings")]
    [SerializeField] private float idleBeforeAttackDuration = 2.0f; // Berapa lama idle sebelum menyerang
    [SerializeField] private float attackAnimationDuration = 1.0f; // Durasi klip animasi serangan
    // --- AKHIR BARU ---

    private SpriteRenderer spriteRenderer; // Referensi ke SpriteRenderer monster
    private Color originalColor; // Menyimpan warna asli monster

    private bool isDead = false; 
    private bool isAttacking = false; // Flag baru untuk melacak apakah sedang dalam animasi serangan

    void Awake()
    {
        // Pastikan komponen Animator dan SpriteRenderer ada
        if (anim == null)
        {
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError("Animator component not found on " + gameObject.name + ". Please add an Animator component and assign a Controller.", this);
            }
        }
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name + ".", this);
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // Simpan warna asli saat Awake
        }

        // --- PENTING: Pastikan parameter "isDead" di Animator diatur ke false saat game dimulai ---
        if (anim != null)
        {
            anim.SetBool("isDead", false); 
        }

        // --- BARU: Mulai loop perilaku monster (idle, attack, dll.) ---
        StartCoroutine(EnemyBehaviorLoop());
    }

    // Fungsi untuk monster menerima damage (jika monster punya HP)
    public void TakeDamage(int amount)
    {
        if (isDead) return; 

        enemyHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current HP: {enemyHealth}");

        // Panggil efek kedip saat kena damage, tapi hanya jika tidak mati
        if (enemyHealth > 0) 
        {
            StartCoroutine(BlinkEffect()); 
        }

        if (enemyHealth <= 0)
        {
            Die();
        }
    }

    // Fungsi untuk monster mati
    public void Die()
    {
        if (isDead) return; 

        isDead = true;
        Debug.Log($"{gameObject.name} has died!");

        // --- BARU: Hentikan coroutine perilaku monster saat mati ---
        StopAllCoroutines(); // Menghentikan EnemyBehaviorLoop dan BlinkEffect jika sedang berjalan
        // --- AKHIR BARU ---

        // Nonaktifkan collider dan Rigidbody saat monster mati segera
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector2.zero;
            enemyRb.bodyType = RigidbodyType2D.Kinematic; // Menghentikan fisika
        }

        // Mulai coroutine yang mengatur urutan kematian: animasi -> kedip -> hancur
        StartCoroutine(DeathSequence()); 
    }

    // --- BARU: Coroutine untuk loop perilaku monster (idle dan attack) ---
    private IEnumerator EnemyBehaviorLoop()
    {
        while (!isDead) // Loop selama monster belum mati
        {
            // 1. Play Idle Animation
            if (anim != null)
            {
                // Anda bisa menggunakan SetTrigger atau SetBool di Animator Controller
                // Contoh jika Anda punya parameter boolean "IsAttacking"
                // anim.SetBool("IsAttacking", false); 
                anim.Play("enemy_idle"); // Pastikan nama state di Animator sama persis
            }

            yield return new WaitForSeconds(idleBeforeAttackDuration); // Tunggu selama durasi idle

            if (isDead) break; // Keluar dari loop jika mati saat sedang idle

            // 2. Play Attack Animation
            isAttacking = true;
            if (anim != null)
            {
                // anim.SetBool("IsAttacking", true); // Jika menggunakan boolean parameter
                anim.Play("enemy_attack"); // Pastikan nama state di Animator sama persis
            }
            yield return new WaitForSeconds(attackAnimationDuration); // Tunggu selama durasi animasi serangan
            isAttacking = false;

            if (isDead) break; // Keluar dari loop jika mati saat sedang menyerang
        }
        Debug.Log("EnemyBehaviorLoop stopped because enemy is dead.");
    }
    // --- AKHIR BARU ---


    // --- Coroutine Baru untuk Urutan Kematian ---
    private IEnumerator DeathSequence()
    {
        // 1. Memainkan animasi kematian monster
        if (anim != null)
        {
            anim.SetBool("isDead", true); 
            Debug.Log($"Animator parameter 'isDead' set to true. Waiting for {deathAnimationDuration} seconds for death animation.");
            yield return new WaitForSeconds(deathAnimationDuration); 
        }
        else
        {
            Debug.LogWarning("Animator not found. Skipping death animation wait.");
        }

        // 2. Setelah animasi mati selesai, baru mulai efek kedip-kedip sebelum menghilang
        StartCoroutine(BlinkAndDisappear());
    }

    // --- Coroutine untuk efek kedip-kedip saat menerima damage (tanpa mati) ---
    private IEnumerator BlinkEffect()
    {
        // Pastikan tidak berkedip jika sudah dalam kondisi menyerang yang mengubah sprite (jika ada)
        // Atau jika monster sedang mati.
        if (isDead || isAttacking) yield break; // Hindari konflik jika attack animation mengubah warna

        float singleBlinkPhaseDuration = 0.05f; 
        int temporaryBlinkCount = 1; 

        for (int i = 0; i < temporaryBlinkCount; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = flashColor; 
            yield return new WaitForSeconds(singleBlinkPhaseDuration); 
            // Pastikan tidak di-set ke original jika sudah mati, karena DeathSequence akan mengambil alih
            if (spriteRenderer != null && !isDead) spriteRenderer.color = originalColor; 
            yield return new WaitForSeconds(singleBlinkPhaseDuration); 
        }
        // Pastikan warna kembali normal setelah kedip selesai, kecuali jika sudah mati
        if (spriteRenderer != null && !isDead) spriteRenderer.color = originalColor; 
    }

    // --- Coroutine untuk efek kedip-kedip dan kemudian menghilang/hancur ---
    private IEnumerator BlinkAndDisappear()
    {
        float singleBlinkPhaseDuration = (blinkCount > 0) ? blinkDuration / (blinkCount * 2) : 0.1f; 
        
        for (int i = 0; i < blinkCount; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = flashColor; 
            yield return new WaitForSeconds(singleBlinkPhaseDuration); 
            if (spriteRenderer != null) spriteRenderer.color = originalColor; 
            yield return new WaitForSeconds(singleBlinkPhaseDuration); 
        }

        Destroy(gameObject); 
    }
}