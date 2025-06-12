// File: MenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Nama scene Main Menu. Pastikan namanya sama persis dengan di Build Settings.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Opsi Tambahan")]
    [Tooltip("Tentukan level progress yang akan dibuka ketika kembali ke menu. Kosongkan jika tidak ingin mengubah.")]
    [SerializeField] private int unlockLevel = 0;

    public void GoToMainMenu()
    {
        if (unlockLevel > 0)
        {
            int current = PlayerPrefs.GetInt("LevelUnlocked", 1);
            if (unlockLevel > current)
            {
                PlayerPrefs.SetInt("LevelUnlocked", unlockLevel);
                Debug.Log($"MenuManager: Progress updated → LevelUnlocked = {unlockLevel}");
            }
        }

        Debug.Log($"MenuManager: Loading scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("MenuManager: Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
