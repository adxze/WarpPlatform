using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    
    // Start is called before the first frame update
    public KeyManager manager;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void onTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            manager.BlueKey = true;
        }
    }
}
