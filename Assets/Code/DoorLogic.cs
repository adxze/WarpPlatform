using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    
    [SerializeField] private Sprite keyActive;
    [SerializeField] private Sprite keyInactive; 
    private SpriteRenderer spriteRenderer;
    
    [SerializeField] private Collider2D[] doorCollider; 
    [SerializeField] private float doorOpenTime = 5f;
    [SerializeField] private float keyTimer = 1f; 
    
    private bool canUseKey = true;
    private Coroutine closeDoorCoroutine; 
    
    
    [SerializeField] Animator[] doorAnimator;
    
    private AudioManager audioManager;
    private void Awake()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canUseKey)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        foreach (Collider2D collider in doorCollider)
        {
            if (collider != null) 
            {
                collider.enabled = false;
                foreach (Animator animator in doorAnimator)
                {
                    audioManager.playSFX(audioManager.laser, 0.19f);
                    animator.SetBool("isActive", true);
                }
                // doorAnimator.SetBool("isActive", true);
                spriteRenderer.sprite = keyInactive;
            }
        }
        
        Debug.Log("Kunci Berhasil digunakan");
        
        if (closeDoorCoroutine != null)
            StopCoroutine(closeDoorCoroutine);
        
        closeDoorCoroutine = StartCoroutine(CloseDoorTime());
        StartCoroutine(KeyTimer());
    }

    private IEnumerator KeyTimer()
    {
        canUseKey = false;
        yield return new WaitForSeconds(keyTimer);
        canUseKey = true;
    }
    
    private IEnumerator CloseDoorTime()
    {
        yield return new WaitForSeconds(doorOpenTime);
        CloseDoor();
        foreach (Animator animator in doorAnimator)
        {
            audioManager.playSFX(audioManager.laserBack, 0.2f);
            animator.SetBool("isActive", false);
        }
        // doorAnimator.SetBool("isActive", false);
        spriteRenderer.sprite = keyActive;
    }

    private void CloseDoor()
    {
        foreach (Collider2D collider in doorCollider)
        {
            if (collider != null) 
            {
                collider.enabled = true;
            }
        }
        
        Debug.Log("Pintu tertutup secara otomatis");
    }
}