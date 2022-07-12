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
        
        
        
        private HexGrid hexGrid;
        private Hex startingHex;
        private Unit unitToMove;

        private bool accessibleHexesDisplayed;
        private bool waitingForPath;
        
        private HashSet<Hex> accessibleHex = new ();

        public PlayerMovementSelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            selectedUnit = sm.selectedUnit;

            sm.accessibleHexesReceived = false;
            
            sm.GetPathForUnit();
            
            return;
            
            
            accessibleHexesDisplayed = false;
            waitingForPath = false;
            
            unitToMove = sm.selectedUnit;
            startingHex = unitToMove.currentHex;
            
            hexGrid.SetAccessibleHexes(startingHex,unitToMove.move,unitToMove.playerId);
        }
        
        public override void UpdateLogic()
        {
            if(sm.clickedUnit && sm.accessibleHexesReceived) OnUnitSelected();
        }
        
        private void OnAccessibleHexesReceived()
        {
            sm.accessibleHexesReceived = false;
            Debug.Log($"Accessible Hexes Count : {sm.accessibleHexes.Count}");
        }
        
        private void OnUnitSelected()
        {
            sm.clickedUnit = false;
            sm.ChangeState(sm.idleState);
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;
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

