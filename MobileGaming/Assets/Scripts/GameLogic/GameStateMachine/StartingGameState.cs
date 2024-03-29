using System.Collections;
using System.Collections.Generic;
using CallbackManagement;
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
                player.InitiateVariables();
            }
            
            
            //Setting Callbacks
            CallbackManager.InitEvents();
            CallbackManager.OnAnyPlayerTurnStart += sm.RefreshUnitHuds;
            CallbackManager.OnPlayerTurnStart += sm.ResetPlayerActions;
            CallbackManager.OnPlayerTurnStart += sm.GainVictoryPointIfAlliedUnitIsOnFort;
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


