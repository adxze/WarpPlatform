using UnityEngine;
using Cinemachine;

public class BoundaryManager : MonoBehaviour
{
    [Header("Boundaries")]
    [SerializeField] private PolygonCollider2D[] boundaries;
    [SerializeField] private int startingBoundaryIndex = 0;
    
    // Reference
    private CinemachineConfiner confiner;
    private int currentBoundaryIndex;
    
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
        
        if (confiner == null)
        {
            Debug.LogError("BoundaryManager: No CinemachineConfiner found in scene!");
            return;
        }
        
        if (boundaries == null || boundaries.Length == 0)
        {
            Debug.LogError("BoundaryManager: No boundaries assigned!");
            return;
        }
        
        // Set starting boundary
        if (startingBoundaryIndex >= 0 && startingBoundaryIndex < boundaries.Length)
        {
            SetBoundary(startingBoundaryIndex);
        }
    }
    
    // Call this from a trigger to switch boundaries
    public void SetBoundary(int index)
    {
        if (index < 0 || index >= boundaries.Length || confiner == null)
            return;
            
        confiner.m_BoundingShape2D = boundaries[index];
        confiner.InvalidatePathCache();
        currentBoundaryIndex = index;
    }
    
    // Get the current boundary index
    public int GetCurrentBoundaryIndex()
    {
        return currentBoundaryIndex;
    }
    
    // Get a specific boundary by index
    public PolygonCollider2D GetBoundary(int index)
    {
        if (index >= 0 && index < boundaries.Length)
            return boundaries[index];
        return null;
    }
    
    // Get the current active boundary
    public PolygonCollider2D GetCurrentBoundary()
    {
        if (currentBoundaryIndex >= 0 && currentBoundaryIndex < boundaries.Length)
            return boundaries[currentBoundaryIndex];
        return null;
    }
}