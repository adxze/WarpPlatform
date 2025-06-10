using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string currentLevelName = "";
    public int levelIndex = 0;
    public bool hasPlayedBefore = false;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;
    
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "gamesave.json";
    
    private SaveData currentSaveData;
    private string savePath;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSaveSystem()
    {
        // Create save path
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        
        // Initialize save data
        currentSaveData = new SaveData();
        
        Debug.Log("Save system initialized. Save path: " + savePath);
    }
    
    #region Save/Load Methods
    
    private void SaveGame()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(savePath, jsonData);
            Debug.Log($"Game auto-saved: {currentSaveData.currentLevelName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }
    
    public bool LoadGame()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string jsonData = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);
                Debug.Log("Game loaded successfully!");
                return true;
            }
            else
            {
                Debug.Log("No save file found. Starting new game.");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load game: " + e.Message);
            return false;
        }
    }
    
    #endregion
    
    #region Level Save/Load Methods
    
    public void SaveCurrentLevel(string levelName, int levelIndex)
    {
        currentSaveData.currentLevelName = levelName;
        currentSaveData.levelIndex = levelIndex;
        currentSaveData.hasPlayedBefore = true;
        
        // Automatically save when level changes
        SaveGame();
    }
    
    public SaveData GetSaveData()
    {
        return currentSaveData;
    }
    
    public bool HasSaveData()
    {
        return File.Exists(savePath) && currentSaveData.hasPlayedBefore;
    }
    
    public string GetSavedLevelName()
    {
        return currentSaveData.currentLevelName;
    }
    
    public int GetSavedLevelIndex()
    {
        return currentSaveData.levelIndex;
    }
    
    #endregion
    
    #region Public Methods for UI
    
   
    public void ClearSaveDataAndStartFromLevel1()
    {
       try
        {
            currentSaveData = new SaveData();
            
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted successfully!");
            }
            
            Debug.Log("Save data cleared! Game will start from Level 1.");
            
            // Reload the current scene to start fresh
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to clear save data: " + e.Message);
        }
    }
    
    #endregion
}