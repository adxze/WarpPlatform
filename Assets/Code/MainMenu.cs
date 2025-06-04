using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private AudioManager audioManager;
    public void Awake()
    {   
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void Play()
    {
        SceneManager.LoadScene("Level");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void makeSounds()
    {
        audioManager.playSFX(audioManager.klick, 1f);
    }
}
