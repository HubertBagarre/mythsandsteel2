using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/Immram Warband")]
public class FactionImmramWarband : ScriptableFaction
{
    private class MovementBuff : BaseUnitBuff
    {
        private int turnsActive = 0;
        
        protected override void OnBuffAdded(Unit unit)
        {
            if (unit.currentBuffs.Count(buff => buff is MovementBuff) >= 4)
            {
                if(unit.currentBuffs.Contains(this)) unit.currentBuffs.Remove(this);
                return;
            }
            
            buffInfoId = 1;
            
            CallbackManager.OnPlayerTurnStart += IncreaseMovement;
        }

        protected override void OnBuffRemoved(Unit unit)
        {
            CallbackManager.OnPlayerTurnStart -= IncreaseMovement;
        }

        private void IncreaseMovement(PlayerSM playerSm)
        {
            if (playerSm == assignedUnit.player)
            {
                assignedUnit.move++;
                turnsActive++;
            }

            if (turnsActive > 0)
            {
                RemoveBuff();
            }
        }
    }
    
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
                previouslyAttackingUnit.AddBuff(new MovementBuff());
                
                attackingUnit.move++;
                attackingUnit.AddBuff(new MovementBuff());
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
