using UnityEngine;

public class DirectionalBooster : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private BoostDirection direction = BoostDirection.Up;
    [SerializeField] private float boostForce = 10f;
    [SerializeField] private bool useConstantForce = false;
    [SerializeField] private float boostDuration = 0.2f; // Only used for constant force
    [SerializeField] private bool overridePlayerVelocity = true;
    
    [Header("Visual & Audio")]
    [SerializeField] private ParticleSystem boostEffect;
    [SerializeField] private AudioClip boostSound;
    [SerializeField] private Color gizmoColor = Color.yellow;
    
    public enum BoostDirection
    {
        Up,
        Down,
        Left,
        Right,
        Custom 
    }
    
    [Header("Custom Direction")]
    [SerializeField] [Range(0, 360)] private float customAngle = 45f;
    
    private Vector2 boostVector;
    private float boostTimer;
    private Rigidbody2D playerRb;
    private AudioSource audioSource;
    
    private void Awake()
    {
        UpdateBoostVector();
        
        if (boostSound != null && GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = boostSound;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    private void UpdateBoostVector()
    {
        switch (direction)
        {
            case BoostDirection.Up:
                boostVector = Vector2.up;
                break;
            case BoostDirection.Down:
                boostVector = Vector2.down;
                break;
            case BoostDirection.Left:
                boostVector = Vector2.left;
                break;
            case BoostDirection.Right:
                boostVector = Vector2.right;
                break;
            case BoostDirection.Custom:
                float radians = customAngle * Mathf.Deg2Rad;
                boostVector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                break;
        }
        
        boostVector = boostVector.normalized * boostForce;
    }
    
    private void Update()
    {
        if (useConstantForce && playerRb != null && boostTimer > 0)
        {
            boostTimer -= Time.deltaTime;
            
            playerRb.AddForce(boostVector, ForceMode2D.Force);
            
            if (boostTimer <= 0)
            {
                playerRb = null;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody2D>();
            
            if (playerRb != null)
            {
                if (useConstantForce)
                {
                    boostTimer = boostDuration;
                }
                else
                {
                    
                    if (overridePlayerVelocity)
                    {
                       
                        playerRb.velocity = Vector2.zero;
                    }
                    
                    playerRb.AddForce(boostVector, ForceMode2D.Impulse);
                    playerRb = null;
                }
                
                if (boostEffect != null)
                {
                    boostEffect.Play();
                }
                
                if (audioSource != null && boostSound != null)
                {
                    audioSource.Play();
                }
            }
        }
    }
    
    private void OnValidate()
    {
        UpdateBoostVector();
    }
    
    // Visual debugging
    // private void OnDrawGizmos()
    // {
    //     UpdateBoostVector();
    //     
    //     Gizmos.color = gizmoColor;
    //     
    //     Vector3 startPos = transform.position;
    //     Vector3 endPos = startPos + new Vector3(boostVector.x, boostVector.y, 0).normalized * 1.5f;
    //     
    //     Gizmos.DrawLine(startPos, endPos);
    //     
    //     Vector3 direction = (endPos - startPos).normalized;
    //     Vector3 right = Quaternion.Euler(0, 0, -30) * direction * 0.5f;
    //     Vector3 left = Quaternion.Euler(0, 0, 30) * direction * 0.5f;
    //     
    //     Gizmos.DrawLine(endPos, endPos - right);
    //     Gizmos.DrawLine(endPos, endPos - left);
    //     
    //     Collider2D collider = GetComponent<Collider2D>();
    //     if (collider != null)
    //     {
    //         Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.4f);
    //         
    //         if (collider is BoxCollider2D)
    //         {
    //             BoxCollider2D boxCollider = collider as BoxCollider2D;
    //             Vector3 size = new Vector3(boxCollider.size.x, boxCollider.size.y, 0.1f);
    //             Vector3 center = transform.TransformPoint(boxCollider.offset);
    //             
    //             Gizmos.DrawCube(center, size);
    //         }
    //         else if (collider is CircleCollider2D)
    //         {
    //             CircleCollider2D circleCollider = collider as CircleCollider2D;
    //             Vector3 center = transform.TransformPoint(circleCollider.offset);
    //             
    //             Gizmos.DrawSphere(center, circleCollider.radius);
    //         }
    //     }
    // }
}