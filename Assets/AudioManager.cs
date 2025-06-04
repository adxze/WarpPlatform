using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header(" audio source ")] 
    [SerializeField] AudioSource musicSources;
    [SerializeField] AudioSource SFXSources;

    [Header(" audio clip ")] 
    public AudioClip background;
    public AudioClip backgroundMenu;
    public AudioClip backgroundCredit;
    public AudioClip death;
    public AudioClip checkpoint;
    public AudioClip portals;
    public AudioClip laser;
    public AudioClip laserBack;
    public AudioClip Booster;
    public AudioClip klick;

    // Make these accessible to AudioSettingsManager
    public AudioSource MusicSource { get { return musicSources; } }
    public AudioSource SFXSource { get { return SFXSources; } }

    private void Start()
    {
        LoadSavedVolumeSettings(); // Load saved settings first
        PlayBackgroundMusicForCurrentScene();
    }

    private void LoadSavedVolumeSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        if (musicSources != null)
            musicSources.volume = musicVolume;
        if (SFXSources != null)
            SFXSources.volume = sfxVolume;
    }

    public void UpdateMusicVolume(float volume)
    {
        if (musicSources != null)
            musicSources.volume = volume;
    }

    public void UpdateSFXVolume(float volume)
    {
        if (SFXSources != null)
            SFXSources.volume = volume;
    }

    private void PlayBackgroundMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        AudioClip musicToPlay = null;

        switch (currentSceneName.ToLower())
        {
            case "menu":
                musicToPlay = backgroundMenu;
                break;
            case "credits":
                musicToPlay = backgroundCredit;
                break;
            case "level":
                musicToPlay = background;
                break;
            default:
                musicToPlay = background; 
                break;
        }

        if (musicToPlay != null)
        {
            musicSources.clip = musicToPlay;
            musicSources.loop = true;
            musicSources.Play();
        }
    }

    public void playSFX(AudioClip clip, float volume = 1.0f)
    {
        SFXSources.PlayOneShot(clip, volume);
    }
}