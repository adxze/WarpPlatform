using UnityEngine;
using Cinemachine;

public class DirectionalBoundaryTrigger : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private PolygonCollider2D boundaryToActivate;
    
    [Header("Direction Settings")]
    [SerializeField] private bool checkDirection = true;
    [SerializeField] private Vector2 allowedDirection = Vector2.right; // Direction player must be moving to trigger
    [SerializeField] private float directionThreshold = 0.5f; // How closely must player move in allowed direction
    
    [Header("Optional")]
    [SerializeField] private float cooldownTime = 0.5f;
    
    // References
    private CinemachineConfiner confiner;
    private bool inCooldown = false;
    private float cooldownTimer = 0f;
    private Vector2 lastPlayerPosition;
    
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
        
        if (confiner == null)
        {
            Debug.LogError("DirectionalBoundaryTrigger: No CinemachineConfiner found in scene!");
        }
        
        if (boundaryToActivate == null)
        {
            Debug.LogError("DirectionalBoundaryTrigger: No boundary assigned!");
        }
        
        // Normalize the allowed direction
        allowedDirection.Normalize();
    }
    
    private void Update()
    {
        if (inCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                inCooldown = false;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !inCooldown && confiner != null && boundaryToActivate != null)
        {
            if (confiner.m_BoundingShape2D == boundaryToActivate)
                return;
                
            lastPlayerPosition = other.transform.position;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !inCooldown && confiner != null && boundaryToActivate != null)
        {
            if (confiner.m_BoundingShape2D == boundaryToActivate)
                return;
            
            // Check direction if enabled
            if (checkDirection)
            {
                Vector2 playerPosition = other.transform.position;
                Vector2 movementDirection = (playerPosition - lastPlayerPosition).normalized;
                
                float directionAlignment = Vector2.Dot(movementDirection, allowedDirection);
                
                if (directionAlignment < directionThreshold)
                    return;
            }
            
            confiner.m_BoundingShape2D = boundaryToActivate;
            confiner.InvalidatePathCache();
            
            inCooldown = true;
            cooldownTimer = cooldownTime;
        }
    }
    
    // Visual debugging
    private void OnDrawGizmos()
    {
        if (checkDirection)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Vector3 direction = new Vector3(allowedDirection.x, allowedDirection.y, 0) * 1.0f;
            Gizmos.DrawLine(center, center + direction);
            Gizmos.DrawSphere(center + direction, 0.1f);
        }
    }
}