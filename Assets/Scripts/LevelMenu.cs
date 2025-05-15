using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelMenu : MonoBehaviour
{
    // Tombol-tombol sesuai nama yang kamu mau
    public Button Level1btn;
    public Button Level2btn;
    public Button Level3btn;
    public Button backbtn;

    [Header("Pengaturan Transisi")]
    public float fadeDuration = 0.5f;
    public CanvasGroup canvasGroup;  // Pastikan diassign di Inspector!
    public AudioClip soundKlikTombol;
    public AudioClip soundHoverTombol;

    private AudioSource audioSource;

    void Start()
    {
        // Cek dan assign CanvasGroup otomatis jika lupa
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        audioSource = GetComponent<AudioSource>();

        // Hubungkan tombol dengan aksi
        Level1btn.onClick.AddListener(() => StartCoroutine(MuatLevel("Level1")));
        Level2btn.onClick.AddListener(() => StartCoroutine(MuatLevel("Level2")));
        Level3btn.onClick.AddListener(() => StartCoroutine(MuatLevel("Level3")));
        backbtn.onClick.AddListener(() => StartCoroutine(KembaliKeMenuUtama()));

        // Tambahkan efek hover
        TambahkanEfekHover(Level1btn);
        TambahkanEfekHover(Level2btn);
        TambahkanEfekHover(Level3btn);
        TambahkanEfekHover(backbtn);
    }

    void TambahkanEfekHover(Button tombol)
    {
        EventTrigger trigger = tombol.gameObject.AddComponent<EventTrigger>();

        // Efek saat mouse masuk
        var pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((e) => {
            tombol.transform.localScale = Vector3.one * 1.1f;
            if (soundHoverTombol) audioSource.PlayOneShot(soundHoverTombol, 0.5f);
        });
        trigger.triggers.Add(pointerEnter);

        // Efek saat mouse keluar
        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((e) => {
            tombol.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }

    IEnumerator MuatLevel(string namaLevel)
    {
        // Mainkan suara klik
        if (soundKlikTombol) audioSource.PlayOneShot(soundKlikTombol);

        // Animasi fade out
        if (canvasGroup != null)
        {
            float waktu = 0f;
            while (waktu < fadeDuration)
            {
                waktu += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, waktu/fadeDuration);
                yield return null;
            }
        }

        // Pindah ke scene level
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
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, waktu/fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        // Tombol ESC untuk kembali
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(KembaliKeMenuUtama());
        }
    }
}