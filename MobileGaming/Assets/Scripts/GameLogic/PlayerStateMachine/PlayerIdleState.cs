using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            
            sm.RefreshUnitOutlines();
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            
            //TODO - Update selection info box

            EnterMovingState(sm.selectedUnit);
        }
        
        protected override void OnHexClicked()
        {
            base.OnHexClicked();

            var unit = sm.selectedHex.currentUnit;
            if (unit != null)
            {
                sm.SendUnitClicked(unit);
            }
        }
        
        private void EnterMovingState(Unit unit)
        {
            if (unit.playerId == sm.playerId)
            {
                if (sm.canSendInfo)
                {
                    if (sm.actionsLeft > 0 || unit.hasBeenActivated)
                    {
                        sm.ChangeState(sm.movementSelectionState);
                    }
                }
            }
        }
    }
}