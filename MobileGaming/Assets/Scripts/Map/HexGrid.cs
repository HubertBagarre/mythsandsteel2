using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class HexGrid : NetworkBehaviour
{
    [Header("Data Container")]
    public List<Unit> units = new();
    public Dictionary<Vector3Int,Hex> hexes = new ();
    
    [Header("Generation Settings")]
    public Vector2Int mapSize = new (8,10);
    private Transform camAnchor;

    private static Vector3Int[] directionOffsets = new[]
    {
        new Vector3Int(1, 0, -1),
        new Vector3Int(1, -1, 0),
        new Vector3Int(0, -1, 1),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, -1)
    };

    public static HexGrid instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void GenerateMap()
    {
        camAnchor = Camera.main.transform.parent;
        DestroyPreviousGrid();
        NetworkSpawner.SpawnGrid(mapSize);
    }
    
    public void ServerAssignUnitsToTiles()
    {
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];

            //unit.hexGridIndex = Convert.ToSByte(i);
            
            var q = Convert.ToSByte(unit.hexCol - (unit.hexRow - (unit.hexRow & 1)) / 2);
            var r = unit.hexRow;
            var s = Convert.ToSByte(-q - r);
            var vector = new Vector3Int(q, r, s);
            if (!hexes.ContainsKey(vector)) continue;
            
            var targetHex = hexes[vector];
            unit.currentHex = targetHex;
            targetHex.currentUnit = unit;
            var position = targetHex.transform.position + Vector3.up * 2;
            unit.ChangeTransformPosition(position);
        }
    }

    private void DestroyPreviousGrid()
    {
        hexes.Clear();
        units.Clear();
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SetNeighbours()
    {
        var emptyNeighbours = new Hex[] {null, null, null, null, null, null};
        foreach (var hex in hexes.Values)
        {
           hex.neighbours.AddRange(emptyNeighbours);
        }
        UpdateNeighbours();
    }
    
    public void UpdateNeighbours()
    {
        foreach (var hex in hexes.Values)
        {
            for (var i = 0; i < 6; i++)
            {
                var offset = directionOffsets[i];
                var testPos = new Vector3Int(hex.q, hex.r, hex.s) + offset;
                if (hexes.ContainsKey(testPos))
                {
                    hex.neighbours[i] = hexes[testPos];
                }
                else
                {
                    hex.neighbours[i] = null;
                }
            }
        }
        
    }

    public void CenterCamera()
    {
        // TODO - Center Camera on map center, instead of fixed value
        Debug.Log("Centering Cam");
        foreach (var player in GameSM.instance.players)
        {
            player.RpcMoveCamera(new Vector3(10, 0, -6.92f),new Vector3(0, 34, -8.7f));
        }
    }

    public void CenterCamera1()
    {
        sbyte maxRow = 0;
        sbyte minRow = 0;
        sbyte maxCol = 0;
        sbyte minCol = 0;
        foreach (var hex in hexes.Values)
        {
            if (hex.row > maxRow) maxRow = hex.row;
            if (hex.row < minRow) minRow = hex.row;
            if (hex.col > maxCol) maxCol = hex.col;
            if (hex.col < minCol) minCol = hex.col;
        }
        
        var yPos = (maxRow + minRow) / 2f;
        var xPos = (maxCol + minCol) / 2f;

        camAnchor.position = new Vector3(2 * xPos, 0,-yPos * 1.74f);
    }

    public void SetPlayerLists(PlayerSM player)
    {
        player.SetUnitsAndHexesArrays(units.ToArray(),hexes.Values.ToArray());
    }

}
