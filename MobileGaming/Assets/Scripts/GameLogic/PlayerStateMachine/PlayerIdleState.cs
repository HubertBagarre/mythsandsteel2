using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerIdleState : BasePlayerState
    {
        public PlayerIdleState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            sm.debugText.text = $"Player {sm.playerId}, {this}";
            
            sm.RefreshUnitOutlines();
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            
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
    }
}