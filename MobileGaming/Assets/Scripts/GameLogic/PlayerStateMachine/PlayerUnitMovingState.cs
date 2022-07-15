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

            if (sm.attackingUnit != null && sm.attackedUnit != null)
            {
                Debug.Log($"ATTACK MODE");
            }
            
            Debug.Log($"Going to move {sm.unitMovementUnit} to {sm.unitMovementHex}");

            sm.GetPathForUnitMovement();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            if(sm.unitMovementAnimationDone) OnUnitMovementAnimationDone();
            if(sm.unitAttackAnimationDone) OnUnitAttackAnimationDone();
            sm.UpdateUnitHud();
        }
        
        private void OnUnitMovementAnimationDone()
        {
            sm.unitMovementAnimationDone = false;
            sm.CmdResetMovementAnimationDoneTrigger();
            if (sm.attackingUnit != null && sm.attackedUnit != null)
            {
                sm.CmdTryToResolveAttack();
                return;
            }
            sm.ChangeState(sm.idleState);
        }

        private void OnUnitAttackAnimationDone()
        {
            sm.unitAttackAnimationDone = false;
            sm.CmdResetAttackAnimationDoneTrigger();
            
            sm.ChangeState(sm.idleState);
        }

        public override void Exit()
        {
            
        }
    }
}

