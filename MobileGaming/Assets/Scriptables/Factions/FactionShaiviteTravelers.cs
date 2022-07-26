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
        private int times = 0;

        public Postures posture;
        
        protected override void OnBuffAdded(Unit unit)
        {
            posture = Postures.Defensive;

            CallbackManager.OnAnyPlayerTurnStart += ResetTimeCount;
            
            buffInfoId = 2;
        }

        private void ResetTimeCount()
        {
            times = 0;
        }

        public void ChangePosture()
        {
            if(times >= 2) return;
            
            times++;
            
            if (posture == Postures.Defensive)
            {
                posture = Postures.Offensive;
                assignedUnit.attacksPerTurn += 1;
                assignedUnit.physicDef -= 1;
                assignedUnit.magicDef -= 1;
                buffInfoId = 3;
            }
            else
            {
                posture = Postures.Defensive;
                assignedUnit.HealUnit(2);
                assignedUnit.attacksPerTurn -= 1;
                assignedUnit.physicDef += 1;
                assignedUnit.magicDef += 1;
                buffInfoId = 2;
            }
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

        CallbackManager.OnPlayerTurnStart += OnPlayerTurnStart;

        CallbackManager.OnUnitRespawned += RespawnInOffensivePosture;
        
        CallbackManager.OnUnitAttack += PostureCheckAfterAttack;

        CallbackManager.OnAnyUnitHexEnter += PostureCheckAfterMovement;

        CallbackManager.OnUnitAbilityCasted += PostureCheckAfterAbility;
        
        void GainFaithOnAttacked(Unit attackingUnit,Unit attackedUnit)
        {
            if(attackedUnit.player != targetPlayer) return;

            targetPlayer.faith += 1;
            
            if(IsUnitInPosture(attackedUnit,Postures.Defensive)) targetPlayer.faith += 1;
        }
        
        void OnPlayerTurnStart(PlayerSM playerSm)
        {
            SetFaithModifierOnTurnStart(playerSm);

            EffectOnPosture(playerSm);

            ChangeStanceOnTurnStart(playerSm);
        }

        void SetFaithModifierOnTurnStart(PlayerSM playerSm)
        {
            if(targetPlayer != playerSm) return;

            foreach (var unit in targetPlayer.allUnits.Where(unit => unit.player = targetPlayer).Where(unit => !unit.isDead))
            {
                Debug.Log($"{unit} current buff count : {unit.currentBuffs.Count}");
                if(HasPosture(unit)) Debug.Log($"{unit} current posture count : {GetPosture(unit).posture}");
            }
            
            targetPlayer.faithModifier = (targetPlayer.allUnits.Where(unit => !unit.isDead).Where(unit => unit.player = targetPlayer).Any(unit => IsUnitInPosture(unit,Postures.Offensive))) ? 0 : 1;
        }
        
        void EffectOnPosture(PlayerSM playerSm)
        {
            if(targetPlayer != playerSm) return;

            foreach (var unit in targetPlayer.allUnits.Where(unit => !unit.isDead).Where(unit => unit.player = targetPlayer))
            {
                if(IsUnitInPosture(unit,Postures.Defensive))
                {
                    unit.HealUnit(2);
                }
                else
                {
                    unit.attacksPerTurn += 1;
                    unit.physicDef -= 1;
                    unit.magicDef -= 1;
                }
            }
        }

        void ChangeStanceOnTurnStart(PlayerSM playerSm)
        {
            if(targetPlayer != playerSm) return;
            
            foreach (var unit in targetPlayer.allUnits.Where(unit => !unit.isDead).Where(unit => unit.player = targetPlayer))
            {
                if (unit.currentHp < (int) Math.Floor(unit.maxHp / 2f))
                {
                    GetPosture(unit).ChangePosture();
                }
            }
        }

        void RespawnInOffensivePosture(Unit unit)
        {
            if (unit.player != targetPlayer) return;

            GetPosture(unit).posture = Postures.Offensive;
            unit.attacksPerTurn += 1;
            unit.physicDef -= 1;
            unit.magicDef -= 1;
        }

        void PostureCheckAfterAttack(Unit attackingUnit, Unit attackedUnit)
        {
            ChangePostureOnNoMoreActions(attackingUnit);
        }

        void PostureCheckAfterMovement(Unit unit, Hex hex)
        {
            ChangePostureOnNoMoreActions(unit);
        }

        void PostureCheckAfterAbility(Unit unit, Hex[] hexes)
        {
            ChangePostureOnNoMoreActions(unit);
        }

        void ChangePostureOnNoMoreActions(Unit unit)
        {
            if(targetPlayer != unit.player) return;
            
            if(!unit.canMove && (!unit.canAttack || !unit.canUseAbility)) GetPosture(unit).ChangePosture();
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