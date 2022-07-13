using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GraphSearch
{
    public static BFSResult BFSGetRange(Hex startPoint, int movementPoints,IEnumerable<Unit> enemyUnits, bool withAttack = false)
    {
        var ignoreUnits = enemyUnits == null;
        var unit = (enemyUnits != null) ? startPoint.currentUnit : null;
        var attackRange = (enemyUnits != null) ? unit.range : 0;
        var friendlyPlayer = (enemyUnits != null) ? unit.playerId : 0;

        var visitedHex = new Dictionary<Hex, Hex?>();
        var costSoFar = new Dictionary<Hex, int>();
        var attackableUnits = new Dictionary<Unit, Hex>();
        var hexesToVisitQueue = new Queue<Hex>();

        hexesToVisitQueue.Enqueue(startPoint);
        costSoFar.Add(startPoint,0);
        visitedHex.Add(startPoint,null);
        
        while (hexesToVisitQueue.Count > 0)
        {
            var currentHex = hexesToVisitQueue.Dequeue();
            foreach (var hexNeighbour in currentHex.neighbours)
            {
                if(hexNeighbour == null) continue;
                if(!ignoreUnits) if(hexNeighbour.currentUnit != null) if(hexNeighbour.currentUnit.playerId != friendlyPlayer) continue;

                int hexCost = ignoreUnits ? 1 : hexNeighbour.movementCost;
                int currentCost = costSoFar[currentHex];
                int newCost = currentCost + hexCost;

                if (newCost <= movementPoints)
                {
                    if (!visitedHex.ContainsKey(hexNeighbour))
                    {
                        visitedHex[hexNeighbour] = currentHex;
                        costSoFar[hexNeighbour] = newCost;
                        hexesToVisitQueue.Enqueue(hexNeighbour);
                        
                        if (!ignoreUnits && withAttack)
                        {
                            foreach (var enemyUnit in enemyUnits)
                            {
                                if (Hex.DistanceBetween(enemyUnit.currentHex, hexNeighbour) <= attackRange)
                                {
                                    if(!attackableUnits.ContainsKey(enemyUnit)) attackableUnits.Add(enemyUnit,hexNeighbour);
                                }
                            }
                        }
                    }
                    else if (costSoFar[hexNeighbour] > newCost)
                    {
                        costSoFar[hexNeighbour] = newCost;
                        visitedHex[hexNeighbour] = currentHex;
                        if (!ignoreUnits && withAttack)
                        {
                            foreach (var enemyUnit in enemyUnits)
                            {
                                var newDistanceToHex = Hex.DistanceBetween(enemyUnit.currentHex, hexNeighbour);
                                var currentDistanceToHex = Hex.DistanceBetween(enemyUnit.currentHex, attackableUnits[enemyUnit]);
                                if (newDistanceToHex <= attackRange && newDistanceToHex > currentDistanceToHex)
                                {
                                    if(attackableUnits.ContainsKey(enemyUnit)) attackableUnits[enemyUnit] = hexNeighbour;
                                    else attackableUnits.Add(enemyUnit,hexNeighbour);
                                }
                            }
                        }
                    }
                }
            }

        }
        
        return new BFSResult() {visitedHexesDict = visitedHex,attackableUnitsDict = attackableUnits};
    }
    
    public static BFSResult BFSGetRange(Unit unit,IEnumerable<Unit> enemyUnits,bool withAttack = true)
    {
        return BFSGetRange(unit.currentHex, unit.move, enemyUnits,withAttack);
    }
    
    
    public static List<Hex> GeneratePathBFS(Hex current, Dictionary<Hex, Hex?> visitedHexesDict)
    {
        List<Hex> path = new () {current};
        
        while (visitedHexesDict[current] != null)
        {
            path.Add(visitedHexesDict[current]);
            current = visitedHexesDict[current];
        }
        path.Reverse();
        return path.Skip(1).ToList();
    }
}

public struct BFSResult
{
    public Dictionary<Hex, Hex?> visitedHexesDict;
    public Dictionary<Unit, Hex> attackableUnitsDict;

    public List<Hex> GetPathTo(Hex destination)
    {
        if (!visitedHexesDict.ContainsKey(destination))
        {
            return new List<Hex>();
        }

        return GraphSearch.GeneratePathBFS(destination, visitedHexesDict);
    }
    
    public List<Hex> GetPathTo(Unit targetUnit)
    {
        if (!attackableUnitsDict.ContainsKey(targetUnit))
        {
            return new List<Hex>();
        }

        return GetPathTo(attackableUnitsDict[targetUnit]);
    }

    public bool IsHexInRange(Hex hex)
    {
        return visitedHexesDict.ContainsKey(hex);
    }
    
    public bool IsUnitAttackable(Unit unit)
    {
        return attackableUnitsDict.ContainsKey(unit);
    }

    public IEnumerable<Unit> GetAttackableUnits => attackableUnitsDict.Keys;

    public IEnumerable<Hex> GetHexesInRange() => visitedHexesDict.Keys;
}