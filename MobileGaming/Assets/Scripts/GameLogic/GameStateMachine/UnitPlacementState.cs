using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class UnitPlacementState : BaseState
    {
        private GameSM sm;
        private HexGrid hexGrid;

        public UnitPlacementState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            
        }

        public override void UpdateLogic()
        {
            if(sm.player0UnitsPlaced && sm.player1UnitsPlaced) OnUnitsPlaced();
        }

        private void OnUnitsPlaced()
        {
            
        }
    }
}