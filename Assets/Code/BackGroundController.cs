using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundController : MonoBehaviour
{
    private float startPost, length;
    public GameObject cam;
    public float ParalaxEffect;
    
    void Start()
    {
        startPost = cam.transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * ParalaxEffect; // 0-1 / 0 = move with cam / 1 = stay
        float movement = cam.transform.position.x * (1 - ParalaxEffect);
        transform.position = new Vector3(distance, transform.position.y, transform.position.z);
        if (movement > startPost + length)
        {
             startPost += length;
        } 
        else if (movement < startPost - length)
        {
            startPost -= length;
        }
    }
}
