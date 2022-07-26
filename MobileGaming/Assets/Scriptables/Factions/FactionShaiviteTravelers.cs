using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/Shaivite Travelers")]
public class FactionShaiviteTravelers : ScriptableFaction
{
    public enum Postures
    {
        Defensive,
        Offensive
    }
    
    private class Posture : BaseUnitBuff
    {
        private int timesPostureChangedThisTurn = 0;

        public Postures posture;
        
        protected override void OnBuffAdded(Unit unit)
        {
            posture = Postures.Defensive;

            CallbackManager.OnPlayerTurnStart += TurnStartOperations;
            
            CallbackManager.OnUnitRespawned += RespawnInOffensivePosture;
        
            CallbackManager.OnUnitAttack += PostureCheckAfterAttack;

            CallbackManager.OnUnitMove += PostureCheckAfterMovement;

            CallbackManager.OnUnitAbilityCasted += PostureCheckAfterAbility;
            
            buffInfoId = 2;
        }

        private void TurnStartOperations(PlayerSM playerSm)
        {
            timesPostureChangedThisTurn = 0;

            if (playerSm != assignedUnit.player) return;
            
            if (assignedUnit.currentHp < (int) Math.Floor(assignedUnit.maxHp / 2f))
            {
                timesPostureChangedThisTurn++;
                assignedUnit.player.faith += 2;
                posture = posture == Postures.Defensive ? Postures.Offensive : Postures.Defensive;
            }
            
            ApplyPostureEffect();
        }

        private void ApplyPostureEffect()
        {
            if (posture == Postures.Defensive)
            {
                assignedUnit.HealUnit(2);
                buffInfoId = 2;
            }
            else
            {
                assignedUnit.attacksPerTurn += 1;
                assignedUnit.physicDef -= 1;
                assignedUnit.magicDef -= 1;
                buffInfoId = 3;
            }
        }

        private void RespawnInOffensivePosture(Unit unit)
        {
            if (unit != assignedUnit) return;
            
            posture = Postures.Offensive;
            
            ApplyPostureEffect();
        }

        private void PostureCheckAfterAttack(Unit attackingUnit,Unit attackedUnit)
        {
            if(attackingUnit != assignedUnit) return;

            if (!attackingUnit.canMove && (!attackedUnit.canAttack || !attackingUnit.canUseAbility))
            {
                ChangePosture();
            }
        }
        
        private void PostureCheckAfterMovement(Unit unit,Hex hex)
        {
            if(unit != assignedUnit) return;
            
            if (!unit.canMove && (!unit.canAttack || !unit.canUseAbility))
            {
                ChangePosture();
            }
        }
        
        private void PostureCheckAfterAbility(Unit unit,Hex[] hexes)
        {
            if(unit != assignedUnit) return;
            
            if (!unit.canMove && (!unit.canAttack || !unit.canUseAbility))
            {
                ChangePosture();
            }
        }
        
        private void ChangePosture()
        {
            if(timesPostureChangedThisTurn >= 2) return;
            
            timesPostureChangedThisTurn++;
            
            if (posture == Postures.Defensive)
            {
                posture = Postures.Offensive;
            }
            else
            {
                posture = Postures.Defensive;
                assignedUnit.attacksPerTurn -= 1;
                assignedUnit.physicDef += 1;
                assignedUnit.magicDef += 1;
            }
            
            ApplyPostureEffect();
        }
    }
    
    public override void SetupEvents(PlayerSM player)
    {
        var targetPlayer = player;

        foreach (var unit in player.allUnits.Where(unit => unit.player == targetPlayer))
        {
            unit.AddBuff(new Posture());
        }
        

        CallbackManager.OnUnitAttack += GainFaithOnAttacked;

        CallbackManager.OnPlayerTurnStart += SetFaithModifierOnTurnStart;
        
        
        void GainFaithOnAttacked(Unit attackingUnit,Unit attackedUnit)
        {
            if(attackedUnit.player != targetPlayer) return;

            targetPlayer.faith += 1;
            
            if(IsUnitInPosture(attackedUnit,Postures.Defensive)) targetPlayer.faith += 1;
        }
        
        
        void SetFaithModifierOnTurnStart(PlayerSM playerSm)
        {
            if(targetPlayer != playerSm) return;

            Debug.Log("Bonk");
            
            Debug.Log($"Player {targetPlayer.playerId} has {targetPlayer.allUnits.Count(unit => unit.player == targetPlayer)} ally units");
            
            Debug.Log($"Player {targetPlayer.playerId} has {targetPlayer.allUnits.Where(unit => unit.player == targetPlayer).Count(HasPosture)} ally units with Posture Buff");

            foreach (var unit in targetPlayer.allUnits.Where(unit => unit.player == targetPlayer).Where(unit => !unit.isDead))
            {
                Debug.Log($"{unit} current buff count : {unit.currentBuffs.Count}");
                if(HasPosture(unit)) Debug.Log($"{unit} current posture count : {GetPosture(unit).posture}");
            }
            
            var msg = (targetPlayer.allUnits.Where(unit => !unit.isDead).Where(unit => unit.player == targetPlayer).Any(unit => IsUnitInPosture(unit,Postures.Offensive))) ? "No Faith modifier : " : "Faith Modifier at : ";
            targetPlayer.faithModifier = (targetPlayer.allUnits.Where(unit => !unit.isDead).Where(unit => unit.player == targetPlayer).Any(unit => IsUnitInPosture(unit,Postures.Offensive))) ? 0 : -1;
            Debug.Log($"{msg}+{targetPlayer.faithModifier}");
        }
    }
    
    private bool HasPosture(Unit unit)
    {
        return unit.currentBuffs.Any(unitBuff => unitBuff is Posture);
    }
    
    private Posture GetPosture(Unit unit)
    {
        return (Posture) unit.currentBuffs.Where(unitBuff => unitBuff is Posture).ToArray()[0];
    }

    private bool IsUnitInPosture(Unit unit, Postures posture)
    {
        if (!unit.currentBuffs.Any(unitBuff => unitBuff is Posture)) return false;
        
        return (unit.currentBuffs.Where(unitBuff => unitBuff is Posture).ToArray())[0] is Posture buff && buff.posture == posture;
    }

}