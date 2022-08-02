using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit Placement")]
public class ScriptableUnitPlacement : ScriptableObject
{
    public UnitPlacement[] placements;
    public Sprite placementPreview,placementIllustration;

    [Serializable]
    public class UnitPlacement
    {
        public byte unitIndex;
        public Vector2[] positions;
    }

    public void SpawnUnits(int playerIndex)
    {
        var centerCubeCoords = Hex.OddrCoordToCube(new Vector2Int(5,4));
        foreach (var placement in placements)
        {
            foreach (var position in placement.positions)
            {
                if (playerIndex == 0)
                {
                    var cubePos = Hex.OddrCoordToCube(new Vector2Int((int)position.x,(int)position.y));
                    var reflectedCube = Hex.ReflectHexCoord(cubePos, centerCubeCoords);
                    var reflectedPos = Hex.OddrCubeToCoord(reflectedCube);
                    NetworkSpawner.SpawnUnit(placement.unitIndex,reflectedPos,Convert.ToSByte(playerIndex));
                }
                else
                {
                    NetworkSpawner.SpawnUnit(placement.unitIndex,position,Convert.ToSByte(playerIndex));
                }
                
                
            }
        }
        
        
    }
}
