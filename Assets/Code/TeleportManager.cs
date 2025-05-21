using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // For UI text, remove if not using TextMeshPro

public class TeleportManager : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private KeyCode teleportKey = KeyCode.F;
    [SerializeField] private KeyCode cycleTeleportersKey = KeyCode.Tab; // For cycling between teleporters
    [SerializeField] private float teleportCooldown = 1.0f;
    [SerializeField] private GameObject teleportEffect;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI teleporterInfoText; // Optional UI to show current teleporter
    
    private bool canTeleport = true;
    private float cooldownTimer = 0f;
    private PlayerController playerController;
    private teleporter currentTeleportPoint;
    
    // All teleporters by level
    private Dictionary<string, List<teleporter>> teleportersByLevel = new Dictionary<string, List<teleporter>>();
    private string currentLevelId = ""; // Current level the player is in
    private int currentTeleporterIndex = 0; // Index of selected teleporter in current level
    
    // Reference to game manager to get current level
    private GameManager gameManager;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        
        // Find and organize all teleporters by level
        FindAllTeleporters();
        
        // Hide teleporter info text if it exists
        if (teleporterInfoText != null)
            teleporterInfoText.gameObject.SetActive(false);
    }
    
    private void FindAllTeleporters()
    {
        teleporter[] allTeleporters = FindObjectsOfType<teleporter>();
        
        // Clear and rebuild the dictionary
        teleportersByLevel.Clear();
        
        // Organize teleporters by level
        foreach (teleporter tp in allTeleporters)
        {
            string levelId = tp.GetLevelId();
            
            if (!teleportersByLevel.ContainsKey(levelId))
            {
                teleportersByLevel[levelId] = new List<teleporter>();
            }
            
            teleportersByLevel[levelId].Add(tp);
        }
        
        // Debug info
        foreach (var entry in teleportersByLevel)
        {
            Debug.Log($"Level {entry.Key} has {entry.Value.Count} teleporters");
        }
    }

    private void Update()
    {
        // Update current level if game manager exists
        if (gameManager != null)
        {
            string newLevelId = gameManager.GetCurrentLevelId();
            
            // If level changed, reset teleporter selection
            if (newLevelId != currentLevelId)
            {
                currentLevelId = newLevelId;
                currentTeleporterIndex = 0;
                UpdateCurrentTeleporter();
            }
        }
        
        // Handle cooldown
        if (!canTeleport)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canTeleport = true;
            }
        }
        
        // Cycle between teleporters in current level
        if (Input.GetKeyDown(cycleTeleportersKey) && teleportersByLevel.ContainsKey(currentLevelId))
        {
            List<teleporter> availableTeleporters = teleportersByLevel[currentLevelId];
            
            if (availableTeleporters.Count > 0)
            {
                currentTeleporterIndex = (currentTeleporterIndex + 1) % availableTeleporters.Count;
                UpdateCurrentTeleporter();
            }
        }

        // Teleport input
        if (Input.GetKeyDown(teleportKey) && canTeleport && currentTeleportPoint != null)
        {
            if (currentTeleportPoint.CanTeleportTo)
            {
                StartCoroutine(TeleportPlayer());
            }
        }
    }
    
    private void UpdateCurrentTeleporter()
    {
        // Clear current selection
        currentTeleportPoint = null;
        
        // If no teleporters in this level, return
        if (!teleportersByLevel.ContainsKey(currentLevelId) || teleportersByLevel[currentLevelId].Count == 0)
        {
            if (teleporterInfoText != null)
            {
                teleporterInfoText.gameObject.SetActive(false);
            }
            return;
        }
        
        List<teleporter> availableTeleporters = teleportersByLevel[currentLevelId];
        
        // Ensure index is valid
        if (currentTeleporterIndex >= availableTeleporters.Count)
            currentTeleporterIndex = 0;
        
        // Set current teleporter
        currentTeleportPoint = availableTeleporters[currentTeleporterIndex];
        
        // Update UI
        if (teleporterInfoText != null && currentTeleportPoint != null)
        {
            teleporterInfoText.text = $"Teleporter: {currentTeleportPoint.GetTeleporterName()} ({currentTeleporterIndex + 1}/{availableTeleporters.Count})";
            teleporterInfoText.gameObject.SetActive(true);
        }
    }

    private IEnumerator TeleportPlayer()
    {
        canTeleport = false;
        cooldownTimer = teleportCooldown;
        currentTeleportPoint.StartCooldown();

        Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = playerRb.velocity;

        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, playerController.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.05f);

        playerController.transform.position = currentTeleportPoint.transform.position;

        if (currentTeleportPoint.ShouldPreserveMomentum())
        {
            playerRb.velocity = playerVelocity;
        }
        else
        {
            playerRb.velocity = Vector2.zero;
        }

        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, playerController.transform.position, Quaternion.identity);
        }
    }

    // This can be called when player unlocks a new teleporter
    public void AddTeleporter(teleporter newTeleporter)
    {
        string levelId = newTeleporter.GetLevelId();
        
        if (!teleportersByLevel.ContainsKey(levelId))
        {
            teleportersByLevel[levelId] = new List<teleporter>();
        }
        
        if (!teleportersByLevel[levelId].Contains(newTeleporter))
        {
            teleportersByLevel[levelId].Add(newTeleporter);
        }
        
        // If this is for the current level, update the selection
        if (levelId == currentLevelId)
        {
            UpdateCurrentTeleporter();
        }
    }
}