using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Collections;
using TMPro;

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
    
    private CinemachineConfiner confiner;
    private LevelData currentLevel;
    private float checkTimer;
    private float levelTextTimer;
    private TeleportManager teleportManager;
    
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
    
    private void CheckPlayerLevel()
    {
        Vector2 playerPos = player.position;
        
        foreach (var levelData in levels)
        {
            if (levelData.triggerArea == null) continue;
            
            if (levelData.triggerArea.OverlapPoint(playerPos) && levelData != currentLevel)
            {
                ChangeLevel(levelData);
                break;
            }
        }
    }
    
    private void ChangeLevel(LevelData newLevel)
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
    
}
