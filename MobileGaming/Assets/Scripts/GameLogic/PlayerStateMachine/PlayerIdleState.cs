using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerIdleState : BaseState
    {
        private PlayerSM sm;

        public PlayerIdleState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            sm.debugText.text = $"Player {sm.playerId}, {this}";
            
            sm.RefreshUnitOutlines();
        }

        public override void UpdateLogic()
        {
            if(sm.clickedUnit) OnUnitSelected();
            if(sm.clickedHex) OnHexSelected();
        }

        private void OnUnitSelected()
        {
            sm.clickedUnit = false;
            
            //TODO - Update selection info box
            
            var selectedUnit = sm.selectedUnit;

            if (selectedUnit.playerId == sm.playerId)
            {
                if (sm.canSendInfo)
                {
                    if(sm.actionsLeft > 0 || selectedUnit.hasBeenActivated) sm.ChangeState(sm.movementSelectionState);
                }
            }
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;
            Debug.Log($"Clicked Hex {sm.selectedHex}");
            return;
        }
    }
}