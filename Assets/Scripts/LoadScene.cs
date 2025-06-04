using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Jangan lupa tambahkan ini untuk menggunakan SceneManager

public class LoadScene : MonoBehaviour
{
    public void paused()
    {
        Time.timeScale = 0;
    }

    public void resume()
    {
        Time.timeScale = 1;
    }

    // Fungsi untuk kembali ke scene menu
    public void LoadMenuScene()
    {
        Time.timeScale = 1; // Pastikan timeScale direset ke normal sebelum pindah scene
        SceneManager.LoadScene("MainMenu"); // Ganti "Menu" dengan nama scene menu Anda
    }
}