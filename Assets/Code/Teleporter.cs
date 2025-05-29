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

    public bool CanTeleportTo { get; private set; } = true;
    private float cooldownTimer = 0f;
    private TeleportManager teleportManager;
    private Animator teleanim;

    private void Awake()
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
                // Return to idle when cooldown is over
                ReturnToIdle();
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
            teleanim.SetBool("isActive", true);
        }
    }

    public void ReturnToIdle()
    {
        if (teleanim != null)
        {
            teleanim.SetBool("isActive", false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = portalColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
    }
}