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
}