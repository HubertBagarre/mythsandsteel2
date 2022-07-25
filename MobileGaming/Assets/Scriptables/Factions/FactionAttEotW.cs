using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/The Army To The Edge Of The World")]
public class FactionAttEotW : ScriptableFaction
{
    public override void SetupEvents(PlayerSM player)
    {
        var targetPlayer = player;

        var unitPathDict = player.allUnits.ToDictionary(someUnit => someUnit, someUnit => new List<Hex>());

        CallbackManager.OnAnyPlayerTurnStart += ClearPathDict;

        CallbackManager.OnAnyUnitHexEnter += EffectIfAllMovementSpent;

        CallbackManager.OnUnitRespawned += NoFaithOverloadOnRespawn;

        void ClearPathDict()
        {
            foreach (var hexList in unitPathDict.Values.Where(list => list.Count > 0))
            {
                hexList.Clear();
            }
        }

        void EffectIfAllMovementSpent(Unit unit,Hex hex)
        {
            if(unit.player != targetPlayer) return;
            
            unitPathDict[unit].Add(hex);
            
            if (unit.move != 0) return;

            foreach (var adjAllyUnit in unit.AdjacentUnits().Where(adjUnit => adjUnit.playerId == unit.playerId))
            {
                adjAllyUnit.HealUnit(2);
                
                adjAllyUnit.move++;
                
                CallbackManager.OnPlayerTurnStart += IncreaseMovement;
                    
                void IncreaseMovement(PlayerSM playerSm)
                {
                    if (playerSm == targetPlayer)
                    {
                        adjAllyUnit.move++;
                        CallbackManager.OnPlayerTurnStart -= IncreaseMovement;
                    }
                }
            }
            
            if (unitPathDict[unit].Distinct().Count() == unitPathDict[unit].Count) targetPlayer.faith += 3;

            unitPathDict[unit].Clear();
        }

        

        void NoFaithOverloadOnRespawn(Unit unit)
        {
            if(unit.player != targetPlayer) return;

            targetPlayer.faith -= 3;
        }
    }
}
