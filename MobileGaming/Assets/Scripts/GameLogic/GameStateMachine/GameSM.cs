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

    [Header("Debug")]
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugText2;

    [Header("Game Info")]
    public int currentPlayer;
    public PlayerSM[] players = new PlayerSM[2];
    public int winner;
    
    [Header("Trash Flags")]
    public bool isMapGenerated;
    public bool unitsPlaced;
    public bool player0UnitsPlaced;
    public bool player1UnitsPlaced;
    
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
        isMapGenerated = false;
        unitsPlaced = false;

        player0UnitsPlaced = false;
        player1UnitsPlaced = false;

        currentPlayer = -1;
        winner = -1;
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
        
        hexGrid.SetNeighbours();

        hexGrid.CenterCamera();
        
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
            new Vector2Int(0,4),
            new Vector2Int(2,4),
            new Vector2Int(4,4),
            new Vector2Int(6,4),
            new Vector2Int(8,4),
        };
        
        var positions0 = new List<Vector2Int>()
        {
            new Vector2Int(0,6),
            new Vector2Int(2,6),
            new Vector2Int(4,6),
            new Vector2Int(6,6),
            new Vector2Int(8,6),
        };

        yield return null;
        
        PlacePlayerUnits(null,positions1.ToArray(),1);
        PlacePlayerUnits(null,positions0.ToArray(),0);

        yield return new WaitForSeconds(1f);

        foreach (var player in players)
        {
            HexGrid.instance.SetPlayerLists(player);
        }
        
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
            unit.attacksLeft = unit.attacksPerTurn;
        }
    }

    #region Accessible Tiles For Unit Movement  

    public void ServerSideSetAccessibleHexesNew(Unit unitToGetAccessibleHexes)
    {
        Debug.Log("Setting Accessible Hexes");

        var enemyUnits = HexGrid.instance.units.Where(unit => unit.playerId != unitToGetAccessibleHexes.playerId);
        var bfsResult = GraphSearch.BFSGetRange(unitToGetAccessibleHexes,enemyUnits,unitToGetAccessibleHexes.attacksLeft > 0);
        var accessibleHexes = bfsResult.hexesInRange;
        var attackableDict = bfsResult.attackableUnitsDict;
        var attackableUnits = bfsResult.attackableUnits;
        
        var player = players[unitToGetAccessibleHexes.playerId];
        player.SetAccessibleHexes(accessibleHexes,attackableUnits,attackableDict);
    }
    
    #endregion
    
    #region Unit Movement
    
    public void ServerSideSetUnitMovementPath(Unit movingUnit,Hex destinationHex,PlayerSM player)
    {
        Debug.Log("Setting Accessible Hexes Before Moving");
        if (movingUnit.currentHex == destinationHex)
        {
            Debug.Log("Unit doesn't have to move");
            player.unitMovementAnimationDone = true;
            return;
        }

        var enemyUnits = HexGrid.instance.units.Where(unit => unit.playerId != movingUnit.playerId);
        var bfsResult = GraphSearch.BFSGetRange(movingUnit,enemyUnits,false);
        var path = bfsResult.GetPathTo(destinationHex);
        
        ServerSideUnitMovement(movingUnit,path.ToArray(),player);
    }

    private void ServerSideUnitMovement(Unit movingUnit,Hex[] path,PlayerSM player)
    {
        player.ServerMoveUnit(movingUnit,path);
        player.RpcMoveUnit(movingUnit,path);
        RefreshUnitHuds();
    }
    
    #endregion
    
    #region Unit Attack

    public void ServerSideAttackResolve(Unit attacking, Unit attacked,PlayerSM player)
    {
        player.ServerAttackResolve(attacking,attacked);
        player.RpcAttackResolve(attacking,attacked);
        RefreshUnitHuds();
    }
    
    #endregion

    #region Victory
    
    public bool CheckIfPlayerWon()
    {
        foreach (var player in players)
        {
            if (VictoryByElimination(player))
            {
                winner = player.playerId;
                return true;
            }
        }

        return false;
    }
    
    public bool VictoryByElimination(PlayerSM player)
    {
        return player.allUnits.All(unit => unit.playerId == player.playerId || unit.isDead);
    }

    public void DisconnectPlayers()
    {
        foreach (var player in players)
        {
            player.RpcEndGame(winner);
        }
    }

    #endregion
    
    public void RefreshUnitHuds()
    {
        foreach (var player in players)
        {
            player.RpcUpdateUnitHud();
        }
    }
    
    
}
