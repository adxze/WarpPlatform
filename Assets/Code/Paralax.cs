using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    private Material mat;
    private float distance;

    [Range(0f, 0.5f)]
    public float speed = 0.2f;

    void Start()
    {
        // Get the material from the object's renderer
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Increase the offset distance based on time and speed
        distance += Time.deltaTime * speed;

        // Apply the horizontal offset to create the parallax effect
        mat.SetTextureOffset("_MainTex", Vector2.right * distance);
    }
}
