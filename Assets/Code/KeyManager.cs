using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static KeyManager instance;
    public bool RedKey = false; 
    public bool BlueKey = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
