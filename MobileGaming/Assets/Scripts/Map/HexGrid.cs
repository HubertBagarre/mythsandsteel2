using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using NaughtyAttributes;
using UnityEngine;

public class HexGrid : NetworkBehaviour
{
    [Header("Network")]
    [SyncVar] public bool isDoneLoadingMap;

    [Header("Data Container")]
    public List<Unit> units = new();
    public Dictionary<Vector3Int,Hex> hexes = new ();
    
    [Header("Generation Settings")]
    public Vector2Int mapSize = new (8,10);
    private Transform camAnchor;
    
    [Header("PathFinding")]
    public bool isFindingHex = false;
    public List<Hex> hexesToReturn = new();
    public List<Hex> costMoreHex = new();
    public bool isMovingUnit = false;
    public bool isFindingPath = false;
    
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
        isDoneLoadingMap = false;
        DestroyPreviousGrid();
        NetworkSpawner.SpawnGrid(mapSize);
    }
    
    public void ServerAssignUnitsToTiles()
    {
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];

            unit.hexGridIndex = Convert.ToSByte(i);
            
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

    public void UpdateNeighbours(Hex hex)
    {
        for (var i = 0; i < 6; i++)
        {
            var offset = directionOffsets[i];
            var testPos = new Vector3Int(hex.q, hex.r, hex.s) + offset;
            if (hexes.ContainsKey(testPos))
            {
                hex.neighbours[i] = hexes[testPos];
            }
        }
    }

    public void CenterCamera()
    {
        // TODO - Center Camera on map center, instead of fixed value
        Debug.Log("Centering Cam");
        foreach (var player in GameSM.instance.players)
        {
            player.RpcMoveCamera(new Vector3(10, 0, -3.5f),new Vector3(0, 34, -15.35f));
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
    
    public Hex[] GetNeighbours(Hex hex)
    {
        var returnArray = new Hex[6];
        for (var i = 0; i < 6; i++)
        {
            var offset = directionOffsets[i];
            var testPos = new Vector3Int(hex.q,hex.r,hex.s) + offset;
            if (hexes.ContainsKey(testPos))
            {
                returnArray[i] = hexes[testPos];
            }
        }

        return returnArray;
    }

    public float GetDistanceBetweenHexes(Hex a, Hex b)
    {
        return Hex.DistanceBetween(a, b);
    }
    
    public void SetAccessibleHexes(Hex startingHex, int movement,int playerBlock)
    {
        hexesToReturn.Clear();
        hexesToReturn.Add(startingHex);
        foreach (var hex in hexes.Values)
        {
            hex.currentCostToMove = -1;
        }
        StartCoroutine(AccessibleRecursive(startingHex,movement,playerBlock));
    }
    
    private IEnumerator AccessibleRecursive(Hex startingHex,int movement,int playerBlock,int costToMove = 1)
    {
        isFindingHex = false;
        
        if (movement > 0)
        {
            isFindingHex = true;
            var accessibleHex = new List<Hex>();
            
            foreach (var hex in startingHex.neighbours)
            {
                if (hex != null)
                {
                    if (hex.movementCost != sbyte.MaxValue && !hexesToReturn.Contains(hex) && !accessibleHex.Contains(hex))
                    {
                        var noEnemyUnit = true;
                        if (hex.currentUnit != null) noEnemyUnit = (hex.currentUnit.playerId == playerBlock);
                        
                        if (noEnemyUnit)
                        {
                            if (hex.movementCost == 1 || costMoreHex.Contains(hex)) accessibleHex.Add(hex);
                            else if (movement > 1) costMoreHex.Add(hex);
                        }
                        
                    }
                }
            }

            yield return null;
            
            foreach (var hex in accessibleHex)
            {
                if (hex.currentCostToMove == -1) hex.currentCostToMove = costToMove;
                hexesToReturn.Add(hex);
                hexesToReturn = hexesToReturn.Distinct().ToList();
                StartCoroutine(AccessibleRecursive(hex, movement - 1,playerBlock,costToMove + 1));
            }
        }
    }


    public void OnMoveRequestReceived()
    {
        Debug.Log($"Received request to move Unit");    
    }
    
    public List<Hex> path = new List<Hex>();
    
    public void SetPath(Hex endPoint,Hex[] accessibleHexes)
    {
        path.Clear();
        path.Add(endPoint);
        StartCoroutine(RecursivePath(endPoint, accessibleHexes));
    }

    private IEnumerator RecursivePath(Hex endpoint,Hex[] accessibleHexes)
    {
        isFindingPath = true;
        var cost = sbyte.MaxValue;
        Hex returnHex = null;
        foreach (var hex in endpoint.neighbours)
        {
            if (accessibleHexes.Contains(hex))
            {
                if (hex.currentCostToMove == -1)
                {
                    isFindingPath = false;
                    yield break;
                }
                
                if (hex.currentCostToMove < cost)
                {
                    cost = Convert.ToSByte(hex.currentCostToMove);
                    returnHex = hex;
                }
            }
        }

        yield return null;
        
        path.Add(returnHex);
        StartCoroutine(RecursivePath(returnHex, accessibleHexes));

    }
    
    
}
