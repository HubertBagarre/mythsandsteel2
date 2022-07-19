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
            sm.StartGenerationRoutine();
            
            foreach (var player in sm.players)
            {
                player.faith = 0;
            }
        }

        public override void UpdateLogic()
        {
            if(sm.isMapGenerated) StartGame();
        }

        private void StartGame()
        {
            sm.ChangeState(sm.placementState);
        }
    }
}


