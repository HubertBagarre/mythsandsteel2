using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GraphSearch
{
    public static BFSResult BFSGetRange(Unit unit)
    {
        var startPoint = unit.currentHex;
        int movementPoints = unit.move;
        int attackRange = unit.range;
        int friendlyPlayer = unit.playerId;
        
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
                if(hexNeighbour.currentUnit != null) if(hexNeighbour.currentUnit.playerId != friendlyPlayer) continue;

                int hexCost = hexNeighbour.movementCost;
                int currentCost = costSoFar[currentHex];
                int newCost = currentCost + hexCost;

                if (newCost <= movementPoints)
                {
                    if (!visitedHex.ContainsKey(hexNeighbour))
                    {
                        visitedHex[hexNeighbour] = currentHex;
                        costSoFar[hexNeighbour] = newCost;
                        hexesToVisitQueue.Enqueue(hexNeighbour);
                    }
                    else if (costSoFar[hexNeighbour] > newCost)
                    {
                        costSoFar[hexNeighbour] = newCost;
                        visitedHex[hexNeighbour] = currentHex;
                    }
                }
            }

        }

        return new BFSResult() {visitedHexesDict = visitedHex,attackableUnitsDict = attackableUnits};
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

    public bool IsHexInRange(Hex hex)
    {
        return visitedHexesDict.ContainsKey(hex);
    }

    public IEnumerable<Hex> GetHexesInRange() => visitedHexesDict.Keys;
}