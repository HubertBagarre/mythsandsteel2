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

            sm.DisplayAbilityButton(false);

            sm.UIUpdateUnitHud();
            
            sm.UIUpdateFaithCount();
            sm.UIUpdateVictoryPointCount();

            sm.CmdCheckVictoryConditions();

            ResetTempVariables();
            sm.ResetTempVariables();
        }
        
        private void ResetTempVariables()
        {
            sm.unitMovementHex = null;
            sm.unitMovementUnit = null;
            sm.attackedUnit = null;
            sm.attackingUnit = null;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            sm.UIUpdateUnitHud();
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            
            //TODO - Update selection info box
            
            if(CanMoveOrAttackOrUseAbilityWithUnit(sm.selectedUnit)) EnterMovingState(sm.selectedUnit);
        }
        
        protected override void OnHexClicked()
        {
            base.OnHexClicked();
            
            var unit = sm.selectedHex.currentUnit;
            if (unit != null)
            {
                sm.CmdSendUnitClicked(unit);
            }
        }

        private bool CanMoveOrAttackOrUseAbilityWithUnit(Unit unit)
        {
            if (unit.playerId != sm.playerId)
            {
                Debug.Log("This is an enemy unit");
                return false;
            };
            
            return unit.canUseAbility || (unit.canAttack && unit.attacksLeft > 0 && unit.AreEnemyUnitsInRange())|| (unit.canMove && unit.move > 0);
        }
        
        private void EnterMovingState(Unit unit)
        {
            if (sm.unitsToActivate > 0 || unit.hasBeenActivated)
            {
                sm.ChangeState(sm.movementSelectionState);
            }
        }

        public override void Exit()
        {
            sm.UIUpdateUnitHud();
        }
    }
}