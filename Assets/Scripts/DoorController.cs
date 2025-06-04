using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Nama Scene berikutnya yang akan dimuat (misal: Level2). Pastikan di Build Settings!")]
    [SerializeField] private string nextSceneName = "Level2";
    [Tooltip("Sprite pintu saat terkunci")]
    [SerializeField] private Sprite lockedDoorSprite;
    [Tooltip("Sprite pintu saat terbuka")]
    [SerializeField] private Sprite unlockedDoorSprite;

    [Header("Required Items to Unlock")]
    [SerializeField] private int requiredObeng = 1;
    [SerializeField] private int requiredTang = 1;
    [SerializeField] private int requiredKunci = 1;

    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;
    
    // --- PENTING: Gunakan [SerializeField] untuk HealthUI di sini ---
    [Tooltip("Seret GameObject yang memiliki script HealthUI (biasanya Canvas) ke slot ini di Inspector.")]
    [SerializeField] private HealthUI healthUI; // Sekarang diset melalui Inspector
    // --- AKHIR PENTING ---

    private bool isUnlocked = false; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Log untuk memastikan komponen ditemukan
        if (spriteRenderer == null) Debug.LogError("Door: SpriteRenderer not found!", this);
        if (doorCollider == null) Debug.LogError("Door: Collider2D not found!", this);
        
        // PENTING: Cek apakah healthUI sudah terhubung di Inspector
        if (healthUI == null)
        {
            Debug.LogError("Door: HealthUI reference is NULL in Inspector! Please assign it.", this);
        }

        // Atur pintu pada kondisi awal: terkunci dan solid (tidak bisa dilewati)
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false; 
            Debug.Log($"Door '{gameObject.name}' Awake: Initial isTrigger set to {doorCollider.isTrigger} (solid).");
        }
        if (spriteRenderer != null)
        {
            if (lockedDoorSprite != null)
            {
                spriteRenderer.sprite = lockedDoorSprite; 
            }
            else
            {
                Debug.LogWarning("Door: Locked Door Sprite is not assigned!", this);
            }
        }
        
        UpdateDoorState(); // Cek status pintu saat pertama kali aktif
    }

    void OnEnable()
    {
        // Berlangganan event saat objek aktif
        // Pastikan healthUI tidak NULL sebelum mencoba subscribe
        if (healthUI != null)
        {
            CollectibleItem.OnItemCollected += OnItemCollectedHandler;
            Debug.Log("DoorController subscribed to OnItemCollected.");
        }
        else
        {
            Debug.LogError("DoorController OnEnable: HealthUI is NULL, cannot subscribe to item events. Assign in Inspector!", this);
        }
    }

    void OnDisable()
    {
        // Berhenti berlangganan saat objek dinonaktifkan atau dihancurkan
        // Pastikan healthUI tidak NULL sebelum mencoba unsubscribe
        if (healthUI != null) 
        {
            CollectibleItem.OnItemCollected -= OnItemCollectedHandler;
            Debug.Log("DoorController unsubscribed from OnItemCollected.");
        }
        else
        {
            Debug.LogWarning("DoorController OnDisable: healthUI was NULL, could not unsubscribe. (This might be normal if the object is being destroyed).");
        }
    }

    private void OnItemCollectedHandler(string itemType)
    {
        Debug.Log($"DoorController: Item '{itemType}' collected. Re-checking door state.");
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        if (healthUI == null)
        {
            Debug.LogWarning("UpdateDoorState: HealthUI is NULL. Cannot check item counts. Please assign HealthUI in Inspector.", this);
            return;
        }

        int currentObeng = healthUI.GetObengCount();
        int currentTang = healthUI.GetTangCount();
        int currentKunci = healthUI.GetKunciCount();

        Debug.Log($"Door State: Current items - Obeng:{currentObeng}, Tang:{currentTang}, Kunci:{currentKunci}");
        Debug.Log($"Door State: Required items - Obeng:{requiredObeng}, Tang:{requiredTang}, Kunci:{requiredKunci}");

        bool allItemsCollected = 
            currentObeng >= requiredObeng &&
            currentTang >= requiredTang &&
            currentKunci >= requiredKunci;

        Debug.Log($"Door State: All items collected status is {allItemsCollected}. isUnlocked is {isUnlocked}.");

        if (allItemsCollected && !isUnlocked)
        {
            UnlockDoor();
        }
        else if (!allItemsCollected && isUnlocked) 
        {
            LockDoor(); 
        }
        else if (allItemsCollected && isUnlocked) 
        {
            Debug.Log("Door State: Door is already unlocked and all items collected.");
        }
        else 
        {
            Debug.Log("Door State: Not all items collected yet. Door remains locked.");
        }
    }

    private void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("UnlockDoor: Method called. Setting isUnlocked to true.");

        if (spriteRenderer != null)
        {
            if (unlockedDoorSprite != null)
            {
                spriteRenderer.sprite = unlockedDoorSprite;
                Debug.Log("UnlockDoor: Changed sprite to Unlocked Door.");
            }
            else
            {
                Debug.LogWarning("UnlockDoor: Unlocked Door Sprite is not assigned!", this);
            }
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = true; 
            Debug.Log($"UnlockDoor: Door collider isTrigger set to {doorCollider.isTrigger} (passable).");
        }
        else
        {
            Debug.LogError("UnlockDoor: doorCollider is NULL. Cannot set isTrigger.", this);
        }
    }

    private void LockDoor()
    {
        isUnlocked = false;
        Debug.Log("LockDoor: Method called. Setting isUnlocked to false.");

        if (spriteRenderer != null && lockedDoorSprite != null)
        {
            spriteRenderer.sprite = lockedDoorSprite;
            Debug.Log("LockDoor: Changed sprite to Locked Door.");
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = false; 
            Debug.Log($"LockDoor: Door collider isTrigger set to {doorCollider.isTrigger} (solid).");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log($"OnTriggerEnter2D: Player '{other.gameObject.name}' entered door area. Door's current isUnlocked status: {isUnlocked}.");
            if (isUnlocked)
            {
                Debug.Log($"OnTriggerEnter2D: Door is unlocked! Loading scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName); 
            }
            else
            {
                if (healthUI != null)
                {
                    Debug.Log($"OnTriggerEnter2D: Door is locked! Please collect all items. Current: O:{healthUI.GetObengCount()}/R:{requiredObeng}, T:{healthUI.GetTangCount()}/R:{requiredTang}, K:{healthUI.GetKunciCount()}/R:{requiredKunci}");
                }
                else
                {
                    Debug.Log("OnTriggerEnter2D: Door is locked! (HealthUI not found to show missing items).");
                }
            }
        }
        else
        {
            Debug.Log($"OnTriggerEnter2D: Non-player object '{other.gameObject.name}' entered door area. Tag: {other.tag}");
        }
    }
}