using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Hex : NetworkBehaviour
{
    [Header("Gaming")]
    [SyncVar] public sbyte movementCost = 1;
    public Unit currentUnit;
    public ScriptableTile tile;
    
    [Header("Offset Coordinates (odd-r)")]
    [SyncVar] public sbyte col;
    [SyncVar] public sbyte row;
    
    [Header("Cube Coordinates")]
    [SyncVar] public sbyte q;
    [SyncVar] public sbyte r;
    [SyncVar] public sbyte s;
    public Hex[] neighbours = new Hex[6];

    [Header("Tile Application")] [SerializeField]
    private Transform modelParent;
    private Renderer modelRenderer;
    private Material normalMat;
    [SerializeField] private Material selectableMat;
    [SerializeField] private Material unselectableMat;
    [SerializeField] private Material selectedMat;

    [Header("PathFinding")] public int currentCostToMove = -1;
    
    
    public static float DistanceBetween(Hex a, Hex b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s))/2f;
    }

    public static IEnumerable<sbyte> GetCostToNeighbours(Hex a)
    {
        var returnArray = new[]
            {sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue};
        for (var i = 0; i < 6; i++)
        {
            if (a.neighbours[i] != null) returnArray[i] = a.neighbours[i].movementCost;
        }

        return returnArray;
    }

    public IEnumerable<Hex> GetAccessibleNeighbours(sbyte movement)
    {
        return neighbours.Where(hex => hex != null).Where(hex => hex.movementCost <= movement).ToList();
    }


    public static void OddrToCube(Hex hex)
    {
        hex.q = Convert.ToSByte(hex.col - (hex.row - (hex.row & 1)) / 2);
        hex.r = hex.row;
        hex.s = Convert.ToSByte(-hex.q - hex.r);
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
        modelRenderer = model.GetComponent<Renderer>();
        normalMat = modelRenderer.material;
        model.transform.localPosition = Vector3.zero;
        movementCost = tile.movementCost;
    }

    public enum HexColors {Normal, Unselectable, Selectable, Selected}
    public void ChangeHexColor(HexColors color)
    {
        modelRenderer.material = color switch
        {
            HexColors.Normal => normalMat,
            HexColors.Unselectable => unselectableMat,
            HexColors.Selectable => selectableMat,
            HexColors.Selected => selectedMat,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
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
