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
            sm.debugText.text = this.ToString();
            
        }

        public override void UpdateLogic()
        {
            if(sm.clickedUnit) OnUnitSelected();
            if(sm.clickedHex) OnHexSelected();
        }

        private void OnUnitSelected()
        {
            sm.clickedUnit = false;
            sm.ChangeState(sm.movementSelectionState);
        }

        private void OnHexSelected()
        {
            sm.clickedHex = false;
        }
    }
}