using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button newGameButton;
    public Button continueButton;
    public Button collectionButton;
    public Button creditsButton;
    public Button exitButton;
    public float fadeDuration = 0.5f;

    void Start()
    {
        // --- DEBUGGING: CEK DAN RESET PLAYERPREFS SEMENTARA ---
        // Baris ini bisa Anda hapus atau jadikan komentar setelah debugging selesai.
        Debug.Log("Current LevelUnlocked PlayerPref: " + PlayerPrefs.GetInt("LevelUnlocked", 1));
        // Jika Anda ingin memastikan tombol "Continue" nonaktif di setiap awal run untuk testing:
        // PlayerPrefs.DeleteKey("LevelUnlocked"); // Hapus kunci LevelUnlocked
        // Debug.Log("LevelUnlocked PlayerPref after deletion: " + PlayerPrefs.GetInt("LevelUnlocked", 1));
        // --- AKHIR DEBUGGING ---

        newGameButton.onClick.AddListener(() => StartCoroutine(StartNewGame()));
        continueButton.onClick.AddListener(() => StartCoroutine(ContinueToLevelMenu()));
        collectionButton.onClick.AddListener(() => StartCoroutine(FadeAndLoadScene("MenuCollection")));
        creditsButton.onClick.AddListener(() => StartCoroutine(FadeAndLoadScene("Credits")));
        exitButton.onClick.AddListener(() => StartCoroutine(FadeAndQuit()));

        // Enable continue jika LevelUnlocked > 1
        if (PlayerPrefs.GetInt("LevelUnlocked", 1) > 1)
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    IEnumerator StartNewGame()
    {
        PlayerPrefs.SetInt("LevelUnlocked", 1); // Reset progres
        PlayerPrefs.Save(); // Penting: Pastikan data disimpan segera setelah di-reset
        yield return StartCoroutine(FadeAndLoadScene("LevelMenu"));
    }

    IEnumerator ContinueToLevelMenu()
    {
        yield return StartCoroutine(FadeAndLoadScene("LevelMenu"));
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        Button[] buttons = { newGameButton, continueButton, collectionButton, creditsButton, exitButton }; // Tambahkan collectionButton

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (var btn in buttons)
            {
                if (btn != null)
                {
                    var img = btn.GetComponent<Image>();
                    var txt = btn.GetComponentInChildren<Text>();
                    if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
                    if (txt != null) txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, alpha);
                }
            }

            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeAndQuit()
    {
        yield return StartCoroutine(FadeAndLoadScene("Exit")); // Asumsi ada scene "Exit"

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}