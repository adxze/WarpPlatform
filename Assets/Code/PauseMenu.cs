using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject globalVolumeMenu;
    
    private bool gameIsPaused = false;
    private float previousTimeScale = 1f;
    
    private void Start()
    {
        // Make sure pause menu starts hidden
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        if (globalVolumeMenu != null)
        {
            globalVolumeMenu.SetActive(false);
        }
        
        // Hide mouse during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        if (gameIsPaused) return;
        
        gameIsPaused = true;
        
        // Store current time scale and freeze the game
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        // Show pause menu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        
        // Enable Global Volume Menu
        if (globalVolumeMenu != null)
        {
            globalVolumeMenu.SetActive(true);
        }
        
        // Clear UI selection to fix button pressed state
        EventSystem.current.SetSelectedGameObject(null);
        
        // Show mouse for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        if (!gameIsPaused) return;
        
        gameIsPaused = false;
        
        Time.timeScale = previousTimeScale;
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        if (globalVolumeMenu != null)
        {
            globalVolumeMenu.SetActive(false);
        }
        
        EventSystem.current.SetSelectedGameObject(null);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("Game Resumed");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // In build
        Application.Quit();
        #endif
    }
    
    public bool IsGamePaused()
    {
        return gameIsPaused;
    }
    
    public void TogglePause()
    {
        if (gameIsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
}