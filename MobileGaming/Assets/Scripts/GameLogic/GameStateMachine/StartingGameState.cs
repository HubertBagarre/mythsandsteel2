using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class StartingGameState : BaseState
    {
        private GameSM sm;
    
        public StartingGameState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            sm.ChangePlayer(Random.Range(0, 2));
            
            sm.ChangeState(sm.placementState);
        }
    }
}


