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
    
}