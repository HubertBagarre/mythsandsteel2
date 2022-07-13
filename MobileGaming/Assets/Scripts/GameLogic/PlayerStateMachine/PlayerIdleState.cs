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
            
            sm.debugText.text = $"Player {sm.playerId}, {this}";
            
            sm.RefreshUnitOutlines();
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            
            //TODO - Update selection info box
            
            var selectedUnit = sm.selectedUnit;

            if (selectedUnit.playerId == sm.playerId)
            {
                if (sm.canSendInfo)
                {
                    if (sm.actionsLeft > 0 || selectedUnit.hasBeenActivated)
                    {
                        Debug.Log("Going in Movement");
                        sm.ChangeState(sm.movementSelectionState);
                    }
                }
            }
        }

        protected override void OnHexClicked()
        {
            base.OnHexClicked();
            var hex = sm.selectedHex;
            for (int i = 0; i < 3; i++)
            {
                Debug.Log($"{hex} has {hex.GetNeighborsInRange(i).Count()} neighbours in a range of {i}");
            }
        }
    }
}