using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSavingManager : MonoBehaviour
{
    private GameData gameData;
    public static DataSavingManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            Debug.LogError("ERROR: one or more DataSavingManager in scene");
        }
        instance = this;
    }

    private void Start()
    {
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();   
    }

    public void LoadGame()
    {
        // - load any save data from data handler
        if (this.gameData == null)
        {
            this.gameData = new GameData();
        }
        
        // - Push the loaded data to all other script that need it.
    }

    public void SaveGame()
    {
        // pass the data so they can update it 
        
        // save the file to data handler 
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
