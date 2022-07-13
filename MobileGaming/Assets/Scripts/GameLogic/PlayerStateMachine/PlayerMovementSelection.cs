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
        private Unit selectedUnit;
        private bool waitingForAccessibleHexes;
        
        private bool waitingForPath;
        
        
        private HexGrid hexGrid;
        private Hex startingHex;
        private Unit unitToMove;

        private bool accessibleHexesDisplayed;
        
        public PlayerMovementSelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            selectedUnit = sm.selectedUnit;
            waitingForAccessibleHexes = true;

            sm.accessibleHexesReceived = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAccessibleHexesNew(selectedUnit);
            
            sm.GetAccessibleHexesForUnitMovement();
        }
        
        private void ClientSideSetAccessibleHexesNew(Unit unitToGetAccessibleHexes)
        {
            var enemyUnits = sm.allUnits.Where(unit => unit.playerId != unitToGetAccessibleHexes.playerId);
            var bfsResult = GraphSearch.BFSGetRange(unitToGetAccessibleHexes,enemyUnits);
            var returnHexes = bfsResult.GetHexesInRange();
            var attackableUnits = bfsResult.GetAttackableUnits;
        
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
            if(sm.accessibleHexesReceived && waitingForAccessibleHexes) OnAccessibleHexesReceived();
            if(waitingForAccessibleHexes) return;
            if (sm.clickedNothing) OnNothingClicked();
            if(sm.clickedUnit) OnUnitClicked();
            if(sm.clickedHex) OnHexClicked();
        }
        
        private void OnAccessibleHexesReceived()
        {
            waitingForAccessibleHexes = false;
            sm.ResetAccessibleHexesTrigger();
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
                sm.SetUnitMovementUnitAndHex(selectedUnit,selectedHex);
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
            //sm.ChangeState(sm.unitMovingState);
        }

        public override void Exit()
        {
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }

}

