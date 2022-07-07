using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerInactiveState : BaseState
    {
        private PlayerSM sm;

        public PlayerInactiveState(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
    }
}
