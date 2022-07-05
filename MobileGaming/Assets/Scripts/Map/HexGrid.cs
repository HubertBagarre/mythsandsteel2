using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public Dictionary<Vector3Int,Hex> hexes = new ();

    public Vector2Int mapSize = new (8,10);
    public float hexSize;
    public GameObject hexPrefab;
    private static Vector3Int[] directionOffsets = new[]
    {
        new Vector3Int(1, 0, -1),
        new Vector3Int(1, -1, 0),
        new Vector3Int(0, -1, 1),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, -1)
    };

    [Button]
    public void InstantiateHexes()
    {
        for (sbyte x = 0; x < mapSize.x; x++)
        {
            for (sbyte y = 0; y < mapSize.y; y++)
            {
                var xPos = y % 2 == 0 ? 2 * x : 2 * x + 1;
                var hexGameObject = Instantiate(hexPrefab, new Vector3(xPos, 0, -1.73f * y), Quaternion.identity);
                hexGameObject.name = $"Hex {x},{y}";
                var hex = hexGameObject.GetComponent<Hex>();
                hex.col = x;
                hex.row = y;
                hex.CoordToCube();
                hex.ApplyTile(ObjectIDList.instance.tiles[1]);
                hexes.Add(new Vector3Int(hex.q,hex.r,hex.s),hex);
            }
        }
        
        UpdateNeighbours();
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
}
