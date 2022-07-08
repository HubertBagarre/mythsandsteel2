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

        public override void Enter()
        {
            sm.players[0] = null;
            sm.players[1] = null;
        }

        public override void Exit()
        {
            sm.ResetInstance();
        }
    }
}