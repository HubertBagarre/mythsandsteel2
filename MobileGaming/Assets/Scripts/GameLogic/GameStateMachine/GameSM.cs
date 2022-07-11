using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameStates;
using Mirror;
using TMPro;
using Random = UnityEngine.Random;

public class GameSM : StateMachine
{
    public GameLobbyState lobbyState;
    public StartingGameState startingState; // Player Select, Setting up Game
    public UnitPlacementState placementState; // Placement Phase
    public PlayerTurnState playerTurnState; // Waits for player input (move, attack/ability or end turn)
    public PlayerResolveState postPlayerTurn; // End of player Turn effects
    public PlayerPreparationState prePlayerTurn; // Before player Turn effects
    public EndingGameState endingState; // Winner Display, End of Game
    public BetweenTurnState betweenTurnState; // Between Turn effects

    //private HexGrid hexGrid;

    [Header("Debug")]
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugText2;

    [Header("Game Info")]
    public int currentPlayer;
    public PlayerSM[] players = new PlayerSM[2];
    
    [Header("Trash Flags")]
    public bool isMapGenerated;
    public bool unitsPlaced;
    public bool player0UnitsPlaced;
    public bool player1UnitsPlaced;
    
    [Header("Triggers")]
    public bool playerTurnOver;
    
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
        postPlayerTurn = new PlayerResolveState(this);
        prePlayerTurn = new PlayerPreparationState(this);
        endingState = new EndingGameState(this);
        betweenTurnState = new BetweenTurnState(this);
        
    }

    public void ResetHexGrid()
    {
        Destroy(HexGrid.instance.gameObject);
        HexGrid.instance = null;
    }

    protected override BaseState GetInitialState()
    {
        InitVariables();

        return lobbyState;
    }

    private void InitVariables()
    {
        //hexGrid = HexGrid.instance;

        isMapGenerated = false;
        unitsPlaced = false;
        playerTurnOver = false;

        player0UnitsPlaced = false;
        player1UnitsPlaced = false;

        currentPlayer = -1;
    }
    
    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        debugText.text = currentState.ToString();
    }

    public void ReturnToLobby()
    {
        ChangeState(lobbyState);
    }
    
    public void AllowPlayerSend(int player)
    {
        players[0].canSendInfo = player == 0;
        players[1].canSendInfo = player == 1;
    }

    #region StartingGame

    [Server]
    public void SelectRandomPlayer()
    {
        ChangePlayer(Random.Range(0, 2));
    }

    public void StartGenerationRoutine()
    {
        StartCoroutine(MapGenerationRoutine());
    }

    private IEnumerator MapGenerationRoutine()
    {
        var hexGrid = HexGrid.instance;
        
        SelectRandomPlayer();

        yield return null;
        
        hexGrid.GenerateMap();

        yield return null;

        
        
        yield return null;
        
        foreach (var hex in hexGrid.hexes.Values)
        {
            hex.ApplyTileServer(hex.currentTileID);
        }
        
        isMapGenerated = true;
    }
    
    #endregion
    
    #region UnitPlacement

    public void AutoUnitPlacementReady()
    {
        StartCoroutine(AutoReadyRoutine());
    }

    private IEnumerator AutoReadyRoutine()
    {
        yield return null;
        player0UnitsPlaced = true;
        player1UnitsPlaced = true;
    }
    
    private static void PlacePlayerUnits(Unit[] units, Vector2Int[] positions,int player)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            NetworkSpawner.SpawnUnit(positions[i],Convert.ToSByte(player));
        }
    }

    public void PlaceUnits()
    {
        StartCoroutine(UnitPlacementRoutine());
    }

    private IEnumerator UnitPlacementRoutine()
    {
        var positions1 = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(2,0),
            new Vector2Int(4,0),
            new Vector2Int(6,0),
            new Vector2Int(8,0),
        };
        
        var positions0 = new List<Vector2Int>()
        {
            new Vector2Int(0,10),
            new Vector2Int(2,10),
            new Vector2Int(4,10),
            new Vector2Int(6,10),
            new Vector2Int(8,10),
        };

        yield return null;
        
        PlacePlayerUnits(null,positions1.ToArray(),1);
        PlacePlayerUnits(null,positions0.ToArray(),0);

        yield return new WaitForSeconds(1f);

        unitsPlaced = true;
    }

    #endregion
    
    public void ChangePlayer(int player)
    {
        currentPlayer = player;
        debugText2.text = $"Current player : {currentPlayer}";
        foreach (var playe in players)
        {
            playe.DisplayIfItsYourTurn(player);
        }
    }

    public void ResetPlayerActions(PlayerSM playerSm)
    {
        playerSm.actionsLeft = playerSm.maxActions;

        var hexGrid = HexGrid.instance;
        foreach (var unit in hexGrid.units.Where(unit => unit.playerId == currentPlayer))
        {
            unit.hasBeenActivated = false;
            unit.move = unit.baseMove;
        }
    }
}
