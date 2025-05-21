using UnityEngine;

public class teleporter : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private Color portalColor = Color.blue;
    
    [Header("Teleport Settings")]
    [SerializeField] private bool preserveMomentum = true;
    [SerializeField] private float teleportCooldown = 0.5f;
    
    [Header("Level Settings")]
    [SerializeField] private string levelId; // Identifier for which level this teleporter belongs to
    [SerializeField] private string teleporterName; // Optional name for the teleporter

    public bool CanTeleportTo { get; private set; } = true;
    private float cooldownTimer = 0f;

    private void Update()
    {
        if (!CanTeleportTo && cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                CanTeleportTo = true;
            }
        }
    }

    public void StartCooldown()
    {
        CanTeleportTo = false;
        cooldownTimer = teleportCooldown;
    }

    public bool ShouldPreserveMomentum()
    {
        return preserveMomentum;
    }
    
    public string GetLevelId()
    {
        return levelId;
    }
    
    public string GetTeleporterName()
    {
        return string.IsNullOrEmpty(teleporterName) ? gameObject.name : teleporterName;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = portalColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
        
        // Display level ID in scene view
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f, levelId);
#endif
    }
}