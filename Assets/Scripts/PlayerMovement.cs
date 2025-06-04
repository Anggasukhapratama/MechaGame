using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;
    public float slideSpeed = 10f;
    public float slideDuration = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController; 

    // Referensi ke script PlayerHealth (seret di Inspector jika belum otomatis terisi)
    [SerializeField] private PlayerHealth playerHealth; 

    private float mobileInputX = 0f; // Input dari tombol mobile UI

    private Vector2 moveInput;
    private bool isJumping = false;
    public bool isSliding = false; 
    private float slideTime;

    // Enum untuk mengelola state animasi
    private enum MovementState { idle, run, jump, slide, dead } 

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround; // Layer untuk tanah yang bisa diinjak
    private BoxCollider2D coll; 

    private bool isPlayerDead = false; // Flag untuk melacak status kematian pemain

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>(); 

        playerController = new PlayerController(); // Inisialisasi Input System

        // Pastikan playerHealth terisi, kalau belum coba ambil otomatis
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        playerController.Enable(); // Aktifkan input system

        // Berlangganan event input dari Input System
        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Slide.performed += ctx => Slide();

        // Berlangganan event kematian dan respawn dari PlayerHealth
        PlayerHealth.OnPlayerDied += HandlePlayerDeath; 
        PlayerHealth.OnPlayerRespawned += HandlePlayerRespawn; 
    }

    private void OnDisable()
    {
        playerController.Disable(); // Nonaktifkan input system
        // Berhenti berlangganan event
        PlayerHealth.OnPlayerDied -= HandlePlayerDeath; 
        PlayerHealth.OnPlayerRespawned -= HandlePlayerRespawn; 
    }

    private void Update()
    {
        if (isPlayerDead) return; // Jangan lakukan apapun jika pemain mati

        moveInput = playerController.Movement.Move.ReadValue<Vector2>(); // Baca input keyboard/gamepad

        // Logika durasi slide
        if (isSliding)
        {
            slideTime -= Time.deltaTime;
            if (slideTime <= 0)
            {
                isSliding = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // Jika pemain mati, biarkan Rigidbody dikendalikan oleh PlayerHealth.
        // JANGAN atur velocity = Vector2.zero di sini saat mati.
        if (isPlayerDead) 
        {
            return; // Cukup kembali saja, jangan lakukan kontrol movement lain
        }

        // Kontrol gerakan horizontal (jika tidak sedang slide)
        if (!isSliding)
        {
            float combinedHorizontalInput = moveInput.x + mobileInputX; // Gabungkan input keyboard dan mobile
            float speed = Mathf.Abs(combinedHorizontalInput) > 0.7f ? runSpeed : moveSpeed; // Lari jika input kuat
            Vector2 targetVelocity = new Vector2(combinedHorizontalInput * speed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }

        UpdateAnimation(); // Perbarui animasi pemain

        // Cek apakah pemain di tanah untuk reset isJumping
        if (isGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }

    private void UpdateAnimation()
    {
        MovementState state;

        if (isPlayerDead) 
        {
            state = MovementState.dead; // Jika mati, set animasi mati
        }
        else if (isSliding)
        {
            state = MovementState.slide; // Jika slide, set animasi slide
        }
        else if (rb.velocity.y > 0.1f || rb.velocity.y < -0.1f) // Cek vertikal untuk lompat/jatuh
        {
            state = MovementState.jump; // Jika melompat/jatuh, set animasi jump
        }
        else 
        {
            float horizontal = moveInput.x + mobileInputX; 
            if (horizontal != 0f)
            {
                state = MovementState.run; // Jika bergerak horizontal, set animasi run
                sprite.flipX = horizontal < 0f; // Balik sprite sesuai arah
            }
            else
            {
                state = MovementState.idle; // Jika diam, set animasi idle
            }
        }

        anim.SetInteger("state", (int)state); // Set parameter Animator
    }

    // Cek apakah pemain berada di tanah
    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    // Fungsi lompat
    private void Jump()
    {
        if (isPlayerDead) return; // Jangan lompat jika mati
        if (isGrounded() && !isSliding) // Hanya bisa lompat jika di tanah dan tidak sedang slide
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }

    // Fungsi slide
    private void Slide()
    {
        if (isPlayerDead) return; // Jangan slide jika mati
        float horizontal = moveInput.x + mobileInputX; 
        if (isGrounded() && !isSliding && horizontal != 0) // Hanya bisa slide jika di tanah, tidak sedang slide, dan ada input horizontal
        {
            isSliding = true;
            slideTime = slideDuration;
            rb.velocity = new Vector2(Mathf.Sign(horizontal) * slideSpeed, rb.velocity.y);
        }
    }

    // Fungsi untuk tombol mobile (geser ke kanan)
    public void MoveRight(bool isPressed)
    {
        if (isPlayerDead) { mobileInputX = 0f; return; } 
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f) 
            mobileInputX = 0f;
    }

    // Fungsi untuk tombol mobile (geser ke kiri)
    public void MoveLeft(bool isPressed)
    {
        if (isPlayerDead) { mobileInputX = 0f; return; } 
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f) 
            mobileInputX = 0f;
    }

    // Fungsi untuk tombol mobile (lompat)
    public void MobileJump()
    {
        if (isPlayerDead) return; 
        Jump();
    }

    // Fungsi untuk tombol mobile (slide)
    public void MobileSlide()
    {
        if (isPlayerDead) return; 
        Slide();
    }

    // --- INTERAKSI FISIK (COLLISION) ---
    // Fungsi ini menangani semua tabrakan fisik pemain (Is Trigger TIDAK DICENTANG)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Jangan proses collision jika pemain sudah mati
        if (isPlayerDead) return;

        // Cek jika yang bertabrakan adalah monster
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (isSliding)
                {
                    // Jika pemain sedang slide, monster mati
                    Debug.Log("Sliding into enemy! Enemy dies.");
                    enemy.Die(); 
                }
                else
                {
                    // Jika pemain tidak slide, pemain mengambil damage
                    Debug.Log("Touched enemy normally. Player takes damage.");
                    playerHealth.TakeDamage(1); // Memicu TakeDamage yang akan memanggil HandleDeathByStopping
                    // Optional: Dorong pemain sedikit ke belakang
                    // Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    // rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
                }
            }
        }
        // Cek jika yang bertabrakan adalah jebakan
        else if (collision.gameObject.CompareTag("Trap"))
        {
            // Untuk jebakan, selalu kurangi darah pemain, tidak peduli slide atau tidak
            Debug.Log("Hit a trap! Player takes damage.");
            playerHealth.TakeDamage(1); // Memicu TakeDamage yang akan memanggil HandleDeathByStopping
            // Optional: Dorong pemain sedikit ke belakang
            // Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            // rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
        }
    }


    // --- FUNGSI UNTUK MENANGANI KEMATIAN PEMAIN (DIPANGGIL DARI PLAYERHEALTH EVENT) ---
    private void HandlePlayerDeath()
    {
        isPlayerDead = true; 
        anim.SetInteger("state", (int)MovementState.dead); 
        
        // Hapus `rb.velocity = Vector2.zero;` di sini, biarkan PlayerHealth yang mengatur Rigidbody.
        // rb.velocity = Vector2.zero; 

        playerController.Disable(); // Nonaktifkan input dari Input System
        mobileInputX = 0f; // Pastikan input mobile juga reset
        isSliding = false; // Pastikan status sliding direset
    }

    // --- FUNGSI UNTUK MENANGANI RESPPAWN PEMAIN ---
    private void HandlePlayerRespawn()
    {
        isPlayerDead = false; 
        playerController.Enable(); // Aktifkan kembali input

        anim.SetInteger("state", (int)MovementState.idle); // Set animasi kembali ke idle saat respawn
        Debug.Log("Player has respawned!");
    }
}