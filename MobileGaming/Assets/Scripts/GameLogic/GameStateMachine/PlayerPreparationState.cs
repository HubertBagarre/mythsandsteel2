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
    }
}