using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerUnitMovingState : BasePlayerState
    {
        public PlayerUnitMovingState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            Debug.Log($"Going to move {sm.unitMovementUnit} to {sm.unitMovementHex}");

            sm.GetPathForUnitMovement();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            if(sm.unitMovementAnimationDone) OnUnitMovementAnimationDone();
        }
        
        private void OnUnitMovementAnimationDone()
        {
            sm.ResetMovementAnimationDoneTrigger();
            sm.ChangeState(sm.idleState);
        }

        public override void Exit()
        {
            
        }
    }
}

