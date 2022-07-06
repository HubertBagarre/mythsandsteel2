using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("Testing")]
    public GameObject unitPrefab;
    public sbyte unitMovement;
    
    public Dictionary<Vector3Int,Hex> hexes = new ();
    
    [Header("Generation Settings")]
    public Vector2Int mapSize = new (8,10);
    public GameObject hexPrefab;
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

    private void Start()
    {
        camAnchor = Camera.main.transform.parent;
        InstantiateHexes();
    }

    [Button]
    public void InstantiateHexes()
    {
        DestroyPreviousGrid();
        for (sbyte x = 0; x < mapSize.x; x++)
        {
            for (sbyte y = 0; y < mapSize.y; y++)
            {
                var xPos = y % 2 == 0 ? 2 * x : 2 * x + 1;
                var hexGameObject = Instantiate(hexPrefab, new Vector3(xPos, 0, -1.73f * y), Quaternion.identity,transform);
                hexGameObject.name = $"Hex {x},{y}";
                var hex = hexGameObject.GetComponent<Hex>();
                hex.col = x;
                hex.row = y;
                hex.currentCostToMove = -1;
                Hex.OddrToCube(hex);
                hex.ApplyTile(ObjectIDList.instance.tiles[1]);
                hexes.Add(new Vector3Int(hex.q,hex.r,hex.s),hex);
            }
        }
        
        UpdateNeighbours();
        CenterCamera();
        
        InstantiateUnit();
    }

    private void DestroyPreviousGrid()
    {
        hexes.Clear();
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void UpdateNeighbours()
    {
        foreach (var hex in hexes.Values)
        {
            for (var i = 0; i < 6; i++)
            {
                var offset = directionOffsets[i];
                var testPos = new Vector3Int(hex.q,hex.r,hex.s) + offset;
                if (hexes.ContainsKey(testPos))
                {
                    hex.neighbours[i] = hexes[testPos];
                }
            }
        }
    }

    private void CenterCamera()
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

    public void SetAccessibleHexes(Hex startingHex, int movement)
    {
        hexesToReturn.Clear();
        hexesToReturn.Add(startingHex);
        foreach (var hex in hexes.Values)
        {
            hex.currentCostToMove = -1;
        }
        StartCoroutine(AccessibleRecursive(startingHex,movement));
    }
    
    private IEnumerator AccessibleRecursive(Hex startingHex,int movement,int costToMove = 1)
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
                        if (hex.movementCost == 1 || costMoreHex.Contains(hex)) accessibleHex.Add(hex);
                        else if (movement > 1) costMoreHex.Add(hex);
                    }
                }
            }

            yield return null;
            
            foreach (var hex in accessibleHex)
            {
                if (hex.currentCostToMove == -1) hex.currentCostToMove = costToMove;
                hexesToReturn.Add(hex);
                hexesToReturn = hexesToReturn.Distinct().ToList();
                StartCoroutine(AccessibleRecursive(hex, movement - 1,costToMove + 1));
            }
        }
    }
    
    public void MoveUnit(Unit unit, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unit, path));
    }
    
    private IEnumerator MoveUnitRoutine(Unit unit, Hex[] path)
    {
        isMovingUnit = true;
        foreach (var hex in path)
        {
            unit.currentHex.OnUnitExit(unit);
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;
            
            hex.OnUnitEnter(unit);
            
            yield return new WaitForSeconds(0.5f);
        }

        isMovingUnit = false;
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
    

    public void InstantiateUnit()
    {
        var unit = Instantiate(unitPrefab, new Vector3(10, 2, -1.73f * 4), Quaternion.identity).GetComponent<Unit>();
        hexes[new Vector3Int(3,4,-7)].OnUnitEnter(unit);
        unit.baseMove = unitMovement;
        unit.move = unitMovement;
    }
}
