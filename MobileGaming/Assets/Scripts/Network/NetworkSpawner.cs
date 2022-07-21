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
                if((x==4&&y==8)||(x==5&&y==8)||(x==6&&y==8)||(x==4&&y==7)||(x==4&&y==0)||(x==5&&y==0)||(x==6&&y==0)||(x==5&&y==1)) hex.currentTileID = 2;
                Hex.OddrToCube(hex);
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
        spawnedUnit.hexRow = Convert.ToSByte(position.x);
        spawnedUnit.hexCol = Convert.ToSByte(position.y);
        spawnedUnit.playerId = player;
        spawnedUnit.unitScriptableId = unitId;
        spawnedUnit.LinkUnitScriptable(spawnedUnit.unitScriptableId);
        HexGrid.instance.units.Add(spawnedUnit);
        NetworkServer.Spawn(unitObject);
    }
    
    public static void SpawnUnit(Vector2 position, sbyte player)
    {
        var unitObject = Object.Instantiate(((NewNetworkRoomManager) NetworkManager.singleton).unitPrefab,
            Vector3.zero, Quaternion.identity);
        unitObject.name = $"Unit {position.x},{position.y}";
        var spawnedUnit = unitObject.GetComponent<Unit>();
        spawnedUnit.hexRow = Convert.ToSByte(position.x);
        spawnedUnit.hexCol = Convert.ToSByte(position.y);
        spawnedUnit.playerId = player;
        spawnedUnit.unitScriptableId = 3;
        spawnedUnit.LinkUnitScriptable(spawnedUnit.unitScriptableId);
        HexGrid.instance.units.Add(spawnedUnit);
        NetworkServer.Spawn(unitObject);
    }

    public static void SpawnUnits()
    {
        if (!NetworkServer.active) return;

        for (int x = 0; x < 10; x+=2)
        {
            for (int y = 0; y < 11; y+=10)
            {
                var unitObject = Object.Instantiate(((NewNetworkRoomManager) NetworkManager.singleton).unitPrefab,
                    Vector3.zero, Quaternion.identity);
                unitObject.name = $"Unit {x},{y}";
                var unit = unitObject.GetComponent<Unit>();
                unit.hexRow = Convert.ToSByte(x);
                unit.hexCol = Convert.ToSByte(y);
                unit.playerId = y > 0 ? Convert.ToSByte(0) : Convert.ToSByte(1);
                unit.baseMove = 3;
                unit.move = 3;
                NetworkServer.Spawn(unitObject);
            }
        }
        
    }
    
}
