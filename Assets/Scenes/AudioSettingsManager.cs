using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Optional UI Text")]
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Text sfxVolumeText;
    
    private AudioManager audioManager;
    
    // PlayerPrefs keys
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    
    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found! Make sure AudioManager exists in the scene.");
            return;
        }
        
        InitializeSettings();
        SetupUIListeners();
    }
    
    private void InitializeSettings()
    {
        // Load saved settings or use defaults (50% = 0.5f)
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
        
        // Set slider values
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }
        
        // Apply settings to AudioManager
        ApplyMusicVolume(musicVolume);
        ApplySFXVolume(sfxVolume);
        UpdateVolumeText();
    }
    
    private void SetupUIListeners()
    {
        // Add listeners to sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        ApplyMusicVolume(value);
        UpdateVolumeText();
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        ApplySFXVolume(value);
        UpdateVolumeText();
        
        // Play a test SFX sound when adjusting volume (only if not muted)
        if (audioManager != null && audioManager.klick != null && value > 0)
        {
            audioManager.playSFX(audioManager.klick, value);
        }
    }
    
    private void ApplyMusicVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.UpdateMusicVolume(volume);
        }
    }
    
    private void ApplySFXVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.UpdateSFXVolume(volume);
        }
    }
    
    private void UpdateVolumeText()
    {
        if (musicVolumeText != null && musicVolumeSlider != null)
        {
            int volumePercent = Mathf.RoundToInt(musicVolumeSlider.value * 100);
            musicVolumeText.text = volumePercent == 0 ? "MUTED" : $"{volumePercent}%";
        }
        
        if (sfxVolumeText != null && sfxVolumeSlider != null)
        {
            int volumePercent = Mathf.RoundToInt(sfxVolumeSlider.value * 100);
            sfxVolumeText.text = volumePercent == 0 ? "MUTED" : $"{volumePercent}%";
        }
    }
    
    // Public methods that can be called from UI buttons
    public void ResetToDefaults()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.5f;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 0.5f;
        }
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    // Static methods to get current volume settings from any scene
    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
    }
    
    public static float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
    }
    
    // Public methods to manually set volumes (useful for other scripts)
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = volume;
        }
        else
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            ApplyMusicVolume(volume);
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = volume;
        }
        else
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            ApplySFXVolume(volume);
        }
    }
    
    // Method to get current slider values
    public float GetCurrentMusicVolume()
    {
        return musicVolumeSlider != null ? musicVolumeSlider.value : GetMusicVolume();
    }
    
    public float GetCurrentSFXVolume()
    {
        return sfxVolumeSlider != null ? sfxVolumeSlider.value : GetSFXVolume();
    }
    
    // Auto-save when object is destroyed or app loses focus
    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            PlayerPrefs.Save();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            PlayerPrefs.Save();
    }
    
    // Method to remove all listeners (useful for cleanup)
    private void OnDisable()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
    }
}
