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
        
        private void StartGame()
        {
            sm.SelectRandomPlayer();
            
            sm.ChangeState(sm.placementState);
        }
    }
}


