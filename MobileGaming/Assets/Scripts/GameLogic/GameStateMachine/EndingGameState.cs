using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class EndingGameState : BaseState
    {
        private GameSM sm;

        public EndingGameState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
    }
}