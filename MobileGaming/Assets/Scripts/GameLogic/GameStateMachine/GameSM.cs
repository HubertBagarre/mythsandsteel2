using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameStates;
using Mirror;
using TMPro;

public class GameSM : StateMachine
{
    public GameLobbyState lobbyState;
    public StartingGameState startingState; // Player Select, Setting up Game
    public UnitPlacementState placementState; // Placement Phase
    public PlayerTurnState playerTurnState; // Waits for player input (move, attack/ability or end turn)
    public PlayerResolveState playerResolveState; // End of player Turn effects
    public PlayerPreparationState playerPreparationState; // Before player Turn effects
    public EndingGameState endingState; // Winner Display, End of Game
    public BetweenTurnState betweenTurnState; // Between Turn effects

    [Header("Debug")]
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugText2;

    [Header("Game Info")]
    public int currentPlayer;
    
    public PlayerSM[] players = new PlayerSM[2];
    
    public static GameSM instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(this);

        lobbyState = new GameLobbyState(this);
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
        currentPlayer = -1;

        return startingState;
    }
    
    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        debugText.text = currentState.ToString();
    }
    
    #region StartingGame

    [Server]
    public void SelectRandomPlayer()
    {
        ChangePlayer(Random.Range(0, 2));
        
        RpcUpdatePlayerUI();
    }

    #endregion

    public void AllowPlayerSend(int player)
    {
        
    }
    
    public void ChangePlayer(int player)
    {
        currentPlayer = player;
        debugText2.text = $"Current player : {currentPlayer}";
    }

    [ClientRpc]
    private void RpcUpdatePlayerUI()
    {
        debugText2.text = $"Current player : {currentPlayer}";
    }
}
