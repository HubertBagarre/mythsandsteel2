using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class GameLobbyState : BaseState
    {
        private GameSM sm;

        public GameLobbyState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            sm.players[0] = null;
            sm.players[1] = null;
        }

        public override void UpdateLogic()
        {
            if(sm.players[0] != null && sm.players[1] != null && sm.players[1] != sm.players[0]) sm.ChangeState(sm.startingState);
        }
        
        
    }
}
