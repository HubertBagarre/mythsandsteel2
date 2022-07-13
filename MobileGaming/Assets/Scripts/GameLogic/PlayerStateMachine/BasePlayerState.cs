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
            if (sm.clickedNothing && !onNothingClickedTriggered) OnNothingClicked();
            if (sm.clickedUnit && !onUnitClickedTriggered) OnUnitClicked();
            if (sm.clickedHex && !onHexClickedTriggered) OnHexClicked();
        }

        protected virtual void OnNothingClicked()
        {
            Debug.Log("Clicked Nothing");
            onNothingClickedTriggered = true;
            sm.clickedNothing = false;
            sm.OnNothingClicked();
        }
        
        protected virtual void OnUnitClicked()
        {
            Debug.Log($"Clicked Unit : {sm.selectedUnit}");
            onUnitClickedTriggered = true;
            sm.clickedUnit = false;
            sm.OnUnitClicked();
        }
        
        protected virtual void OnHexClicked()
        {
            Debug.Log($"Clicked Hex : {sm.selectedHex}");
            onHexClickedTriggered = true;
            sm.clickedHex = false;
            sm.OnHexClicked();
        }
    }
}