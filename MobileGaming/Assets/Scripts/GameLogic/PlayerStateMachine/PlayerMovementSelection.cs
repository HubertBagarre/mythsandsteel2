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
        
        private HexGrid hexGrid;
        private Hex startingHex;
        private Unit unitToMove;

        private bool accessibleHexesDisplayed;
        private bool waitingForPath;
        
        private List<Hex> accessibleHex = new ();

        public PlayerMovementSelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            accessibleHexesDisplayed = false;
            waitingForPath = false;
            
            unitToMove = sm.selectedUnit;
            startingHex = unitToMove.currentHex;
            
            hexGrid = sm.hexGrid;
            hexGrid.SetAccessibleHexes(startingHex,unitToMove.move);
        }

        private void ColorAccessibleTiles()
        {
            foreach (var hex in hexGrid.hexes.Values)
            {
                var state = accessibleHex.Contains(hex) ? Hex.HexColors.Selectable : Hex.HexColors.Unselectable;
                hex.ChangeHexColor(state);
            }
        }
        
        public override void UpdateLogic()
        {
            if (waitingForPath && !hexGrid.isFindingPath)
            {
                OnPathFound();
                return;
            }
            
            if (accessibleHexesDisplayed && !waitingForPath)
            {
                if(sm.clickedUnit) OnUnitSelected();
                if(sm.clickedHex) OnHexSelected();
                return;
            }
            
            if(!hexGrid.isFindingHex) OnHexGridAccessibleHexesRead();
        }

        private void OnHexGridAccessibleHexesRead()
        {
            accessibleHexesDisplayed = true;
            accessibleHex = new List<Hex>(hexGrid.hexesToReturn);
            ColorAccessibleTiles();
        }

        private void OnUnitSelected()
        {
            sm.clickedUnit = false;
            sm.ChangeState(sm.idleState);
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;
            if (accessibleHex.Contains(sm.selectedHex) && sm.selectedHex != startingHex)
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
            hexGrid.MoveUnit(unitToMove,path);
            sm.ChangeState(sm.idleState);
        }
        

        public override void Exit()
        {
            foreach (var hex in hexGrid.hexes.Values)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }

}

