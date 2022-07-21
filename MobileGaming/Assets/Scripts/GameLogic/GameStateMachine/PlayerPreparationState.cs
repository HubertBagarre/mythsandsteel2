using System.Collections;
using System.Collections.Generic;
using CallbackManagement;
using UnityEngine;

namespace GameStates
{
    public class PlayerPreparationState : BaseState
    {
        private GameSM sm;

        public PlayerPreparationState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
        
        public override void Enter()
        {
            if (sm.CheckIfPlayerWon())
            {
                sm.ChangeState(sm.endingState);
                return;
            }
            
            CallbackManager.AnyPlayerTurnStart();
            
            Debug.Log($"Calling player start for player {sm.players[sm.currentPlayerId].playerId}");
            CallbackManager.PlayerTurnStart(sm.players[sm.currentPlayerId]);
            
            sm.ChangeState(sm.playerTurnState);
        }

        public override void Exit()
        {
            sm.RefreshUnitHuds();
            
            
        }
    }
}