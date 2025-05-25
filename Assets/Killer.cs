using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Killer : MonoBehaviour
{

    public bool isDead = false; 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        

    }

   void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Animator anim = other.GetComponent<Animator>();
            anim.SetTrigger("Death");
            
            // anim.Play("Die");
            // anim.SetBool("isDead", true);
            PlayerController player = other.GetComponent<PlayerController>();
            player.enabled = false;
            Rigidbody2D PlayerRigidbody = other.GetComponent<Rigidbody2D>();
            PlayerRigidbody.gravityScale = 0;  
            PlayerRigidbody.velocity = Vector2.zero;
            anim.SetFloat("verticalSpeed", 0f);
            
            TeleportManager teleport = FindFirstObjectByType<TeleportManager>().GetComponent<TeleportManager>();
            teleport.enabled = false;
            
            GameManager.instance.RespawnAfter(1);
            


        }
    }
}
