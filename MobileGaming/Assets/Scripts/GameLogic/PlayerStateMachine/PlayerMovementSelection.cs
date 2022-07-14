using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
            
            sm.unitMovementUnit = sm.selectedUnit;
            sm.SetUnitMovementUnit(sm.selectedUnit);
            movingUnit = sm.unitMovementUnit;
            
            if (movingUnit == null)
            {
                Debug.LogWarning("NO UNIT SELECTED, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }
            
            receivedAccessibleHexesTriggered = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAccessibleHexesNew(movingUnit);
            
            sm.GetAccessibleHexesForUnitMovement();
        }
        
        private void ClientSideSetAccessibleHexesNew(Unit unitToGetAccessibleHexes)
        {
            var enemyUnits = sm.allUnits.Where(unit => unit.playerId != unitToGetAccessibleHexes.playerId);
            var bfsResult = GraphSearch.BFSGetRange(unitToGetAccessibleHexes,enemyUnits);
            var returnHexes = bfsResult.hexesInRange;
            var attackableUnits = bfsResult.attackableUnits;
        
            foreach (var hex in returnHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Selected);
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
            Debug.Log($"Received Accessible Hexes Count : {sm.accessibleHexes.Count}");
            sm.OnAccessibleHexesReceived();
        }

        protected override void OnNothingClicked()
        {
            base.OnNothingClicked();
            sm.ChangeState(sm.idleState);
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
                sm.ChangeState(sm.unitMovingState);
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
            Debug.Log($"Attacking {hex.currentUnit}, on {hex}");
            sm.attackingUnit = movingUnit;
            sm.attackedUnit = hex.currentUnit;
            sm.SetAttackingUnits(movingUnit,hex.currentUnit);
            sm.ChangeState(sm.unitMovingState);
        }

        public override void Exit()
        {
            sm.accessibleHexesReceived = false;
            sm.ResetAccessibleHexesTrigger();
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }

}

