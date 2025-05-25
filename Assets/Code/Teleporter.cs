using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private Color portalColor = Color.blue;
    
    [Header("Teleport Settings")]
    [SerializeField] private bool preserveMomentum = true;
    [SerializeField] private float teleportCooldown = 0.5f;

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

    public bool ShouldPreserveMomentum() => preserveMomentum;

    private void OnDrawGizmos()
    {
        Gizmos.color = portalColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
    }
}