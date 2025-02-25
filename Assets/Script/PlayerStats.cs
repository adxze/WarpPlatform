using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]

public class PlayerStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f,50f)] public float GroundAcceleration = 5f;
    [Range(0.25f,50f)] public float GroundDeceleration = 20f;
    [Range(0.25f,50f)] public float AirAcceleration = 5f; 
    [Range(0.25f,50f)] public float AirDeceleration = 5f;

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    [Header("Grounded / Collision Check")]
    public LayerMask GroundLayer; 
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f; 
    [Range(0f, 1f)] public float HeadWidht = 0.75f;

    public bool DebugShowIsGroundedBox = true;


}
