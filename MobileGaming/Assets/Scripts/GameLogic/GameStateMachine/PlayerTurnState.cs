using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class PlayerTurnState : BaseState
    {
        private GameSM sm;

        public PlayerTurnState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
        
        public override void Enter()
        {
            sm.AllowPlayerSend(sm.currentPlayer);
        }

        public override void UpdateLogic()
        {
            if(sm.playerTurnOver) EndTurn();
        }

        private void EndTurn()
        {
            sm.playerTurnOver = false;
            sm.ChangeState(sm.postPlayerTurn);
        }
    }
}