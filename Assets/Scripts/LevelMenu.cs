using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelMenu : MonoBehaviour
{
    public Button Level1btn;
    public Button Level2btn;
    public Button Level3btn;
    public Button backbtn;

    [Header("Pengaturan Transisi")]
    public float fadeDuration = 0.5f;
    public CanvasGroup canvasGroup;
    public AudioClip soundKlikTombol;
    public AudioClip soundHoverTombol;

    private AudioSource audioSource;

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        audioSource = GetComponent<AudioSource>();

        // Buka level sesuai progres
        int levelUnlocked = PlayerPrefs.GetInt("LevelUnlocked", 1);
        Level1btn.interactable = true;
        Level2btn.interactable = levelUnlocked >= 2;
        Level3btn.interactable = levelUnlocked >= 3;

        Level1btn.onClick.AddListener(() => StartCoroutine(MuatLevel("TutorialScene")));
        Level2btn.onClick.AddListener(() => StartCoroutine(MuatLevel("Level2")));
        Level3btn.onClick.AddListener(() => StartCoroutine(MuatLevel("Level3")));
        backbtn.onClick.AddListener(() => StartCoroutine(KembaliKeMenuUtama()));

        TambahkanEfekHover(Level1btn);
        TambahkanEfekHover(Level2btn);
        TambahkanEfekHover(Level3btn);
        TambahkanEfekHover(backbtn);
    }

    void TambahkanEfekHover(Button tombol)
    {
        EventTrigger trigger = tombol.gameObject.AddComponent<EventTrigger>();

        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((e) =>
        {
            tombol.transform.localScale = Vector3.one * 1.1f;
            if (soundHoverTombol) audioSource.PlayOneShot(soundHoverTombol, 0.5f);
        });
        trigger.triggers.Add(pointerEnter);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((e) =>
        {
            tombol.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }

    IEnumerator MuatLevel(string namaLevel)
    {
        if (soundKlikTombol) audioSource.PlayOneShot(soundKlikTombol);

        if (canvasGroup != null)
        {
            float waktu = 0f;
            while (waktu < fadeDuration)
            {
                waktu += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, waktu / fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(namaLevel);
    }

    IEnumerator KembaliKeMenuUtama()
    {
        if (soundKlikTombol) audioSource.PlayOneShot(soundKlikTombol);

        if (canvasGroup != null)
        {
            float waktu = 0f;
            while (waktu < fadeDuration)
            {
                waktu += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, waktu / fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(KembaliKeMenuUtama());
        }
    }
}