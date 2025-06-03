using System.Collections;
using UnityEngine;

public class MovingTeleporter : Teleporter
{
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float pauseTime = 1f;
    
    // Movement variables
    private bool movingToB = true;

    protected override void Awake()
    {
        base.Awake(); // Call the parent Awake method
    }

    private void Start()
    {
        // Start movement if enabled
        if (canMove && pointA != null && pointB != null)
        {
            StartCoroutine(MoveBackAndForth());
        }
    }

    // Movement methods
    private IEnumerator MoveBackAndForth()
    {
        while (canMove)
        {
            Transform target = movingToB ? pointB : pointA;
            yield return StartCoroutine(MoveTo(target.position));
            yield return new WaitForSeconds(pauseTime);
            movingToB = !movingToB;
        }
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float time = distance / moveSpeed;
        float elapsed = 0f;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / time;
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }
        
        transform.position = targetPosition;
    }

    protected override void OnDrawGizmos()
    {
        // Call base gizmo drawing (the teleporter circle)
        base.OnDrawGizmos();
        
        // Draw movement gizmos
        if (canMove && pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
    }
}