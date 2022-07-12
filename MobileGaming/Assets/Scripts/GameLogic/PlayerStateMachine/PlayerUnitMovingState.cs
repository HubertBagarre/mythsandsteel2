using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerUnitMovingState : BaseState
    {
        private PlayerSM sm;

        public PlayerUnitMovingState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            Debug.Log($"Going to move {sm.unitMovementUnit} to {sm.unitMovementHex}");

            sm.GetPathForUnitMovement();
            
            sm.ChangeState(sm.idleState);
        }

        public override void UpdateLogic()
        {
            if(sm.unitMovementReceived) OnUnitMovementReceived();
            if(sm.unitMovementAnimationDone) OnUnitMovementAnimationDone();
        }

        private void OnUnitMovementReceived()
        {
            sm.unitMovementReceived = false;
            var msg = "Received Unit Movement : ";
            foreach (var hex in sm.unitMovementPath)
            {
                msg = $"{msg} + Hex {hex.col},{hex.row}";
            }
            Debug.Log(msg);
            
            
            sm.ChangeState(sm.idleState);
        }

        private void OnUnitMovementAnimationDone()
        {
            sm.unitMovementAnimationDone = false;
            sm.ChangeState(sm.idleState);
        }

        public override void Exit()
        {
            
        }
    }
}

