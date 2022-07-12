using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerMovementSelection : BaseState
    {
        private PlayerSM sm;

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
            selectedUnit = sm.selectedUnit;
            waitingForAccessibleHexes = true;

            sm.accessibleHexesReceived = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAccessibleHexesNew(selectedUnit.currentHex, selectedUnit.move, sm);
            
            sm.GetAccessibleHexesForUnitMovement();
        }
        
        private void ClientSideSetAccessibleHexesNew(Hex startHex, int maxMovement, PlayerSM player)
        {
            var bfsResult = GraphSearch.BFSGetRange(startHex, maxMovement, player.playerId);
            var returnHexes = bfsResult.GetHexesInRange();
        
            foreach (var hex in returnHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Selected);
            }
        }
        
        public override void UpdateLogic()
        {
            if(sm.accessibleHexesReceived && waitingForAccessibleHexes) OnAccessibleHexesReceived();
            if(waitingForAccessibleHexes) return;
            if(sm.clickedUnit) OnUnitSelected();
            if(sm.clickedHex) OnHexSelected();
        }
        
        private void OnAccessibleHexesReceived()
        {
            sm.accessibleHexesReceived = false;
            Debug.Log($"Received Accessible Hexes Count : {sm.accessibleHexes.Count}");
            sm.OnAccessibleHexesReceived();
            waitingForAccessibleHexes = false;
        }
        
        private void OnUnitSelected()
        {
            Debug.Log("Clicked On A Unit");
            sm.clickedUnit = false;
            sm.ChangeState(sm.idleState);
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;

            var selectedHex = sm.selectedHex;

            Debug.Log("Accessible Hexes are");
            foreach (var hex in sm.accessibleHexes)
            {
                Debug.Log($"{hex}, in position {hex.col},{hex.row}");
            }
            Debug.Log($"Clicked on {selectedHex}, in position {selectedHex.col},{selectedHex.row}");
            
            if (sm.accessibleHexes.Contains(sm.selectedHex) && selectedHex != startingHex && selectedHex.currentUnit == null)
            {
                //hexGrid.SetPath(sm.selectedHex,accessibleHex.ToArray());
                //waitingForPath = true;
                
                sm.SetUnitMovementUnitAndHex(selectedUnit,selectedHex);
                sm.ChangeState(sm.unitMovingState);
            }
            else
            {                        
                sm.ChangeState(sm.idleState);
            }
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

