using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Hex : NetworkBehaviour
{
    private HexGrid hexGrid;
    
    [Header("Gaming")]
    [SyncVar] public sbyte movementCost = 1;
    [SyncVar] public Unit currentUnit;
    [SyncVar] public int currentTileID;
    public ScriptableTile tile;
    
    [Header("Offset Coordinates (odd-r)")]
    [SyncVar] public sbyte col;
    [SyncVar] public sbyte row;
    
    [Header("Cube Coordinates")]
    [SyncVar] public sbyte q;
    [SyncVar] public sbyte r;
    [SyncVar] public sbyte s;
    public Vector3Int GetCubeCoordinates => new Vector3Int(q, r, s);

    public readonly SyncList<Hex> neighbours = new ();

    [Header("Tile Application")] [SerializeField]
    private Transform modelParent;
    private Renderer modelRenderer;
    private Material normalMat;
    [SerializeField] private Material selectableMat;
    [SerializeField] private Material unselectableMat;
    [SerializeField] private Material attackableMat;
    [SerializeField] private Material selectedMat;
    
    
    public static void OddrToCube(Hex hex)
    {
        hex.q = Convert.ToSByte(hex.col - (hex.row - (hex.row & 1)) / 2);
        hex.r = hex.row;
        hex.s = Convert.ToSByte(-hex.q - hex.r);
    }

    public static void JoinHexGrid(Hex hex)
    {
        if(HexGrid.instance != null) HexGrid.instance.hexes.Add(new Vector3Int(hex.q,hex.r,hex.s),hex);
    }
    
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

    public IEnumerable<Hex> GetNeighborsInRange(int range)
    {
        var bfsResult = GraphSearch.BFSGetRange(this,range,null);
        return bfsResult.hexesInRange;
    }

    public bool HasUnitOfPlayer(sbyte player)
    {
        if (currentUnit == null) return false;
        return currentUnit.playerId == player;
    }

    public void ApplyTileServer(int tileID)
    {
        ApplyTile(tileID);
        RpcApplyChanges(tileID);
    }

    private void ApplyTile(int tileID)
    {
        var newTile = ObjectIDList.instance.tiles[tileID];
        currentTileID = tileID;
        tile = newTile;
        
        //Update tile stats
        movementCost = tile.movementCost;
        
        //Change Hex model
        if(modelParent.childCount >= 1) Destroy(modelParent.GetChild(0).gameObject);
        if(tile.model == null)
        {
            movementCost = sbyte.MaxValue;
            return;
        }

        var model = Instantiate(tile.model, modelParent);
        modelRenderer = model.GetComponent<Renderer>();
        normalMat = modelRenderer.material;
        model.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    private void RpcApplyChanges(int tileID)
    {
        ApplyTile(tileID);
    }

    public enum HexColors {Normal, Unselectable, Selectable, Selected, Attackable}
    public void ChangeHexColor(HexColors color)
    {
        modelRenderer.material = color switch
        {
            HexColors.Normal => normalMat,
            HexColors.Unselectable => unselectableMat,
            HexColors.Selectable => selectableMat,
            HexColors.Selected => selectedMat,
            HexColors.Attackable => attackableMat,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }
    
    public void OnUnitEnter(Unit unit)
    {
        unit.currentHex = this;
        unit.hexCol = col;
        unit.hexRow = row;
    }
    
    public void DecreaseUnitMovement(Unit unit)
    {
        unit.move -= movementCost;
    }

    public void OnUnitExit(Unit unit)
    {
        
    }
    
}
