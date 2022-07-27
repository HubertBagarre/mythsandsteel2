using System.Collections;
using System.Collections.Generic;
using CallbackManagement;
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
            sm.AllowPlayerSend(-1);
            
            sm.EndGameForPlayers();
            
            sm.debugText2.text = $"Player {sm.winner} won !";
        }

        public override void Exit()
        {
            sm.players[0] = null;
            sm.players[1] = null;
            
            sm.ResetHexGrid();

            CallbackManager.InitEvents();
        }
    }
}