using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    CinemachineConfiner confiner;

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = mapBoundary;
        }
    }
}
