using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/The Army To The Edge Of The World")]
public class FactionAttEotW : ScriptableFaction
{
    private class MovementBuff : BaseUnitBuff
    {
        private int turnsActive = 0;
        
        protected override void OnBuffAdded(Unit unit)
        {
            buffInfoId = 0;
            CallbackManager.OnPlayerTurnStart += IncreaseMovement;
            CallbackManager.OnUnitMove += RemoveBuffOnMovement;
        }

        protected override void OnBuffRemoved(Unit unit)
        {
            CallbackManager.OnPlayerTurnStart -= IncreaseMovement;
        }

        private void RemoveBuffOnMovement(Unit unit, Hex hex)
        {
            if(unit == assignedUnit) RemoveBuff();
        }
        
        private void IncreaseMovement(PlayerSM playerSm)
        {
            if (playerSm == assignedUnit.player)
            {
                assignedUnit.move++;
                turnsActive++;
                Debug.Log($"Turns active : {turnsActive}");
            }

            if (turnsActive > 0)
            {
                RemoveBuff();
            }
        }
    }
    
    private class AttackBuff : BaseUnitBuff
    {
        private int turnsActive = 0;
        
        protected override void OnBuffAdded(Unit unit)
        {
            buffInfoId = 1;
            assignedUnit.attackDamage += 1;
            
            CallbackManager.OnPlayerTurnStart += IncreaseAttack;
            CallbackManager.OnUnitAttack += RemoveBuffOnAttack;
        }

        protected override void OnBuffRemoved(Unit unit)
        {
            CallbackManager.OnPlayerTurnStart -= IncreaseAttack;
        }

        private void RemoveBuffOnAttack(Unit attackingUnit, Unit attackedUnit)
        {
            if(attackingUnit == assignedUnit) RemoveBuff();
        }
        
        private void IncreaseAttack(PlayerSM playerSm)
        {
            if (playerSm == assignedUnit.player)
            {
                assignedUnit.attackDamage += 1;
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

        var unitPathDict = player.allUnits.ToDictionary(someUnit => someUnit, someUnit => new List<Hex>());

        CallbackManager.OnAnyPlayerTurnStart += ClearTrackers;

        CallbackManager.OnAnyUnitHexEnter += EffectIfAllMovementSpent;

        CallbackManager.OnUnitRespawned += NoFaithOverloadOnRespawn;

        void ClearTrackers()
        {
            foreach (var hexList in unitPathDict.Values.Where(list => list.Count > 0))
            {
                hexList.Clear();
            }
        }

        void EffectIfAllMovementSpent(Unit unit, Hex hex)
        {
            if (unit.player != targetPlayer) return;

            unitPathDict[unit].Add(hex);

            if (unit.move != 0) return;

            foreach (var adjAllyUnit in unit.AdjacentUnits().Where(adjUnit => adjUnit.playerId == unit.playerId))
            {
                adjAllyUnit.HealUnit(2);

                adjAllyUnit.move++;

                adjAllyUnit.AddBuff(new MovementBuff());
                adjAllyUnit.AddBuff(new AttackBuff());
            }

            if (unitPathDict[unit].Distinct().Count() == unitPathDict[unit].Count) targetPlayer.faith += 3;

            unitPathDict[unit].Clear();
        }

        void NoFaithOverloadOnRespawn(Unit unit)
        {
            if (unit.player != targetPlayer) return;

            targetPlayer.faith -= 3;
        }
    }
}