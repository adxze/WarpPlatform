using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // For UI text, remove if not using TextMeshPro

public class TeleportManager : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private KeyCode teleportKey = KeyCode.F;
    [SerializeField] private float teleportCooldown = 1.0f;
    [SerializeField] private GameObject teleportEffect;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI teleporterInfoText; 
    
    private bool canTeleport = true;
    private float cooldownTimer = 0f;
    private PlayerController playerController;
    
    private Dictionary<string, List<teleporter>> teleportersByLevel = new Dictionary<string, List<teleporter>>();
    private string currentLevelId = ""; 
    
    private GameManager gameManager;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        
        FindAllTeleporters();
        
        if (teleporterInfoText != null)
            teleporterInfoText.gameObject.SetActive(false);
    }
    
    private void FindAllTeleporters()
    {
        teleporter[] allTeleporters = FindObjectsOfType<teleporter>();
        
        teleportersByLevel.Clear();
        
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
        if (gameManager != null)
        {
            string newLevelId = gameManager.GetCurrentLevelId();
            
            if (newLevelId != currentLevelId)
            {
                currentLevelId = newLevelId;
                UpdateTeleporterInfo();
            }
        }
        
        if (!canTeleport)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canTeleport = true;
            }
        }

        if (Input.GetKeyDown(teleportKey) && canTeleport)
        {
            TeleportToAvailablePortal();
        }
    }
    
    private void UpdateTeleporterInfo()
    {
        // Update UI to show available teleporters in current level
        if (teleporterInfoText != null)
        {
            if (teleportersByLevel.ContainsKey(currentLevelId) && teleportersByLevel[currentLevelId].Count > 0)
            {
                int availableCount = GetAvailableTeleportersCount();
                if (availableCount > 0)
                {
                    teleporterInfoText.text = $"Teleporters available: {availableCount}";
                    teleporterInfoText.gameObject.SetActive(true);
                }
                else
                {
                    teleporterInfoText.gameObject.SetActive(false);
                }
            }
            else
            {
                teleporterInfoText.gameObject.SetActive(false);
            }
        }
    }
    
    private int GetAvailableTeleportersCount()
    {
        if (!teleportersByLevel.ContainsKey(currentLevelId))
            return 0;
            
        int count = 0;
        foreach (teleporter tp in teleportersByLevel[currentLevelId])
        {
            if (tp.CanTeleportTo)
                count++;
        }
        return count;
    }
    
    private void TeleportToAvailablePortal()
    {
        if (!teleportersByLevel.ContainsKey(currentLevelId) || teleportersByLevel[currentLevelId].Count == 0)
        {
            Debug.Log("No teleporters available in current level");
            return;
        }
        
        teleporter availableTeleporter = null;
        foreach (teleporter tp in teleportersByLevel[currentLevelId])
        {
            if (tp.CanTeleportTo)
            {
                availableTeleporter = tp;
                break;
            }
        }
        
        if (availableTeleporter != null)
        {
            StartCoroutine(TeleportPlayer(availableTeleporter));
        }
        else
        {
            Debug.Log("No available teleporters in current level (all on cooldown)");
        }
    }

    private IEnumerator TeleportPlayer(teleporter targetTeleporter)
    {
        canTeleport = false;
        cooldownTimer = teleportCooldown;
        targetTeleporter.StartCooldown();

        Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = playerRb.velocity;

        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, playerController.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.05f);

        playerController.transform.position = targetTeleporter.transform.position;

        if (targetTeleporter.ShouldPreserveMomentum())
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
        
        UpdateTeleporterInfo();
    }

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
        
        if (levelId == currentLevelId)
        {
            UpdateTeleporterInfo();
        }
    }
}