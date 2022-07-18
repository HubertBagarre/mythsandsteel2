using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

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
            sm.RefreshUnitHuds();
            
            currentPlayer = sm.players[sm.currentPlayer];
            
            sm.AllowPlayerSend(sm.currentPlayer);
            
            if(sm.CheckIfPlayerWon()) sm.ChangeState(sm.endingState);
        }

        public override void UpdateLogic()
        {
            if (currentPlayer.turnIsOver) EndTurn();
            if (currentPlayer.isAskingForAccessibleHexesForUnitMovement) OnAccessibleHexesForUnitMovementAsked();
            if (currentPlayer.isAskingForUnitMovement) OnUnitMovementAsked();
            if (currentPlayer.isAskingForAttackResolve) OnAttackResolveAsked();
            if (currentPlayer.isAskingForAbilitySelectables) OnAbilitySelectableAsked();

        }

        private void OnAccessibleHexesForUnitMovementAsked()
        {
            currentPlayer.isAskingForAccessibleHexesForUnitMovement = false;
            
            var movingUnit = currentPlayer.unitMovementUnit;
            sm.ServerSideSetAccessibleHexesNew(movingUnit);
        }

        private void OnAbilitySelectableAsked()
        {
            currentPlayer.isAskingForAbilitySelectables = false;
            
            var movingUnit = currentPlayer.unitMovementUnit;
            sm.ServerSideSetAbilitySelectableHexes(movingUnit);
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
                movingUnit = attackingUnit;
                if (Hex.DistanceBetween(attackingUnit.currentHex, attackedUnit.currentHex) <= attackedUnit.attackRange)
                {
                    Debug.Log("Unit Doesn't have to move");
                    destinationHex = attackingUnit.currentHex;
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

        public override void Exit()
        {
            sm.RefreshUnitHuds();
        }
    }
}