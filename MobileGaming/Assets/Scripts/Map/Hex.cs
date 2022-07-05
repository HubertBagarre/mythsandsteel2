using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Hex : NetworkBehaviour
{
    [Header("Gaming")]
    [SyncVar] public sbyte movementCost = 1;
    public Unit currentUnit;
    public ScriptableTile tile;
    
    [Header("Offset Coordinates (even-r)")]
    [SyncVar] public sbyte col;
    [SyncVar] public sbyte row;
    
    [Header("Cube Coordinates")]
    [SyncVar] public sbyte q;
    [SyncVar] public sbyte r;
    [SyncVar] public sbyte s;
    public Hex[] neighbours = new Hex[6];

    [Header("Tile Application")] [SerializeField]
    private Transform modelParent;
    
    public static float DistanceBetween(Hex a, Hex b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s))/2f;
    }

    public void CoordToCube()
    {
        q = Convert.ToSByte(col - (row + (row & 1)) / 2);
        r = row;
        s = Convert.ToSByte(-q - r);
    }

    public void ApplyTile(ScriptableTile newTile)
    {
        tile = newTile;
        if(modelParent.childCount >= 1) Destroy(modelParent.GetChild(0));
        if(tile.model == null)
        {
            movementCost = sbyte.MaxValue;
            return;
        }

        var model = Instantiate(tile.model, modelParent);
        model.transform.localPosition = Vector3.zero;
        movementCost = tile.movementCost;
    }

    public void OnUnitEnter(Unit unit)
    {
        currentUnit = unit;
        currentUnit.currentHex = this;
        
    }

    public void OnUnitExit(Unit unit)
    {
        currentUnit = null;
    }
    
}
