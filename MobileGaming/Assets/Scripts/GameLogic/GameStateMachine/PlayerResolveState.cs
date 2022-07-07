using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class PlayerResolveState : BaseState
    {
        private GameSM sm;

        public PlayerResolveState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
        
        public override void Enter()
        {
            sm.ChangeState(sm.betweenTurnState);
        }
    }
}