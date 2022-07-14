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

            ResetTempVariables();
            sm.ResetTempVariables();
            sm.RefreshUnitOutlines();
        }
        
        private void ResetTempVariables()
        {
            sm.unitMovementHex = null;
            sm.unitMovementUnit = null;
            sm.attackedUnit = null;
            sm.attackingUnit = null;
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
            if (unit.playerId != sm.playerId) return;
            if (!sm.canSendInfo) return;
            if (sm.actionsLeft > 0 || unit.hasBeenActivated)
            {
                sm.ChangeState(sm.movementSelectionState);
            }
        }
    }
}