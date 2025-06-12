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

    public static event Action<int> OnHealthChanged;
    public static event Action OnPlayerDied; 
    public static event Action OnPlayerRespawned; 
    public static event Action OnPlayerHurt;

    private Collider2D playerCollider; 
    private Rigidbody2D playerRigidbody; 

    private Vector3 respawnPosition; 

    void Awake()
    {
        currentHealth = maxHealth; 
        OnHealthChanged?.Invoke(currentHealth); 

        playerCollider = GetComponent<Collider2D>(); 
        playerRigidbody = GetComponent<Rigidbody2D>(); 

        respawnPosition = transform.position; 
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0 || currentHealth <= 0) return;

        int previousHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {amount} damage. Current HP: {currentHealth}");

        // Panggil animasi Hurt SELALU, baik sebelum atau sesudah mati
        if (previousHealth > 0)
        {
            OnPlayerHurt?.Invoke();
        }

        OnHealthChanged?.Invoke(currentHealth); 

        if (currentHealth <= 0)
        {
            Debug.Log("PlayerHealth: Darah habis. Memicu kematian.");
            HandleDeathByStopping(); 
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0 || currentHealth <= 0) return; 

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); 

        Debug.Log($"Player healed {amount}. Current HP: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth); 
    }

    private void CommonDeathLogic()
    {
        OnHealthChanged?.Invoke(currentHealth); 
        OnPlayerDied?.Invoke(); 

        this.enabled = false; 
        Invoke("RespawnPlayer", 2f); 
    }

    private void HandleDeathBySinking()
    {
        Debug.Log("Player died by sinking (Acid/DeathZone).");
        currentHealth = 0; 
        CommonDeathLogic(); 

        if (playerRigidbody != null)
        {
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic; 
            playerRigidbody.velocity = Vector2.zero; 
            playerRigidbody.angularVelocity = 0f; 
            
            if (playerCollider != null)
            {
                playerCollider.enabled = true; 
            }
            playerRigidbody.gravityScale = 0.2f; 
        }
    }

    private void HandleDeathByStopping()
    {
        Debug.Log("Player died by stopping (Damage/Trap).");
        currentHealth = 0; 
        CommonDeathLogic(); 

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector2.zero; 
            playerRigidbody.angularVelocity = 0f; 
            playerRigidbody.bodyType = RigidbodyType2D.Kinematic; 
            
            if (playerCollider != null)
            {
                playerCollider.enabled = true; 
            }
        }
    }

    private void RespawnPlayer()
    {
        Debug.Log("Player respawning...");
        
        transform.position = respawnPosition; 

        currentHealth = maxHealth; 
        OnHealthChanged?.Invoke(currentHealth); 

        if (playerRigidbody != null)
        {
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic; 
            playerRigidbody.velocity = Vector2.zero; 
            playerRigidbody.angularVelocity = 0f; 
            playerRigidbody.gravityScale = 1f; 
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = true; 
        }

        this.enabled = true; 
        OnPlayerRespawned?.Invoke(); 
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (currentHealth <= 0) return; 

        if (other.CompareTag("HealthPickup"))
        {
            Heal(1); 
            Destroy(other.gameObject); 
        }
        else if (other.CompareTag("DeathZone") || other.CompareTag("Acid")) 
        {
            HandleDeathBySinking(); 
        }
    }
}
