using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerAbilitySelection : BaseState
    {
        private PlayerSM sm;

        public PlayerAbilitySelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
    }
}

