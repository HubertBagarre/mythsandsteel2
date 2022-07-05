using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    [Header("Hex Logic")]
    public int q;
    public int r;
    public int s;

    [Header("Gaming")]
    public sbyte movementCost = 1;
    public Unit currentUnit;
    
    
    public static float DistanceBetween(Hex a, Hex b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s))/2f;
    }
    
}
