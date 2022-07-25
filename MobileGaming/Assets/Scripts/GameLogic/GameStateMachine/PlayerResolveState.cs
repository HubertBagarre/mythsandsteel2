using System.Collections;
using System.Collections.Generic;
using CallbackManagement;
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
            if (sm.CheckIfPlayerWon())
            {
                
                sm.ChangeState(sm.endingState);
                return;
            }
            
            CallbackManager.PlayerTurnEnd(sm.players[sm.currentPlayerId]);
            
            sm.RefreshUnitHuds();
            
            sm.ChangeState(sm.betweenTurnState);
        }

        public override void Exit()
        {
            sm.RefreshUnitHuds();
        }
    }
}