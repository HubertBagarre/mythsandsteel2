using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerMovementSelection : BaseState
    {
        private PlayerSM sm;
        
        private List<Hex> accessibleHex = new ();

        public PlayerMovementSelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            accessibleHex.Clear();
            SetSelectableTiles(sm.selectedUnit.currentHex,sm.hexGrid.unitMovement);
            ColorAccessibleTiles();
        }

        private void ColorAccessibleTiles()
        {
            foreach (var hex in sm.hexGrid.hexes.Values)
            {
                var state = accessibleHex.Contains(hex) ? Hex.HexColors.Selectable : Hex.HexColors.Unselectable;
                hex.ChangeHexColor(state);
            }
        }

        private void SetSelectableTiles(Hex startingHex,sbyte movementLeft)
        {
            foreach (var hex in startingHex.GetAccessibleNeighbours(movementLeft))
            {
                movementLeft -= hex.movementCost;
                SetSelectableTiles(hex,movementLeft);
                accessibleHex.Add(hex);
            }
        }
        
        public override void UpdateLogic()
        {
            if(sm.clickedUnit) OnUnitSelected();
        }

        private void OnUnitSelected()
        {
            sm.clickedUnit = false;
            sm.ChangeState(sm.idleState);
        }

        public override void Exit()
        {
            foreach (var hex in sm.hexGrid.hexes.Values)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }

}

