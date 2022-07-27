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
            hexGrid = HexGrid.instance;
            
            sm.AutoUnitPlacementReady();
        }

        public override void UpdateLogic()
        {
            if(sm.unitsPlaced) OnUnitsPlaced();
            if(sm.player0UnitsPlaced && sm.player1UnitsPlaced) OnPlayerUnitsPlaced();
        }
        
        private void OnUnitsPlaced()
        {
            sm.unitsPlaced = false;
            hexGrid.ServerAssignUnitsToTiles();
            
            sm.SetupPlayersFactionCallbacks();
            
            sm.ChangeState(sm.prePlayerTurn);
        }

        private void OnPlayerUnitsPlaced()
        {
            sm.player0UnitsPlaced = false;
            sm.player1UnitsPlaced = false;
            sm.PlaceUnits();
        }

        public override void Exit()
        {
            SetupUnitHuds();
            
        }

        private void SetupUnitHuds()
        {
            foreach (var player in sm.players)
            {
                player.RpcUISetupUnitHuds();
            }

            sm.unitHudGenerated = true;
        }
    }
}