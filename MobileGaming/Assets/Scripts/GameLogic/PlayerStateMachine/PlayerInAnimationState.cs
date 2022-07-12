using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerInAnimationState : BaseState
    {
        private PlayerSM sm;

        public PlayerInAnimationState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
    }
}

