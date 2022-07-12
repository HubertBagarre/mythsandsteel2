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
        
        
        private HashSet<Hex> accessibleHex = new ();

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
                hex.ChangeHexColor(Hex.HexColors.Selectable);
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
            sm.clickedUnit = false;
            sm.ChangeState(sm.idleState);
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;
            sm.ChangeState(sm.idleState);
            return;
            
            var selectedHex = sm.selectedHex;
            
            if (accessibleHex.Contains(sm.selectedHex) && selectedHex != startingHex && selectedHex.currentUnit == null)
            {
                hexGrid.SetPath(sm.selectedHex,accessibleHex.ToArray());
                waitingForPath = true;
            }
            else
            {
                sm.ChangeState(sm.idleState);
            }
        }
        
        private void OnPathFound()
        {
            waitingForPath = false;
            var path = hexGrid.path.ToArray().Reverse().ToArray();
            sm.TryToMoveUnit(unitToMove,path);
            sm.ChangeState(sm.idleState);
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

