using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/Immram Warband")]
public class FactionImmramWarband : ScriptableFaction
{
    public override void SetupEvents(PlayerSM player)
    {
        var targetPlayer = player;

        var attackedDict = player.allUnits.ToDictionary(someUnit => someUnit, someUnit => new List<Unit>());

        CallbackManager.OnAnyPlayerTurnStart += ClearAttackedDict;
        
        CallbackManager.OnUnitAttack += OnUnitAttacking;

        CallbackManager.OnUnitKilled += GainFaithOnUnitKilled;

        void ClearAttackedDict()
        {
            foreach (var attackersList in attackedDict.Values.Where(attackers => attackers.Count > 0))
            {
                attackersList.Clear();
            }
        }
        
        void OnUnitAttacking(Unit attackingUnit,Unit attackedUnit)
        {
            if(attackingUnit.player != targetPlayer) return;

            var numberOfUnitThatAttacked = attackedDict[attackedUnit].Count;
            
            if (numberOfUnitThatAttacked > 0)
            {
                attackedUnit.TakeDamage(0,3,attackingUnit);

                var previouslyAttackingUnit = attackedDict[attackedUnit].Last();
                previouslyAttackingUnit.move++;
                attackingUnit.move++;
                
                CallbackManager.OnPlayerTurnStart += IncreaseMovement;
                    
                void IncreaseMovement(PlayerSM playerSm)
                {
                    if (playerSm == targetPlayer)
                    {
                        previouslyAttackingUnit.move++;
                        attackingUnit.move++;
                        CallbackManager.OnPlayerTurnStart -= IncreaseMovement;
                    }
                }
            }
            
            attackedDict[attackedUnit].Add(attackingUnit);
        }

        void GainFaithOnUnitKilled(Unit killedUnit,bool physicalDeath,bool magicalDeath,Unit killer)
        {
            if(killedUnit.player == targetPlayer) return;

            targetPlayer.faith += 4;
            
            if(magicalDeath) targetPlayer.faith += 4;
            
            if(killedUnit.IsOnHexOfType(3)) targetPlayer.faith += 4;
        }
        
        
        
        
    }
}
