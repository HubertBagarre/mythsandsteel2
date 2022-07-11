using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class StartingGameState : BaseState
    {
        private GameSM sm;
        private HexGrid hexGrid;
    
        public StartingGameState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            hexGrid = HexGrid.instance;

            StartGame();
        }

        private void StartGame()
        {
            sm.SelectRandomPlayer();
            
            hexGrid.GenerateMap();
            
            sm.isMapGenerated = true;
            
            sm.ChangeState(sm.placementState);
        }
    }
}


