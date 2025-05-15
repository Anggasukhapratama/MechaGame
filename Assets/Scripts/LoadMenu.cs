using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Tambahkan untuk manajemen scene
using UnityEngine.UI; // Tambahkan untuk akses komponen Button dan Image

public class LoadMenu : MonoBehaviour
{
    public Button startButton; // Referensi ke tombol Start
    public float blinkInterval = 0.5f; // Interval kedip dalam detik
    private Image buttonImage; // Komponen Image dari tombol
    private bool isVisible = true; // Status visibilitas tombol
    private float timer = 0f;

    void Start()
    {
        // Dapatkan komponen Image dari tombol
        buttonImage = startButton.GetComponent<Image>();
        
        // Tambahkan listener untuk event onClick
        startButton.onClick.AddListener(LoadMainMenu);
        
        // Mulai efek kedip
        StartCoroutine(BlinkButton());
    }

    IEnumerator BlinkButton()
    {
        while (true)
        {
            // Toggle visibilitas tombol
            isVisible = !isVisible;
            buttonImage.enabled = isVisible;
            
            // Tunggu interval yang ditentukan
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void LoadMainMenu()
    {
        // Hentikan efek kedip
        StopAllCoroutines();
        
        // Pindah ke scene MainMenu
        SceneManager.LoadScene("MainMenu");
    }
}