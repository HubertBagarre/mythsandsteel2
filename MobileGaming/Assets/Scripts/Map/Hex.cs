using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using Mirror;
using UnityEngine;

public class Hex : NetworkBehaviour
{
    private HexGrid hexGrid;
    
    [Header("Gaming")]
    [SyncVar] public sbyte movementCost = 1;
    [SyncVar] public Unit currentUnit;
    [SyncVar(hook = nameof(OnCurrentTileIdValueChanged))] public int currentTileID;
    [SyncVar(hook = nameof(OnCollectibleIdValueChanged))] public int currentCollectibleId;
    public bool hasCollectible => currentCollectibleId != 0;
    
    [Header("Offset Coordinates (odd-r)")]
    [SyncVar] public sbyte col;
    [SyncVar] public sbyte row;
    
    [Header("Cube Coordinates")]
    [SyncVar] public sbyte q;
    [SyncVar] public sbyte r;
    [SyncVar] public sbyte s;
    public Vector3Int GetCubeCoordinates => new Vector3Int(q, r, s);

    public readonly SyncList<Hex> neighbours = new ();

    [Header("Tile Application")]
    public Transform modelParent;
    public bool shouldBeRendered => currentTileID != 0;
    public Transform modelPropsParent;
    private Renderer modelRenderer;
    private Material normalMat;
    [SerializeField] private Material selectableMat;
    [SerializeField] private Material unselectableMat;
    [SerializeField] private Material attackableMat;
    [SerializeField] private Material selectedMat;
    
    
    public void ApplyCoordToCubeCoords()
    {
        var values = OddrCoordToCube(new Vector2Int(col, row));

        q = Convert.ToSByte(values.x);
        r = Convert.ToSByte(values.y);
        s = Convert.ToSByte(values.z);
    }

    #region Maths
    
    public static Vector3Int OddrCoordToCube(Vector2Int coord)
    {
        // col = coord.x
        // row = coord.y

        var valueQ = (coord.x - (coord.y - (coord.y & 1)) / 2);
        var valueR = coord.y;
        var valueS = -valueQ - valueR;

        return new Vector3Int(valueQ, valueR, valueS);
    }
    
    public static Vector2Int OddrCubeToCoord(Vector3Int cube)
    {
        // q = cube.x
        // r = cube.y
        // s = cube.z

        var valueCol = cube.x + (cube.y - (cube.y & 1)) / 2;
        var valueRow = cube.y;

        return new Vector2Int(valueCol, valueRow);
    }

    public static Vector3Int SubstractCubeCoords(Vector3Int hex1, Vector3Int hex2)
    {
        return new Vector3Int(hex1.x - hex2.x, hex1.y - hex2.y, hex1.z - hex2.z);
    }

    public static Vector3Int ReflectHexCoord(Vector3Int hex,Vector3Int center)
    {
        //Subract Center, Reflect, Add Center

        var substractedHex = SubstractCubeCoords(hex, center);
        var reflectedHex = substractedHex * -1;
        return SubstractCubeCoords(reflectedHex, -center);
    }
    
    #endregion

    public static void JoinHexGrid(Hex hex)
    {
        if(HexGrid.instance != null) HexGrid.instance.hexes.Add(new Vector3Int(hex.q,hex.r,hex.s),hex);
    }
    
    public static float DistanceBetween(Hex a, Hex b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s))/2f;
    }

    public IEnumerable<Hex> GetNeighborsInRange(int range)
    {
        var bfsResult = GraphSearch.BFSGetRange(this,range,null);
        return bfsResult.hexesInRange;
    }
    
    public IEnumerable<Unit> AdjacentUnits()
    {
        return neighbours.Count == 0 ? new Unit[]{} : (from hex in neighbours where hex != null where hex.currentUnit != null select hex.currentUnit).ToArray();
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
        var newTile = ObjectIDList.GetTileScriptable(tileID);
        currentTileID = tileID;

        //Change Hex model
        var model =  ModelSpawner.UpdateHexModel(this);
        ModelSpawner.UpdateHexCollectible(this);
        
        //Update tile stats
        movementCost = newTile.movementCost;
        if(newTile.model == null)
        {
            movementCost = sbyte.MaxValue;
            return;
        }
        
        //Update variables
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
        currentUnit = unit;
        unit.hexCol = col;
        unit.hexRow = row;

        if (hasCollectible)
        {
            var collectibleId = currentCollectibleId;
            currentCollectibleId = 0;
            ObjectIDList.GetCollectibleScriptable(collectibleId).OnPickedUp(unit,this);
            ModelSpawner.UpdateHexCollectible(this);
        }
        
        CallbackManager.UnitHexEnter(unit,this);
    }
    
    public void DecreaseUnitMovement(Unit unit)
    {
        unit.move -= movementCost;
    }

    public void OnUnitExit(Unit unit)
    {
        if (currentUnit == unit) currentUnit = null;
        
        CallbackManager.UnitHexExit(unit,this);
        
        ModelSpawner.UpdateHexCollectible(this);
    }

    private void OnCollectibleIdValueChanged(int prevValue, int newValue)
    {
        ModelSpawner.UpdateHexCollectible(this);
    }
    
    private void OnCurrentTileIdValueChanged(int prevValue, int newValue)
    {
        ModelSpawner.UpdateHexModel(this);
    }
    
}
