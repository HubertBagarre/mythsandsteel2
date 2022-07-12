using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameStates
{
    public class PlayerTurnState : BaseState
    {
        private GameSM sm;
        private PlayerSM currentPlayer;

        public PlayerTurnState(GameSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }
        
        public override void Enter()
        {
            currentPlayer = sm.players[sm.currentPlayer];
            sm.AllowPlayerSend(sm.currentPlayer);
        }

        public override void UpdateLogic()
        {
            if(sm.playerTurnOver) EndTurn();
            if (currentPlayer.isAskingForAccessibleHexesForUnitMovement) OnUnitMovementAsk();

        }

        private void OnUnitMovementAsk()
        {
            currentPlayer.isAskingForAccessibleHexesForUnitMovement = false;
            var selectedUnit = currentPlayer.selectedUnit;
            Debug.Log($"Player {currentPlayer} is asking for accessibles hexes of {selectedUnit}, on hex {selectedUnit.currentHex}, with a movement of {selectedUnit.move}");
            sm.ServerSideSetAccessibleHexesNew(selectedUnit.currentHex,selectedUnit.move,currentPlayer);
        }

        private void EndTurn()
        {
            sm.playerTurnOver = false;
            currentPlayer.EndTurn();
            sm.ChangeState(sm.postPlayerTurn);
        }
    }
}