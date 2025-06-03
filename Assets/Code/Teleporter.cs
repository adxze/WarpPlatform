using System;
using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private Color portalColor = Color.blue;
    
    [Header("Teleport Settings")]
    [SerializeField] private bool preserveMomentum = true;
    [SerializeField] private float teleportCooldown = 2.0f;
    [SerializeField] private float animationDuration = 1f; // NEW: Separate animation duration

    public bool CanTeleportTo { get; private set; } = true;
    private float cooldownTimer = 0f;
    private TeleportManager teleportManager;
    private Animator teleanim;

    protected virtual void Awake()
    {
        teleanim = GetComponent<Animator>();
        teleportManager = FindObjectOfType<TeleportManager>();
    }

    private void Update()
    {
        if (!CanTeleportTo && cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                CanTeleportTo = true;
                // No longer calling ReturnToIdle here since animation handles itself
            }
        }
    }

    public void StartCooldown()
    {
        CanTeleportTo = false;
        cooldownTimer = teleportCooldown;
    }

    public bool ShouldPreserveMomentum() => preserveMomentum;

    public void PlayTeleportAnimation()
    {
        if (teleanim != null)
        {
            // Start the animation and automatically stop it after animationDuration
            StartCoroutine(PlayAnimationForDuration());
            
            
            // Play particle effect when portal is used
            if (visualEffect != null)
            {
                GameObject effect = Instantiate(visualEffect, transform.position, Quaternion.identity);
        
                Destroy(effect, 3f);
            }
        }
    }

    private IEnumerator PlayAnimationForDuration()
    {
        teleanim.SetBool("isActive", true);
        yield return new WaitForSeconds(animationDuration);
        teleanim.SetBool("isActive", false);
    }

    public void ReturnToIdle()
    {
        if (teleanim != null)
        {
            teleanim.SetBool("isActive", false);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = portalColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
    }
}