using System.Collections;
using System.Collections.Generic;
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
            sm.ResetPlayerActions(sm.players[sm.currentPlayer]);
            
            sm.RefreshUnitHuds();
            
            sm.ChangeState(sm.playerTurnState);
        }

        public override void Exit()
        {
            sm.RefreshUnitHuds();
        }
    }
}