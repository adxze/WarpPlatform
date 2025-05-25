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
        public string levelName; // e.g., "Level 1", "Level 2", etc.
        public BoxCollider2D triggerArea; // Box collider that detects player entry
        public PolygonCollider2D cinemachineBoundary; // The actual camera boundary to set
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public GameObject ObjectSpawnPoint;
    }

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Level Data")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private float checkInterval = 0.2f;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelText; // Optional UI element to show level name
    [SerializeField] private float levelTextDisplayTime = 3f; // How long to show the level name
    
    
   
    
    // References
    private CinemachineConfiner confiner;
    private LevelData currentLevel;
    private float checkTimer;
    private float levelTextTimer;
    
    private void Awake()
    {
        instance = this;
        confiner = FindObjectOfType<CinemachineConfiner>();
        
        if (confiner == null)
        {
            Debug.LogError("GameManager: No CinemachineConfiner found in scene!");
            return;
        }
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
                Debug.LogError("GameManager: No player found! Tag your player as 'Player'");
        }
        
        // Hide level text initially
        if (levelText != null)
            levelText.gameObject.SetActive(false);
    }

    
    private void Update()
    {
        if (player == null || confiner == null)
            return;
            
        // Check player's level periodically
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            CheckPlayerLevel();
        }
        
        // Handle level text display timer
        if (levelText != null && levelText.gameObject.activeSelf)
        {
            levelTextTimer -= Time.deltaTime;
            if (levelTextTimer <= 0)
            {
                levelText.gameObject.SetActive(false);
            }
        }
    }
    
    private void CheckPlayerLevel()
    {
        Vector2 playerPos = player.position;
        
        // Check all level trigger areas
        foreach (var levelData in levels)
        {
            // Skip if trigger area is missing
            if (levelData.triggerArea == null) continue;
            
            // If player is in this trigger area and it's not the current level
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
        
        // Update camera confiner to use the associated Cinemachine boundary
        if (newLevel.cinemachineBoundary != null && confiner.m_BoundingShape2D != newLevel.cinemachineBoundary)
        {
            confiner.m_BoundingShape2D = newLevel.cinemachineBoundary;
            confiner.InvalidatePathCache();
        }
        
        // Show level name in UI if available
        if (levelText != null)
        {
            levelText.text = newLevel.levelName;
            levelText.gameObject.SetActive(true);
            levelTextTimer = levelTextDisplayTime;
        }
        
        // Handle objects to activate/deactivate
        if (newLevel.objectsToActivate != null)
        {
            foreach (var obj in newLevel.objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        
        if (newLevel.objectsToDeactivate != null)
        {
            foreach (var obj in newLevel.objectsToDeactivate)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
    

    public string GetCurrentLevelId()
    {
        return currentLevel != null ? currentLevel.levelName : "";
        
    }

    public void respawnAfter(int seconds)
    {
        StartCoroutine(respawn(seconds));
    }
    
    IEnumerator respawn(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        foreach (var levelData in levels)
        {
            if (levelData.levelName == GetCurrentLevelId())
            {
                player.transform.position = levelData.ObjectSpawnPoint.transform.position;
                Animator animator = player.GetComponent<Animator>();
                // animator.SetTrigger("Respawn");
                

                animator.Play("Respawn");
                // animator.SetBool("isGrounded", true);
                // animator.SetBool("isDead", false);
                PlayerController playerController = player.GetComponent<PlayerController>();
                Rigidbody2D PlayerRigidbody = player.GetComponent<Rigidbody2D>();
                TeleportManager teleport = FindFirstObjectByType<TeleportManager>().GetComponent<TeleportManager>();
                
                
                PlayerRigidbody.gravityScale = 1;
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                teleport.enabled = true;
                playerController.enabled = true;
                break;
            }
            
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (levels == null || levels.Count == 0)
            return;

        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            if (level == null) continue;

            Color levelColor = GetLevelColor(i);
            
            if (level.triggerArea != null)
            {
                Gizmos.color = new Color(levelColor.r, levelColor.g, levelColor.b, 0.3f);
                Vector3 center = level.triggerArea.transform.TransformPoint(level.triggerArea.offset);
                Vector3 size = new Vector3(
                    level.triggerArea.size.x * level.triggerArea.transform.lossyScale.x,
                    level.triggerArea.size.y * level.triggerArea.transform.lossyScale.y,
                    0f
                );
                Gizmos.DrawCube(center, size);
                
                Gizmos.color = levelColor;
                Gizmos.DrawWireCube(center, size);
                
                UnityEditor.Handles.color = levelColor;
                UnityEditor.Handles.Label(center + Vector3.up * (size.y * 0.5f + 1f), 
                    level.levelName ?? $"Level {i + 1}");
            }
            
            if (level.cinemachineBoundary != null)
            {
                var points = level.cinemachineBoundary.points;
                if (points.Length > 2)
                {
                    Gizmos.color = new Color(levelColor.r * 0.5f, levelColor.g * 0.5f, levelColor.b * 0.5f, 0.8f);
                    Transform boundaryTransform = level.cinemachineBoundary.transform;
                    
                    for (int j = 0; j < points.Length; j++)
                    {
                        Vector3 currentPoint = boundaryTransform.TransformPoint(points[j]);
                        Vector3 nextPoint = boundaryTransform.TransformPoint(points[(j + 1) % points.Length]);
                        Gizmos.DrawLine(currentPoint, nextPoint);
                    }
                }
            }
            
            if (level.ObjectSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Vector3 spawnPos = level.ObjectSpawnPoint.transform.position;
                
                float crossSize = 0.5f;
                Gizmos.DrawLine(spawnPos + Vector3.left * crossSize, spawnPos + Vector3.right * crossSize);
                Gizmos.DrawLine(spawnPos + Vector3.up * crossSize, spawnPos + Vector3.down * crossSize);
                
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawSphere(spawnPos, 0.3f);
                
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.Label(spawnPos + Vector3.up * 0.8f, "SPAWN");
            }
            
            if (level.objectsToActivate != null)
            {
                Gizmos.color = Color.green;
                foreach (var obj in level.objectsToActivate)
                {
                    if (obj != null)
                    {
                        Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.2f);
                    }
                }
            }
            
            if (level.objectsToDeactivate != null)
            {
                Gizmos.color = Color.red;
                foreach (var obj in level.objectsToDeactivate)
                {
                    if (obj != null)
                    {
                        Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.2f);
                    }
                }
            }
        }
        
        if (Application.isPlaying && currentLevel != null && player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, 1f);
            
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.Label(player.position + Vector3.up * 1.5f, 
                $"Current: {currentLevel.levelName}");
        }
    }
    
    private Color GetLevelColor(int index)
    {
        Color[] colors = {
            Color.cyan,
            Color.magenta,
            Color.yellow,
            new Color(1f, 0.5f, 0f), 
            new Color(0.5f, 0f, 1f), 
            new Color(0f, 1f, 0.5f), 
            new Color(1f, 0f, 0.5f),
            new Color(0.5f, 1f, 0f) 
        };
        
        return colors[index % colors.Length];
    }
    
}