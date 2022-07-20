using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerMovementSelection : BasePlayerState
    {
        private Unit movingUnit;
        private bool receivedAccessibleHexesTriggered;
        
        private Hex startingHex;

        public PlayerMovementSelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            sm.UIUpdateUnitHud();
            
            sm.unitMovementUnit = sm.selectedUnit;
            sm.SetUnitMovementUnit(sm.selectedUnit);
            movingUnit = sm.unitMovementUnit;
            
            if (movingUnit == null)
            {
                Debug.LogWarning("NO UNIT SELECTED, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }

            if (movingUnit.hasAbility && movingUnit.canUseAbility)
            {
                Debug.Log($"{sm.selectedUnit.abilityScriptable.name} costs {sm.selectedUnit.currentAbilityCost} faith. You have {sm.faith}");
                sm.DisplayAbilityButton(true,(sm.selectedUnit.currentAbilityCost <= sm.faith));
            }
            
            receivedAccessibleHexesTriggered = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAccessibleHexesNew(movingUnit);
            
            sm.CmdGetAccessibleHexesForUnitMovement();
        }
        
        private void ClientSideSetAccessibleHexesNew(Unit unitToGetAccessibleHexes)
        {
            var enemyUnits = sm.allUnits.Where(unit => unit.playerId != unitToGetAccessibleHexes.playerId);
            var bfsResult = GraphSearch.BFSGetRange(unitToGetAccessibleHexes,enemyUnits,unitToGetAccessibleHexes.attacksLeft > 0 && unitToGetAccessibleHexes.canUseAbility);
            var returnHexes = bfsResult.hexesInRange.Where(hex => !hex.HasUnitOfPlayer(0) && !hex.HasUnitOfPlayer(1));
            var attackableUnits = bfsResult.attackableUnits;
        
            foreach (var hex in returnHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Selectable);
            }

            foreach (var unit in attackableUnits)
            {
                unit.currentHex.ChangeHexColor(Hex.HexColors.Attackable);
            }
        }
        
        public override void UpdateLogic()
        {
            if (!sm.accessibleHexesReceived) return;
            if(!receivedAccessibleHexesTriggered) OnAccessibleHexesReceived();
            base.UpdateLogic();
        }
        
        private void OnAccessibleHexesReceived()
        {
            receivedAccessibleHexesTriggered = true;
            sm.OnAccessibleHexesReceived();
        }

        protected override void OnNothingClicked()
        {
            base.OnNothingClicked();
            sm.LateExitMovementSelection();
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            var selectedHex = sm.selectedUnit.currentHex;
            if(selectedHex != null)
            {
                if (sm.attackableHexes.Contains(selectedHex))
                {
                    AttackUnit(selectedHex);
                    return;
                }
                
            }
            sm.ChangeState(sm.idleState);
        }
        
        protected override void OnHexClicked()
        {
            base.OnHexClicked();

            var selectedHex = sm.selectedHex;
            if (sm.accessibleHexes.Contains(selectedHex) && selectedHex != startingHex && selectedHex.currentUnit == null)
            {
                sm.SetUnitMovementHex(selectedHex);
                sm.ChangeState(sm.unitInAnimationState);
            }
            else if (sm.attackableHexes.Contains(selectedHex))
            {
                AttackUnit(selectedHex);
            }
            else
            {                        
                sm.ChangeState(sm.idleState);
            }
        }

        private void AttackUnit(Hex hex)
        {
            sm.attackingUnit = movingUnit;
            sm.attackedUnit = hex.currentUnit;
            sm.CmdSetAttackingUnits(movingUnit,hex.currentUnit);
            sm.ChangeState(sm.unitInAnimationState);
        }

        public override void Exit()
        {
            sm.accessibleHexesReceived = false;
            sm.CmdResetAccessibleHexesTrigger();
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
            
            sm.UIUpdateUnitHud();
        }
    }

}

