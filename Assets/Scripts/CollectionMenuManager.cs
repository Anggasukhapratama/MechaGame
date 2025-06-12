using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectionMenuManager : MonoBehaviour
{
    [Header("Tombol Koleksi")]
    public Button collection1Button;
    public Button collection2Button;
    public Button collection3Button;

    [Header("Tombol Navigasi")]
    public Button backButton;

    [Header("Nama Scene Tujuan")]
    [SerializeField] private string collectScene1 = "ColectCar1";
    [SerializeField] private string collectScene2 = "ColectCar2";
    [SerializeField] private string collectScene3 = "ColectCar3";
    [SerializeField] private string mainMenuScene = "MainMenu";

    void Start()
    {
        int levelUnlocked = PlayerPrefs.GetInt("LevelUnlocked", 1);

        // Atur status tombol koleksi berdasarkan progres
        collection1Button.interactable = levelUnlocked >= 2; // Sudah selesai Level 1
        collection2Button.interactable = levelUnlocked >= 3; // Sudah selesai Level 2
        collection3Button.interactable = levelUnlocked >= 4; // Selesai Level 3 (jika ada)

        // Listener tombol
        if (collection1Button.interactable)
            collection1Button.onClick.AddListener(() => LoadScene(collectScene1));
        if (collection2Button.interactable)
            collection2Button.onClick.AddListener(() => LoadScene(collectScene2));
        if (collection3Button.interactable)
            collection3Button.onClick.AddListener(() => LoadScene(collectScene3));

        backButton.onClick.AddListener(() => LoadScene(mainMenuScene));
    }

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
