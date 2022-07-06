using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameStates;

public class GameSM : StateMachine
{
    public StartingGameState startingState;
    public UnitPlacementState placementState;
    public PlayerTurnState playerTurnState;
    public PlayerResolveState playerResolveState;s
    public PlayerPreparationState playerPreparationState;
    public EndingGameState endingState;
    public BetweenTurnState betweenTurnState;
    
    
    private void Awake()
    {
        startingState = new StartingGameState(this);
        placementState = new UnitPlacementState(this);
        playerTurnState = new PlayerTurnState(this);
        playerResolveState = new PlayerResolveState(this);
        playerPreparationState = new PlayerPreparationState(this);
        endingState = new EndingGameState(this);
        betweenTurnState = new BetweenTurnState(this);
        
    }

    protected override BaseState GetInitialState()
    {
        
        
        return startingState;
    }
}
