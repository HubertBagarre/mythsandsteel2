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
            if (currentPlayer.turnIsOver) EndTurn();
            if (currentPlayer.isAskingForAccessibleHexesForUnitMovement) OnAccessibleHexesForUnitMovementAsked();
            if (currentPlayer.isAskingForUnitMovement) OnUnitMovementAsked();
            if (currentPlayer.isAskingForAttackResolve) OnAttackResolveAsked();

        }

        private void OnAccessibleHexesForUnitMovementAsked()
        {
            currentPlayer.isAskingForAccessibleHexesForUnitMovement = false;
            
            var movingUnit = currentPlayer.unitMovementUnit;
            Debug.Log($"Player {currentPlayer} is asking for accessibles hexes of {movingUnit}, on hex {movingUnit.currentHex}, with a movement of {movingUnit.move}");
            sm.ServerSideSetAccessibleHexesNew(movingUnit);
        }

        private void OnUnitMovementAsked()
        {
            currentPlayer.isAskingForUnitMovement = false;
            var movingUnit = currentPlayer.unitMovementUnit;
            var destinationHex = currentPlayer.unitMovementHex;

            var attackingUnit = currentPlayer.attackingUnit;
            var attackedUnit = currentPlayer.attackedUnit;
            if (attackingUnit != null && attackedUnit != null)
            {
                Debug.Log($"ATTACK MODE");
                movingUnit = attackingUnit;
                if (Hex.DistanceBetween(attackedUnit.currentHex, attackedUnit.currentHex) <= attackedUnit.range)
                {
                    destinationHex = attackedUnit.currentHex;
                }
                else if(currentPlayer.attackableUnitDict.Count > 0)
                {
                    if (currentPlayer.attackableUnitDict.ContainsKey(attackedUnit))
                        destinationHex = currentPlayer.attackableUnitDict[attackedUnit];
                    else
                    {
                        Debug.LogWarning("KEY NOT FOUND");
                    } 
                }
            }
            
            Debug.Log(currentPlayer.unitMovementUnit);
            Debug.Log($"Player {currentPlayer} is asking to move {movingUnit}, on hex {movingUnit.currentHex}, to {destinationHex}");
            sm.ServerSideSetUnitMovementPath(movingUnit,destinationHex,currentPlayer);
        }

        private void OnAttackResolveAsked()
        {
            currentPlayer.isAskingForAttackResolve = false;

            sm.ServerSideAttackResolve(currentPlayer.attackingUnit, currentPlayer.attackedUnit,currentPlayer);
        }

        private void EndTurn()
        {
            currentPlayer.turnIsOver = false;
            currentPlayer.EndTurn();
            sm.ChangeState(sm.postPlayerTurn);
        }
    }
}