using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class BasePlayerState : BaseState
    {
        protected PlayerSM sm;
        public bool onNothingClickedTriggered;
        public bool onUnitClickedTriggered;
        public bool onHexClickedTriggered;
        
        public BasePlayerState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            onNothingClickedTriggered = false;
            onUnitClickedTriggered = false;
            onHexClickedTriggered = false;
        }

        public override void UpdateLogic()
        {
            if (sm.clickedNothing) CheckIfCanExecuteOnNothingClicked();
                
            if (sm.clickedUnit) CheckIfCanExecuteOnUnitClicked();
            
            if (sm.clickedHex) CheckIfCanExecuteOnHexesClicked();
        }

        private void CheckIfCanExecuteOnNothingClicked()
        {
            Debug.Log($"Clicked Nothing, it has been triggered : {onNothingClickedTriggered}");
            sm.clickedNothing = false;
            sm.OnNothingClicked();
            if (onNothingClickedTriggered) return;
            onNothingClickedTriggered = true;
            OnNothingClicked();
        }
        
        protected virtual void OnNothingClicked()
        {
            Debug.Log($"Clicked Nothing, Executing {nameof(OnNothingClicked)}");
        }
        
        private void CheckIfCanExecuteOnUnitClicked()
        {
            Debug.Log($"Clicked Unit : {sm.selectedUnit}");
            sm.clickedUnit = false;
            sm.OnUnitClicked();
            if (onUnitClickedTriggered) return;
            onUnitClickedTriggered = true;
            OnUnitClicked();
        }
        
        protected virtual void OnUnitClicked()
        {
            Debug.Log($"Clicked a Unit, Executing {nameof(OnUnitClicked)}");
        }
        
        private void CheckIfCanExecuteOnHexesClicked()
        {
            Debug.Log($"Clicked Hex : {sm.selectedHex}");
            sm.clickedHex = false;
            sm.OnHexClicked();
            if (onHexClickedTriggered) return;
            onHexClickedTriggered = true;
            OnHexClicked();
        }
        
        protected virtual void OnHexClicked()
        {
            Debug.Log($"Clicked a Hex, Executing {nameof(OnHexClicked)}");
        }
    }
}