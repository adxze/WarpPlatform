using System.Collections;
using UnityEngine;
using TMPro;

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
    private GameManager gameManager;
    
    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (teleporterInfoText != null)
            teleporterInfoText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Handle cooldown
        if (!canTeleport)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
                canTeleport = true;
        }

        // Handle teleport input
        if (Input.GetKeyDown(teleportKey) && canTeleport)
            TeleportToAvailablePortal();
    }
    
    public void OnLevelChanged()
    {
        UpdateTeleporterInfo();
    }
    
    private void UpdateTeleporterInfo()
    {
        if (teleporterInfoText == null) return;
        
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
    
    private int GetAvailableTeleportersCount()
    {
        if (gameManager == null) return 0;
        
        var teleporters = gameManager.GetCurrentLevelTeleporters();
        int count = 0;
        
        foreach (var teleporter in teleporters)
        {
            if (teleporter != null && teleporter.gameObject.activeInHierarchy && teleporter.CanTeleportTo)
                count++;
        }
        
        return count;
    }
    
    private void TeleportToAvailablePortal()
    {
        if (gameManager == null) return;
        
        var teleporters = gameManager.GetCurrentLevelTeleporters();
        
        // Find first available teleporter
        foreach (var teleporter in teleporters)
        {
            if (teleporter != null && teleporter.gameObject.activeInHierarchy && teleporter.CanTeleportTo)
            {
                StartCoroutine(TeleportPlayer(teleporter));
                return;
            }
        }
        
        Debug.Log("No available teleporters");
    }

    private IEnumerator TeleportPlayer(Teleporter targetTeleporter)
    {
        canTeleport = false;
        cooldownTimer = teleportCooldown;
        targetTeleporter.StartCooldown();

        var playerRb = playerController.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = playerRb.velocity;

        // Teleport effect at start position
        if (teleportEffect != null)
            Instantiate(teleportEffect, playerController.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.05f);

        // Teleport
        playerController.transform.position = targetTeleporter.transform.position;

        // Handle momentum
        if (targetTeleporter.ShouldPreserveMomentum())
            playerRb.velocity = playerVelocity;
        else
            playerRb.velocity = Vector2.zero;

        // Teleport effect at end position
        if (teleportEffect != null)
            Instantiate(teleportEffect, playerController.transform.position, Quaternion.identity);
        
        UpdateTeleporterInfo();
    }
}