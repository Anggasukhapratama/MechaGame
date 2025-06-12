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

    [Tooltip("Seret GameObject yang memiliki script HealthUI (biasanya Canvas) ke slot ini di Inspector.")]
    [SerializeField] private HealthUI healthUI; 

    private bool isUnlocked = false; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null) Debug.LogError("DoorController: SpriteRenderer not found!", this);
        if (doorCollider == null) Debug.LogError("DoorController: Collider2D not found!", this);
        
        if (healthUI == null)
        {
            // Ini akan muncul di konsol jika Anda lupa menyeret HealthUI di Inspector
            Debug.LogError($"DoorController on '{gameObject.name}': HealthUI reference is NULL in Inspector! Please assign it.", this);
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = false; 
            Debug.Log($"DoorController on '{gameObject.name}': Initial collider isTrigger set to {doorCollider.isTrigger} (solid).");
        }
        if (spriteRenderer != null)
        {
            if (lockedDoorSprite != null)
            {
                spriteRenderer.sprite = lockedDoorSprite; 
            }
            else
            {
                Debug.LogWarning($"DoorController on '{gameObject.name}': Locked Door Sprite is not assigned!", this);
            }
        }
        
        UpdateDoorState(); // Cek status pintu saat pertama kali aktif
    }

    void OnEnable()
    {
        if (healthUI != null)
        {
            CollectibleItem.OnItemCollected += OnItemCollectedHandler;
            Debug.Log($"DoorController on '{gameObject.name}': Subscribed to OnItemCollected.");
        }
        else
        {
            Debug.LogError($"DoorController on '{gameObject.name}': OnEnable: HealthUI is NULL, cannot subscribe to item events. Assign in Inspector!", this);
        }
    }

    void OnDisable()
    {
        if (healthUI != null) 
        {
            CollectibleItem.OnItemCollected -= OnItemCollectedHandler;
            Debug.Log($"DoorController on '{gameObject.name}': Unsubscribed from OnItemCollected.");
        }
        else
        {
            Debug.LogWarning($"DoorController on '{gameObject.name}': OnDisable: healthUI was NULL, could not unsubscribe. (This might be normal if the object is being destroyed).");
        }
    }

    private void OnItemCollectedHandler(string itemType)
    {
        // --- KODE PERBAIKAN DI SINI ---
        // PENTING: Periksa apakah objek DoorController ini sudah dihancurkan.
        // Ini mencegah MissingReferenceException jika event dipicu setelah scene berganti
        // dan objek ini sudah tidak ada, tetapi masih berlangganan event statis.
        if (this == null) 
        {
            // Objek DoorController ini sudah dihancurkan.
            // Hapus langganan dari event statis untuk menghindari error di masa mendatang.
            CollectibleItem.OnItemCollected -= OnItemCollectedHandler; 
            Debug.LogWarning($"DoorController: Instance of a DoorController was destroyed. Unsubscribing from OnItemCollected event to prevent errors.");
            return; // Hentikan eksekusi method ini karena objek sudah tidak ada
        }
        // --- AKHIR KODE PERBAIKAN ---

        Debug.Log($"DoorController on '{gameObject.name}': Item '{itemType}' collected. Re-checking door state.");
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        Debug.Log($"DoorController on '{gameObject.name}': Calling UpdateDoorState().");

        if (healthUI == null)
        {
            Debug.LogWarning($"DoorController on '{gameObject.name}': UpdateDoorState: HealthUI is NULL. Cannot check item counts. This warning implies a missing Inspector assignment or a scene loading issue.", this);
            return;
        }

        int currentObeng = healthUI.GetObengCount();
        int currentTang = healthUI.GetTangCount();
        int currentKunci = healthUI.GetKunciCount();

        Debug.Log($"DoorController on '{gameObject.name}': Current items - Obeng:{currentObeng}, Tang:{currentTang}, Kunci:{currentKunci}");
        Debug.Log($"DoorController on '{gameObject.name}': Required items - Obeng:{requiredObeng}, Tang:{requiredTang}, Kunci:{requiredKunci}");

        bool allItemsCollected = 
            currentObeng >= requiredObeng &&
            currentTang >= requiredTang &&
            currentKunci >= requiredKunci;

        Debug.Log($"DoorController on '{gameObject.name}': All items collected status is {allItemsCollected}. Current isUnlocked status: {isUnlocked}.");

        if (allItemsCollected && !isUnlocked)
        {
            UnlockDoor();
        }
        else if (!allItemsCollected && isUnlocked) 
        {
            // Ini akan mengunci kembali pintu jika item tiba-tiba hilang (misal: di-debug)
            LockDoor(); 
        }
        else if (allItemsCollected && isUnlocked) 
        {
            Debug.Log($"DoorController on '{gameObject.name}': Door is already unlocked and all items collected. No change needed.");
        }
        else 
        {
            Debug.Log($"DoorController on '{gameObject.name}': Not all items collected yet. Door remains locked.");
        }
    }

    private void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log($"DoorController on '{gameObject.name}': UnlockDoor() called. isUnlocked set to TRUE. Door UNLOCKED!");

        if (spriteRenderer != null)
        {
            if (unlockedDoorSprite != null)
            {
                spriteRenderer.sprite = unlockedDoorSprite;
                Debug.Log($"DoorController on '{gameObject.name}': Changed sprite to Unlocked Door.");
            }
            else
            {
                Debug.LogWarning($"DoorController on '{gameObject.name}': Unlocked Door Sprite is not assigned! Door will be unlocked but sprite won't change.", this);
            }
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = true; 
            Debug.Log($"DoorController on '{gameObject.name}': Door collider isTrigger set to {doorCollider.isTrigger} (passable).");
        }
        else
        {
            Debug.LogError($"DoorController on '{gameObject.name}': doorCollider is NULL in UnlockDoor(). Cannot set isTrigger.", this);
        }
    }

    private void LockDoor()
    {
        isUnlocked = false;
        Debug.Log($"DoorController on '{gameObject.name}': LockDoor() called. isUnlocked set to FALSE. Door LOCKED!");

        if (spriteRenderer != null && lockedDoorSprite != null)
        {
            spriteRenderer.sprite = lockedDoorSprite;
            Debug.Log($"DoorController on '{gameObject.name}': Changed sprite to Locked Door.");
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = false; 
            Debug.Log($"DoorController on '{gameObject.name}': Door collider isTrigger set to {doorCollider.isTrigger} (solid).");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log($"DoorController on '{gameObject.name}': OnTriggerEnter2D called by Player. Door's current isUnlocked status: {isUnlocked}.");
            if (isUnlocked)
            {
                Debug.Log($"DoorController on '{gameObject.name}': Door is unlocked! Loading scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName); 
            }
            else
            {
                if (healthUI != null)
                {
                    Debug.Log($"DoorController on '{gameObject.name}': Door is locked! Please collect all items. Current: O:{healthUI.GetObengCount()}/R:{requiredObeng}, T:{healthUI.GetTangCount()}/R:{requiredTang}, K:{healthUI.GetKunciCount()}/R:{requiredKunci}");
                }
                else
                {
                    Debug.Log($"DoorController on '{gameObject.name}': Door is locked! (HealthUI not found to show missing items, check Inspector).");
                }
            }
        }
        else
        {
            Debug.Log($"DoorController on '{gameObject.name}': OnTriggerEnter2D called by non-player object '{other.gameObject.name}'. Tag: {other.tag}");
        }
    }
}