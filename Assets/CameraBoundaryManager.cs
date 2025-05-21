using System.Collections;
using UnityEngine;
using Cinemachine;

public class SimpleLevelBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private PolygonCollider2D boundaryCollider;
    [SerializeField] private SimpleLevelBoundary connectedBoundary;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private bool allowTransition = true;
    
    // References
    private CinemachineConfiner confiner;
    private bool isCurrentlyActive = false;
    
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
        
        if (confiner == null)
        {
            Debug.LogError("SimpleLevelBoundary: No CinemachineConfiner found in scene!");
        }
        
        if (boundaryCollider == null)
        {
            Debug.LogError("SimpleLevelBoundary: No boundary collider assigned!");
        }
    }
    
    private void Start()
    {
        // Check if this is the starting boundary
        if (confiner != null && boundaryCollider != null && 
            confiner.m_BoundingShape2D == boundaryCollider)
        {
            isCurrentlyActive = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only proceed if player enters and this isn't already the active boundary
        if (other.CompareTag("Player") && !isCurrentlyActive && allowTransition && 
            connectedBoundary != null && confiner != null)
        {
            // Start transition to this boundary
            StartCoroutine(TransitionToBoundary());
        }
    }
    
    private IEnumerator TransitionToBoundary()
    {
        // Disable both boundaries from triggering during transition
        allowTransition = false;
        if (connectedBoundary != null)
        {
            connectedBoundary.DisableTransition();
        }
        
        // Remember the previous boundary and its settings
        PolygonCollider2D previousBoundary = confiner.m_BoundingShape2D as PolygonCollider2D;
        
        // Create a temporary transition collider
        GameObject tempObj = new GameObject("TransitionBoundary");
        tempObj.transform.position = Vector3.zero;
        
        // Add a composite collider for the transition
        Rigidbody2D rb = tempObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D composite = tempObj.AddComponent<CompositeCollider2D>();
        
        // Add polygons for both boundaries
        PolygonCollider2D fromCollider = tempObj.AddComponent<PolygonCollider2D>();
        PolygonCollider2D toCollider = tempObj.AddComponent<PolygonCollider2D>();
        
        CopyCollider(previousBoundary, fromCollider);
        CopyCollider(boundaryCollider, toCollider);
        
        // Set up for composite
        fromCollider.usedByComposite = true;
        toCollider.usedByComposite = true;
        
        // Apply the composite collider to the camera
        confiner.m_BoundingShape2D = composite;
        confiner.InvalidatePathCache();
        
        // Wait one frame for the composite to update
        yield return null;
        
        // Adjust damping for smoother transition (if using CinemachineFramingTransposer)
        CinemachineVirtualCamera virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
        CinemachineFramingTransposer transposer = null;
        float originalXDamping = 0;
        float originalYDamping = 0;
        
        if (virtualCam != null)
        {
            transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer != null)
            {
                originalXDamping = transposer.m_XDamping;
                originalYDamping = transposer.m_YDamping;
                
                // Temporarily reduce damping for smoother camera movement
                transposer.m_XDamping = originalXDamping * 0.5f;
                transposer.m_YDamping = originalYDamping * 0.5f;
            }
        }
        
        // Wait for the transition duration
        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Set the final boundary
        confiner.m_BoundingShape2D = boundaryCollider;
        confiner.InvalidatePathCache();
        
        // Restore original damping
        if (transposer != null)
        {
            transposer.m_XDamping = originalXDamping;
            transposer.m_YDamping = originalYDamping;
        }
        
        // Clean up
        Destroy(tempObj);
        
        // Update active states
        isCurrentlyActive = true;
        if (connectedBoundary != null) 
        {
            connectedBoundary.SetInactive();
        }
        
        // Re-enable transitions after a short delay
        yield return new WaitForSeconds(0.5f);
        allowTransition = true;
        if (connectedBoundary != null)
        {
            connectedBoundary.EnableTransition();
        }
    }
    
    private void CopyCollider(PolygonCollider2D source, PolygonCollider2D destination)
    {
        if (source == null || destination == null) return;
        
        destination.pathCount = source.pathCount;
        for (int i = 0; i < source.pathCount; i++)
        {
            Vector2[] sourcePath = source.GetPath(i);
            destination.SetPath(i, sourcePath);
        }
    }
    
    // Helper functions for connected boundaries
    public void SetInactive()
    {
        isCurrentlyActive = false;
    }
    
    public void DisableTransition()
    {
        allowTransition = false;
    }
    
    public void EnableTransition()
    {
        allowTransition = true;
    }
}