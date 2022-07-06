using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            foreach (var hex in sm.hexGrid.hexes.Values)
            {
                hex.currentCostToMove = -1;
            }
            sm.finalAccessibleHex.Clear();
            sm.costMoreHex.Clear();
            sm.SetAccessibleHexes(sm.selectedUnit.currentHex,sm.hexGrid.unitMovement);
            sm.finalAccessibleHex.Remove(sm.selectedUnit.currentHex);
            //ColorAccessibleTiles();
        }

        private void ColorAccessibleTiles()
        {
            foreach (var hex in sm.hexGrid.hexes.Values)
            {
                var state = accessibleHex.Contains(hex) ? Hex.HexColors.Selectable : Hex.HexColors.Unselectable;
                hex.ChangeHexColor(state);
            }
        }
        
        private void SetAccessibleHexes(Hex startingHex,int movementLeft)
        {
            int movementLeftToUse = movementLeft;
            for (var dir = 0; dir < 6; dir++)
            {
                var hex = startingHex.neighbours[dir];
                if (hex != null)
                {
                    if (movementLeftToUse >= hex.movementCost && !accessibleHex.Contains(hex))
                    {
                        accessibleHex.Add(hex);
                        SetAccessibleHexes(hex,movementLeftToUse-hex.movementCost);
                    }
                }
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

