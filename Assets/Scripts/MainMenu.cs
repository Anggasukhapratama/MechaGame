using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button creditsButton;
    public Button exitButton;
    public float fadeDuration = 0.5f;

    void Start()
    {
        // Setup listener untuk setiap tombol
        playButton.onClick.AddListener(() => StartCoroutine(FadeAndLoadScene("LevelMenu")));
        creditsButton.onClick.AddListener(() => StartCoroutine(FadeAndLoadScene("Credits")));
        exitButton.onClick.AddListener(() => StartCoroutine(FadeAndQuit()));
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        // Dapatkan semua Image dan Text di button
        Image[] buttonImages = { playButton.GetComponent<Image>(), 
                                creditsButton.GetComponent<Image>(), 
                                exitButton.GetComponent<Image>() };
        
        Text[] buttonTexts = { playButton.GetComponentInChildren<Text>(), 
                             creditsButton.GetComponentInChildren<Text>(), 
                             exitButton.GetComponentInChildren<Text>() };

        // Animasi fade out
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            foreach (Image img in buttonImages)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = alpha;
                    img.color = c;
                }
            }
            
            foreach (Text txt in buttonTexts)
            {
                if (txt != null)
                {
                    Color c = txt.color;
                    c.a = alpha;
                    txt.color = c;
                }
            }
            
            yield return null;
        }

        // Load scene setelah fade selesai
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeAndQuit()
    {
        // Sama seperti fade untuk load scene
        Image[] buttonImages = { playButton.GetComponent<Image>(), 
                                creditsButton.GetComponent<Image>(), 
                                exitButton.GetComponent<Image>() };
        
        Text[] buttonTexts = { playButton.GetComponentInChildren<Text>(), 
                             creditsButton.GetComponentInChildren<Text>(), 
                             exitButton.GetComponentInChildren<Text>() };

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            foreach (Image img in buttonImages)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = alpha;
                    img.color = c;
                }
            }
            
            foreach (Text txt in buttonTexts)
            {
                if (txt != null)
                {
                    Color c = txt.color;
                    c.a = alpha;
                    txt.color = c;
                }
            }
            
            yield return null;
        }

        // Keluar dari game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}