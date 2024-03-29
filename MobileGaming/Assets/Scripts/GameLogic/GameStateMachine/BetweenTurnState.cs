using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenTurnState : BaseState
{
    private GameSM sm;
    
    public BetweenTurnState(GameSM stateMachine) : base(stateMachine)
    {
        sm = stateMachine;
    }
    
    public override void Enter()
    {
        ChangePlayer();
        sm.ChangeState(sm.prePlayerTurn);
    }

    private void ChangePlayer()
    {
        sm.ChangePlayer(sm.currentPlayerId == 1 ? 0 : 1);
    }
}