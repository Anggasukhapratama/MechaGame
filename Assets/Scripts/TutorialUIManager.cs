using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialUIManager : MonoBehaviour
{
    [Header("Tombol Navigasi")]
    public Button backToLevelButton;

    [Tooltip("Scene level tujuan (misalnya: Level1)")]
    [SerializeField] private string levelSceneName = "Level1";

    void Start()
    {
        if (backToLevelButton != null)
        {
            backToLevelButton.onClick.AddListener(() => LoadLevelScene());
        }
        else
        {
            Debug.LogWarning("TutorialUIManager: Tombol Back belum di-assign.");
        }
    }

    void LoadLevelScene()
    {
        SceneManager.LoadScene(levelSceneName);
    }
}
