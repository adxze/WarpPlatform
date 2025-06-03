using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public BoxCollider2D triggerArea;
        public PolygonCollider2D cinemachineBoundary;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public GameObject spawnPoint;
        public Teleporter[] levelTeleporters; // Direct references - no strings!
    }

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Level Data")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private float checkInterval = 0.2f;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float levelTextDisplayTime = 3f;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showLevelBoundaries = true;
    [SerializeField] private bool showOnlyWhenSelected = false;
    [SerializeField] private float gizmoAlpha = 0.3f;
    
    [Header("Exit Level")]
    [SerializeField] private string sceneName = "Credits";
    
    private CinemachineConfiner confiner;
    private LevelData currentLevel;
    private float checkTimer;
    private float levelTextTimer;
    private TeleportManager teleportManager;
    private int currentLevelIndex = 0;
    
    private void Awake()
    {
        instance = this;
        confiner = FindObjectOfType<CinemachineConfiner>();
        teleportManager = FindObjectOfType<TeleportManager>();
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (levelText != null)
            levelText.gameObject.SetActive(false);
            
        // Initially disable all teleporters
        DisableAllTeleporters();
    }
    
    private void Start()
    {
        
        // CRITICAL: Reset timeScale in case it was paused when leaving scene
        Time.timeScale = 1f;
    
        // Reset cursor state in case it was changed by pause menu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Load saved game or start new after everything is initialized
        LoadSavedGameOrStartNew();
    }

    private void Update()
    {
        if (player == null || confiner == null) return;
            
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            CheckPlayerLevel();
        }
        
        if (levelText != null && levelText.gameObject.activeSelf)
        {
            levelTextTimer -= Time.deltaTime;
            if (levelTextTimer <= 0)
                levelText.gameObject.SetActive(false);
        }
    }
    
    #region Save System Integration
    
    private void LoadSavedGameOrStartNew()
    {
        // Wait for SaveSystem to be ready
        if (SaveSystem.instance == null)
        {
            Invoke(nameof(LoadSavedGameOrStartNew), 0.1f);
            return;
        }
        
        // Try to load saved data
        if (SaveSystem.instance.LoadGame() && SaveSystem.instance.HasSaveData())
        {
            string savedLevelName = SaveSystem.instance.GetSavedLevelName();
            int savedLevelIndex = SaveSystem.instance.GetSavedLevelIndex();
            
            Debug.Log($"Loading saved game: Level {savedLevelName} (Index: {savedLevelIndex})");
            
            // Find and load the saved level
            if (LoadLevelByIndex(savedLevelIndex))
            {
                // Move player to the spawn point of the loaded level
                MovePlayerToCurrentLevelSpawn();
            }
            else
            {
                Debug.LogWarning($"Could not find saved level {savedLevelName}. Starting from first level.");
                StartFromFirstLevel();
            }
        }
        else
        {
            Debug.Log("No save data found. Starting from first level.");
            StartFromFirstLevel();
        }
    }
    
    private void StartFromFirstLevel()
    {
        if (levels.Count > 0)
        {
            currentLevelIndex = 0;
            ChangeLevel(levels[0], false); // Don't auto-save when starting new game
            MovePlayerToCurrentLevelSpawn();
        }
    }
    
    private bool LoadLevelByIndex(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Count)
        {
            currentLevelIndex = levelIndex;
            ChangeLevel(levels[levelIndex], false); // Don't auto-save when loading from save
            return true;
        }
        return false;
    }
    
    private void MovePlayerToCurrentLevelSpawn()
    {
        if (currentLevel?.spawnPoint != null && player != null)
        {
            player.position = currentLevel.spawnPoint.transform.position;
            
            // Reset player controller if needed
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetPlayer();
            }
        }
    }
    
    #endregion
    
    private void CheckPlayerLevel()
    {
        Vector2 playerPos = player.position;
        
        for (int i = 0; i < levels.Count; i++)
        {
            var levelData = levels[i];
            if (levelData.triggerArea == null) continue;
            
            if (levelData.triggerArea.OverlapPoint(playerPos) && levelData != currentLevel)
            {
                currentLevelIndex = i;
                ChangeLevel(levelData, true); // Auto-save when player naturally enters new level
                break;
            }
        }
    }
    
    private void ChangeLevel(LevelData newLevel, bool shouldAutoSave = true)
    {
        currentLevel = newLevel;
        Debug.Log("Entered " + newLevel.levelName);
        
        // Update camera
        if (newLevel.cinemachineBoundary != null)
        {
            confiner.m_BoundingShape2D = newLevel.cinemachineBoundary;
            confiner.InvalidatePathCache();
        }
        
        // Show level name
        if (levelText != null)
        {
            levelText.text = newLevel.levelName;
            levelText.gameObject.SetActive(true);
            levelTextTimer = levelTextDisplayTime;
        }
        
        // Handle objects
        ActivateObjects(newLevel.objectsToActivate);
        DeactivateObjects(newLevel.objectsToDeactivate);
        
        // Enable only this level's teleporters
        UpdateTeleporters();
        
        // Update teleport manager
        if (teleportManager != null)
            teleportManager.OnLevelChanged();
        
        // Auto-save progress when entering new level
        if (shouldAutoSave && SaveSystem.instance != null)
        {
            SaveSystem.instance.SaveCurrentLevel(newLevel.levelName, currentLevelIndex);
        }
    }
    
    private void UpdateTeleporters()
    {
        // Disable all teleporters first
        DisableAllTeleporters();
        
        // Enable only current level's teleporters
        if (currentLevel?.levelTeleporters != null)
        {
            foreach (var teleporter in currentLevel.levelTeleporters)
            {
                if (teleporter != null)
                    teleporter.gameObject.SetActive(true);
            }
        }
    }
    
    private void DisableAllTeleporters()
    {
        foreach (var level in levels)
        {
            if (level.levelTeleporters != null)
            {
                foreach (var teleporter in level.levelTeleporters)
                {
                    if (teleporter != null)
                        teleporter.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void ActivateObjects(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (var obj in objects)
            if (obj != null) obj.SetActive(true);
    }
    
    private void DeactivateObjects(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (var obj in objects)
            if (obj != null) obj.SetActive(false);
    }

    public Teleporter[] GetCurrentLevelTeleporters()
    {
        return currentLevel?.levelTeleporters ?? new Teleporter[0];
    }

    public void RespawnAfter(int seconds)
    {
        StartCoroutine(RespawnCoroutine(seconds));
    }
    
    private IEnumerator RespawnCoroutine(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (currentLevel?.spawnPoint != null)
        {
            playerObj.transform.position = currentLevel.spawnPoint.transform.position;
            
            var animator = playerObj.GetComponent<Animator>();
            var playerController = playerObj.GetComponent<PlayerController>();
            var rigidbody = playerObj.GetComponent<Rigidbody2D>();
            
            animator?.Play("Respawn");
            rigidbody.gravityScale = 1;
            
            yield return new WaitForSeconds(animator?.GetCurrentAnimatorStateInfo(0).length ?? 0f);
            
            if (teleportManager != null) teleportManager.enabled = true;
            if (playerController != null) playerController.enabled = true;
        }
    }
    
    #region Public Methods for Save System and UI
    
    public string GetCurrentLevelName()
    {
        return currentLevel?.levelName ?? "";
    }
    
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
    
    public void ForceLoadLevel(int levelIndex)
    {
        if (LoadLevelByIndex(levelIndex))
        {
            MovePlayerToCurrentLevelSpawn();
        }
    }
    
    // Public method that can be called from UI to reset progress
    public void ResetGameProgress()
    {
        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.ClearSaveDataAndStartFromLevel1();
        }
    }
    
    #endregion



    #region NextScene

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(sceneName);
        }
    }

    #endregion
    
    
    
    
    
    
    
    
    
    
    #region Level Visualization Gizmos

    private void OnDrawGizmos()
    {
        if (!showOnlyWhenSelected)
            DrawLevelGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (showOnlyWhenSelected)
            DrawLevelGizmos();
    }

    private void DrawLevelGizmos()
    {
        if (!showLevelBoundaries || levels == null) return;
        
        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            if (level == null) continue;
            
            // Generate different color for each level
            Color levelColor = GetLevelColor(i);
            levelColor.a = gizmoAlpha;
            
            // Draw trigger area (Box)
            if (level.triggerArea != null)
            {
                Gizmos.color = levelColor;
                
                // Get bounds from BoxCollider2D
                Bounds bounds = level.triggerArea.bounds;
                
                // Draw filled cube (trigger area)
                Gizmos.DrawCube(bounds.center, bounds.size);
                
                // Draw wireframe for better visibility
                Gizmos.color = new Color(levelColor.r, levelColor.g, levelColor.b, 1f);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            
            // Draw cinemachine boundary (Polygon)
            if (level.cinemachineBoundary != null)
            {
                DrawPolygonGizmo(level.cinemachineBoundary, levelColor);
            }
            
            // Draw level name label
            DrawLevelLabel(level, i);
        }
        
        // Highlight current level
        DrawCurrentLevelHighlight();
    }

    private Color GetLevelColor(int index)
    {
        // Generate different colors for each level
        Color[] colors = {
            Color.red,
            Color.green, 
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f), // Purple
            new Color(0f, 1f, 0.5f), // Spring green
            new Color(1f, 0f, 0.5f)  // Rose
        };
        
        return colors[index % colors.Length];
    }

    private void DrawPolygonGizmo(PolygonCollider2D polygonCollider, Color color)
    {
        if (polygonCollider.pathCount == 0) return;
        
        Gizmos.color = color;
        
        // Draw each path in the polygon collider
        for (int pathIndex = 0; pathIndex < polygonCollider.pathCount; pathIndex++)
        {
            Vector2[] path = polygonCollider.GetPath(pathIndex);
            if (path.Length < 3) continue;
            
            // Convert local points to world points
            Vector3[] worldPoints = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                worldPoints[i] = polygonCollider.transform.TransformPoint(path[i]);
            }
            
            // Draw lines between points
            for (int i = 0; i < worldPoints.Length; i++)
            {
                int nextIndex = (i + 1) % worldPoints.Length;
                Gizmos.DrawLine(worldPoints[i], worldPoints[nextIndex]);
            }
        }
    }

    private void DrawLevelLabel(LevelData level, int index)
    {
        if (level.triggerArea == null) return;
        
        Vector3 labelPosition = level.triggerArea.bounds.center;
        labelPosition.y += level.triggerArea.bounds.size.y * 0.6f; // Above the trigger area
        
        // Draw level name and index
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(labelPosition, $"Level {index}: {level.levelName}");
        #endif
    }

    private void DrawCurrentLevelHighlight()
    {
        if (currentLevel?.triggerArea == null) return;
        
        // Highlight current level with pulsing effect
        float pulse = Mathf.Sin(Time.realtimeSinceStartup * 3f) * 0.3f + 0.7f;
        Color highlightColor = Color.white;
        highlightColor.a = pulse * 0.8f;
        
        Gizmos.color = highlightColor;
        Bounds bounds = currentLevel.triggerArea.bounds;
        
        // Draw thick wireframe
        Gizmos.DrawWireCube(bounds.center, bounds.size * 1.1f);
    }

    // Helper method to toggle gizmo visibility from inspector
    [ContextMenu("Toggle Level Boundaries")]
    private void ToggleLevelBoundaries()
    {
        showLevelBoundaries = !showLevelBoundaries;
        Debug.Log($"Level boundaries: {(showLevelBoundaries ? "ON" : "OFF")}");
    }

    #endregion
}