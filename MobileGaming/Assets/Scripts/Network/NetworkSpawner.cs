using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

public class NetworkSpawner
{
    public static void SpawnGrid()
    {
        if (!NetworkServer.active) return;
        
        NetworkServer.Spawn(Object.Instantiate(((NewNetworkRoomManager)NetworkManager.singleton).hexGridPrefab, Vector3.zero, Quaternion.identity));
    }

    public static void SpawnGameStateMachine()
    {
        if (!NetworkServer.active) return;
        
        NetworkServer.Spawn(Object.Instantiate(((NewNetworkRoomManager)NetworkManager.singleton).gameStateMachinePrefab, Vector3.zero, Quaternion.identity));
    }
    
    public static void SpawnGrid(Vector2Int mapSize)
    {
        if (!NetworkServer.active) return;
        
        for (sbyte x = 0; x < mapSize.x; x++)
        {
            for (sbyte y = 0; y < mapSize.y; y++)
            {
                var xPos = y % 2 == 0 ? 2 * x : 2 * x + 1;
                if (y % 2 == 1 && x == 10) continue;
                var hexGameObject = Object.Instantiate(((NewNetworkRoomManager)NetworkManager.singleton).hexPrefab, new Vector3(xPos, 0, -1.73f * y), Quaternion.identity);
                hexGameObject.name = $"Hex {x},{y}";
                var hex = hexGameObject.GetComponent<Hex>();
                hex.col = x;
                hex.row = y;
                hex.currentTileID = 1;
                
                if((x==6&&y==8)||(x==4&&y==7)||(x==4&&y==0)||(x==5&&y==1) || (x==5&&y==4)) hex.currentTileID = 2;
                if((x==4&&y==3)||(x==4&&y==4)||(x==4&&y==5)||(x==5&&y==3)||(x==6&&y==4)||(x==5&&y==5)) hex.currentTileID = 3;
                if((x==2&&y==3)||(x ==3&&y==4)||(x==2&&y==5)||(x==7&&y==3)||(x==7&&y==5)||(x==7&&y==4)) hex.currentCollectibleId = 1;
                if((x==3&&y==0)||(x ==7&&y==0)||(x==3&&y==8)||(x==7&&y==8)||(x==5&&y==2)||(x==5&&y==6)) hex.currentCollectibleId = 2;
                
                hex.ApplyCoordToCubeCoords();
                Hex.JoinHexGrid(hex);
                NetworkServer.Spawn(hexGameObject);

            }
        }
    }

    public static void SpawnUnit(byte unitId,Vector2 position, sbyte player)
    {
        var unitObject = Object.Instantiate(((NewNetworkRoomManager) NetworkManager.singleton).unitPrefab,
            Vector3.zero, Quaternion.identity);
        unitObject.name = $"Unit {position.x},{position.y}";
        var spawnedUnit = unitObject.GetComponent<Unit>();
        spawnedUnit.hexCol = Convert.ToSByte(position.x);
        spawnedUnit.hexRow = Convert.ToSByte(position.y);
        spawnedUnit.playerId = player;
        spawnedUnit.unitScriptableId = unitId;
        spawnedUnit.LinkUnitScriptable(spawnedUnit.unitScriptableId);
        HexGrid.instance.units.Add(spawnedUnit);
        NetworkServer.Spawn(unitObject);
    }
}
