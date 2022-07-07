using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class UnitPlacementState : BaseState
    {
        private GameSM sm;

        public UnitPlacementState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            sm.ChangeState(sm.prePlayerTurn);
        }
    }
}