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
            var movementUnit = sm.unitMovementUnit;
            sm.ChangeState(sm.idleState);
            if((movementUnit.attacksLeft>0 && movementUnit.AreEnemyUnitsInRange()) || (movementUnit.move>0 && movementUnit.canUseAbility)) sm.CmdSendUnitClicked(movementUnit);
        }

        private void OnUnitAttackAnimationDone()
        {
            sm.unitAttackAnimationDone = false;
            sm.CmdResetAttackAnimationDoneTrigger();
            
            var movementUnit = sm.unitMovementUnit;
            sm.ChangeState(sm.idleState);
            if((movementUnit.attacksLeft>0 && movementUnit.AreEnemyUnitsInRange()) || (movementUnit.move>0 && movementUnit.canUseAbility)) sm.CmdSendUnitClicked(movementUnit);
        }

        public override void Exit()
        {
            
        }
    }
}

