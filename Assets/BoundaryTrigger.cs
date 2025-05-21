using UnityEngine;

public class BoundaryTrigger : MonoBehaviour
{
    [Header("Target Boundary")]
    [SerializeField] private int targetBoundaryIndex;
    [SerializeField] private float cooldownTime = 0.5f;
    
    private BoundaryManager boundaryManager;
    private bool inCooldown = false;
    private float cooldownTimer = 0f;
    
    private void Awake()
    {
        boundaryManager = FindObjectOfType<BoundaryManager>();
        
        if (boundaryManager == null)
        {
            Debug.LogError("BoundaryTrigger: No BoundaryManager found in scene!");
        }
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
        if (other.CompareTag("Player") && !inCooldown && boundaryManager != null)
        {
            // Only switch if we're not already using this boundary
            if (boundaryManager.GetCurrentBoundaryIndex() != targetBoundaryIndex)
            {
                boundaryManager.SetBoundary(targetBoundaryIndex);
                
                // Start cooldown
                inCooldown = true;
                cooldownTimer = cooldownTime;
            }
        }
    }
}