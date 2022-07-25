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
    public int currentPlayerId;
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
        
        currentPlayerId = -1;
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

    public void SetupPlayersFactionCallbacks()
    {
        foreach (var player in players)
        {
            var scriptableFaction = ObjectIDList.GetFactionScriptable(player.factionId);
            Debug.Log($"Setting Up Events of {scriptableFaction.name} for Player {player.playerId}");
            scriptableFaction.SetupEvents(player);
        }
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
    
    public void PlaceUnits()
    {
        StartCoroutine(UnitPlacementRoutine());
    }

    private IEnumerator UnitPlacementRoutine()
    {
        foreach (var player in players)
        {
            ObjectIDList.GetUnitPlacementScriptable(player.unitPlacementPresetId).SpawnUnits(player.playerId);
        }
        
        yield return null;
        
        foreach (var player in players)
        {
            HexGrid.instance.SetPlayerLists(player);
        }
        
        unitsPlaced = true;
    }

    #endregion
    
    public void ChangePlayer(int player)
    {
        currentPlayerId = player;
        debugText2.text = $"Current player : {currentPlayerId}";
        foreach (var playe in players)
        {
            playe.DisplayIfItsYourTurn(player);
        }
    }

    public void ResetPlayerActions(PlayerSM playerSm)
    {
        playerSm.unitsToActivate = playerSm.maxActions;

        var hexGrid = HexGrid.instance;
        foreach (var unit in hexGrid.units.Where(unit => unit.playerId == currentPlayerId))
        {
            unit.hasBeenActivated = false;
            unit.move = unit.baseMove;
            unit.attacksLeft = unit.attacksPerTurn;
            unit.canUseAbility = true;
        }
    }

    #region Accessible Tiles For Unit Movement  

    public void ServerSideSetAccessibleHexesNew(Unit unitToGetAccessibleHexes)
    {
        var enemyUnits = HexGrid.instance.units.Where(unit => unit.playerId != unitToGetAccessibleHexes.playerId);
        var bfsResult = GraphSearch.BFSGetRange(unitToGetAccessibleHexes,enemyUnits,unitToGetAccessibleHexes.attacksLeft > 0);
        var accessibleHexes = bfsResult.hexesInRange.Where(hex => !hex.HasUnitOfPlayer(0) && !hex.HasUnitOfPlayer(1));
        var attackableDict = bfsResult.attackableUnitsDict;
        var attackableUnits = bfsResult.attackableUnits;
        
        var player = unitToGetAccessibleHexes.player;
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

    #region Ability Selectable

    public void ServerSideSetAbilitySelectableHexes(Unit castingUnit,byte abilityIndex)
    {
        var scriptableAbility = ObjectIDList.GetAbilityScriptable(abilityIndex);
        if (scriptableAbility is IAbilityCallBacks abilityCallback)
        {
            var selectableHexes = abilityCallback.AbilitySelectables(castingUnit);                                 
        
            var player = castingUnit.player;
            player.entitiesToSelect = scriptableAbility.abilityTargetCount;
            player.SetAbilitySelectables(selectableHexes);
        }
    }

    #endregion

    #region Ability Resolve

    public void ServerSideAbilityResolve(Unit castingUnit, IEnumerable<Hex> targets, PlayerSM player)
    {
        var enumerable = targets as Hex[] ?? targets.ToArray();
        if (player.ConsumeFaith(castingUnit.currentAbilityCost))
        {
            player.ServerAbilityResolve(castingUnit,enumerable);
            player.RpcAbilityResolve(castingUnit,enumerable);
        }
        else
        {
            player.unitAbilityAnimationDone = true;
            player.castingUnit = null;
        }
        RefreshUnitHuds();
    }

    #endregion

    #region Victory

    public void GainVictoryPointIfAlliedUnitIsOnFort(PlayerSM playerSm)
    {
        if(currentPlayerId != playerSm.playerId) return;
        Debug.Log($"Checking Victory points for Player {playerSm.playerId}");
        var noEnemyUnitsOnFort = true;
        var gainedAtLeastOnePoint = false;
        Debug.Log($"Found {playerSm.allUnits.Where(unit => !unit.isDead).Where(unit => unit.currentHex.currentTileID == 3).ToList().Count} alive units on Fort Tiles");
        foreach (var unit in playerSm.allUnits.Where(unit => !unit.isDead).Where(unit => unit.currentHex.currentTileID == 3))
        {
            Debug.Log($"The player of the unit is {unit.player} ({unit.player.playerId}), checking for player {playerSm.playerId}");
            if(unit.player == playerSm)
            {
                Debug.Log("Its an ally, Increasing Victory and giving 1 faith");
                playerSm.victoryPoints++;
                playerSm.faith++;
                gainedAtLeastOnePoint = true;
            }
            else
            {
                Debug.Log("Its an enemy, no Bonus Point");
                noEnemyUnitsOnFort = false;
            }
        }
        
        if(noEnemyUnitsOnFort && gainedAtLeastOnePoint) playerSm.victoryPoints++;
    }
    
    public bool CheckIfPlayerWon()
    {
        foreach (var player in players)
        {
            if (VictoryByElimination(player) || VictoryByVictoryPoints(player))
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
    
    public bool VictoryByVictoryPoints(PlayerSM player)
    {
        return player.victoryPoints >= 10;
    }

    public void DisconnectPlayers()
    {
        foreach (var player in players)
        {
            player.RpcOnEndGame(winner);
        }
    }

    #endregion
    
    public void RefreshUnitHuds()
    {
        foreach (var player in players)
        {
            player.RpcUIUpdateUnitHud();
        }
    }

    public static bool IsPlayerTurn(PlayerSM player)
    {
        return instance.currentPlayerId == player.playerId;
    }
    
    public static bool IsPlayerTurn(int playerId)
    {
        return instance.currentPlayerId == playerId;
    }
    
    
}
