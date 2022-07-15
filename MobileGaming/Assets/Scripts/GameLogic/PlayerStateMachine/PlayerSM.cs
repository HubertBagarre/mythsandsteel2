using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using PlayerStates;
using TMPro;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PlayerSM : StateMachine
{
    public PlayerIdleState idleState;
    public PlayerMovementSelection movementSelectionState;
    public PlayerAbilitySelection abilitySelectionState;
    public PlayerInactiveState inactiveState;
    public PlayerUnitMovingState unitMovingState;

    [Header("Network")] [SyncVar] public int playerId;
    [SyncVar(hook = nameof(OnCanInputValueChanged))] public bool canSendInfo;

    [Header("Game Information")]
    public readonly SyncHashSet<Unit> allUnits = new ();
    public readonly SyncHashSet<Hex> allHexes = new ();

    [Header("Unit Movement")]
    public readonly SyncHashSet<Hex> accessibleHexes = new();
    public readonly SyncHashSet<Hex> attackableHexes = new();
    public readonly SyncDictionary<Unit, Hex> attackableUnitDict = new();
    [SyncVar] public Unit unitMovementUnit;
    [SyncVar] public Hex unitMovementHex;

    [Header("Unit Attack")]
    [SyncVar] public Unit attackingUnit;
    [SyncVar] public Unit attackedUnit;

    [Header("User Interface")]
    [SerializeField] private PlayerUIManager uiManager;
    
    [Header("Selection")]
    [SyncVar] public Unit selectedUnit;
    [SyncVar] public Hex selectedHex;

    [Header("Inputs")] private PlayerInputManager inputManager;
    
    [SerializeField] private LayerMask layersToHit;
    
    [Header("Trigger Bools")]
    [SyncVar(hook = nameof(OnUnitClickedValueChanged))] public bool clickedUnit;
    [SyncVar(hook = nameof(OnHexClickedValueChanged))] public bool clickedHex;
    [SyncVar(hook = nameof(OnNothingClickedValueChanged))] public bool clickedNothing;
    [SyncVar] public bool isAskingForAccessibleHexesForUnitMovement;
    [SyncVar(hook = nameof(OnAccessibleHexesReceivedValueChange))] public bool accessibleHexesReceived;
    [SyncVar] public bool isAskingForUnitMovement;
    [SyncVar] public bool isAskingForAttackResolve;
    [SyncVar] public bool unitMovementAnimationDone;
    [SyncVar] public bool unitAttackAnimationDone;
    [SyncVar] public bool turnIsOver;
    
    
    private Camera cam;
    
    [Header("Actions")]
    [SyncVar] public int maxActions;
    [SyncVar(hook = nameof(OnActionsLeftValueChanged))] public int actionsLeft;
    
    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        movementSelectionState = new PlayerMovementSelection(this);
        abilitySelectionState = new PlayerAbilitySelection(this);
        inactiveState = new PlayerInactiveState(this);
        unitMovingState = new PlayerUnitMovingState(this);
        
        inputManager = PlayerInputManager.instance;
    }

    protected override BaseState GetInitialState()
    {
        if (!isLocalPlayer)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            return inactiveState;
        }
        
        cam = Camera.main;
        
        ResetTriggerVariables();

        uiManager.RefreshUnitOutlines(allUnits,playerId);
        uiManager.ChangeDebugText($"Player {playerId}, {currentState}");
        return idleState;
    }

    private void ResetTriggerVariables()
    {
        clickedUnit = false;
        clickedHex = false;
        isAskingForAccessibleHexesForUnitMovement = false;
        accessibleHexesReceived = false;
        isAskingForUnitMovement = false;
        isAskingForAttackResolve = false;
        unitMovementAnimationDone = false;
        turnIsOver = false;
    }
    
    public override void ChangeState(BaseState newState)
    {
        if(!isLocalPlayer) return;
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
        Debug.Log($"Entering {currentState}");
        RpcChangeDebugText();
    }

    #region Raycast Gaming
    
    private void TryToSelectUnitOrTile(Vector2 screenPosition,float time)
    {
        var ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out var hit, layersToHit))
        {
            var objectHit = hit.transform;
        
            Debug.Log(objectHit);
            
            var objectHex = objectHit.GetComponent<Hex>();
            if (objectHex != null)
            {
                SendHexClicked(objectHex);
                return;
            }
            
            var objectUnit = objectHit.GetComponent<Unit>();
            if (objectUnit != null)
            {
                SendUnitClicked(objectUnit);
            }
            
            return;
        }

        SendNothingClicked();
    }


    [Command]
    public void SendUnitClicked(Unit unit)
    {
        Debug.Log($"{unit} got clicked");
        selectedUnit = unit;
        clickedUnit = true;
    }
    
    [Command]
    public void SendHexClicked(Hex hex)
    {
        Debug.Log($"{hex} got clicked");
        selectedHex = hex;
        clickedHex = true;
    }
    
    [Command]
    public void SendNothingClicked()
    {
        Debug.Log("Nothing got clicked");
        clickedNothing = true;
    }
    
    #endregion
    
    
    public void SetUnitsAndHexesArrays(IEnumerable<Unit> units, IEnumerable<Hex> hexes)
    {
        allUnits.Clear();
        allHexes.Clear();
        foreach (var unit in units)
        {
            allUnits.Add(unit);
        }
        foreach (var hex in hexes)
        {
            allHexes.Add(hex);
        }
        //Debug.Log($"Set Lists, they have {allUnits.Count} and {allHexes.Count} elements");
    }
    
    #region Camera Management
    private void MoveCamera(Vector3 anchorPos,Vector3 camPos)
    {
        var camAnchor = cam.transform.parent;
        camAnchor.localPosition = anchorPos;
        camAnchor.localRotation = Quaternion.identity;
        camAnchor.GetChild(0).localPosition = camPos;
        camAnchor.GetChild(0).localRotation = Quaternion.Euler(new Vector3(70,0,0));
    }

    [ClientRpc]
    public void RpcMoveCamera(Vector3 anchorPos,Vector3 camPos)
    {
        if(!isLocalPlayer) return;
        MoveCamera(anchorPos,camPos);
    }

    #endregion

    #region IdleState

    [Command]
    public void ResetTempVariables()
    {
        unitMovementHex = null;
        unitMovementUnit = null;
        attackedUnit = null;
        attackingUnit = null;
    }

    #endregion
    
    #region Unit Accessible Hexes For Movement
    
    [Command]
    public void GetAccessibleHexesForUnitMovement()
    {
        isAskingForAccessibleHexesForUnitMovement = true;
        accessibleHexesReceived = false;
    }

    public void SetAccessibleHexes(IEnumerable<Hex> hexes,IEnumerable<Unit> attackable, Dictionary<Unit,Hex> dict)
    {
        accessibleHexes.Clear();
        attackableHexes.Clear();
        attackableUnitDict.Clear();
        
        foreach (var pair in dict)
        {
            attackableUnitDict.Add(pair);
        }
        
        foreach (var hex in hexes)
        {
            accessibleHexes.Add(hex);
        }

        foreach (var unit in attackable)
        {
            attackableHexes.Add(unit.currentHex);
        }

        accessibleHexesReceived = true;
        Debug.Log("Received Hexes");
    }

    [Command]
    public void ResetAccessibleHexesTrigger()
    {
        accessibleHexesReceived = false;
    }
    
    public void OnAccessibleHexesReceived()
    {
        if(!isLocalPlayer) return;
        
        foreach (var hex in allHexes)
        {
            var color = accessibleHexes.Contains(hex) ? Hex.HexColors.Selectable : Hex.HexColors.Unselectable;
            if (attackableHexes.Contains(hex)) color = Hex.HexColors.Attackable;
            hex.ChangeHexColor(color);
        }
        
    }
    
    #endregion

    #region Unit Movement

    [Command]
    public void SetUnitMovementUnit(Unit unit)
    {
        unitMovementUnit = unit;
    }
    
    [Command]
    public void SetUnitMovementHex(Hex hex)
    {
        unitMovementHex = hex;
    }

    [Command]
    public void GetPathForUnitMovement()
    {
        isAskingForUnitMovement = true;
    }
    
    public void ServerMoveUnit(Unit unitToMove, Hex[] path)
    {
        if(!unitToMove.hasBeenActivated) actionsLeft--;
        unitToMove.hasBeenActivated = true;
        StartCoroutine(MoveUnitRoutine(unitToMove, path));
    }
    
    [ClientRpc]
    public void RpcMoveUnit(Unit unitToMove, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unitToMove, path));
    }
    
    private IEnumerator MoveUnitRoutine(Unit unit, Hex[] path)
    {
        if(isServer) unitMovementAnimationDone = false;

        unit.currentHex.currentUnit = null;
        
        foreach (var hex in path)
        {
            if (isServer)
            {
                unit.currentHex.OnUnitExit(unit);
            
                hex.OnUnitEnter(unit);
                
                hex.DecreaseUnitMovement(unit);
            }
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;

            UpdateUnitHud();
            
            //TODO - Wait for animation to finish
            
            yield return new WaitForSeconds(0.5f);
            
        }

        if (isServer)
        {
            HexGrid.instance.SyncHexGridVariables();

            unit.currentHex.currentUnit = unit;
            
            unitMovementAnimationDone = true;
        }
        
        UpdateUnitHud();
    }
    
    [Command]
    public void ResetMovementAnimationDoneTrigger()
    {
        unitMovementAnimationDone = false;
    }

    #endregion
    
    #region Unit Attack
    
    [Command]
    public void ResetAttackAnimationDoneTrigger()
    {
        unitAttackAnimationDone = false;
    }
    
    [Command]
    public void SetAttackingUnits(Unit attacking,Unit attacked)
    {
        attackingUnit = attacking;
        attackedUnit = attacked;
    }
    
    [Command]
    public void TryToResolveAttack()
    {
        isAskingForAttackResolve = true;
    }

    public void ServerAttackResolve(Unit attacking,Unit attacked)
    {
        if(!attacking.hasBeenActivated) actionsLeft--;
        attacking.hasBeenActivated = true;
        attacking.move = 0;
        attacking.canUseAbility = false;
        attacking.attacksLeft--;
        StartCoroutine(PlayAttackAnimationRoutine(attacking, attacked));
    }

    [ClientRpc]
    public void RpcAttackResolve(Unit attacking,Unit attacked)
    {
        StartCoroutine(PlayAttackAnimationRoutine(attacking, attacked));
    }

    private IEnumerator PlayAttackAnimationRoutine(Unit attacking,Unit attacked)
    {
        if(isServer) unitAttackAnimationDone = false;
        
        yield return null;
        
        //TODO - Look at attacked
        //TODO - Play Animation
        
        yield return new WaitForSeconds(0.5f);
        
        if (isServer)
        {
            attacking.AttackUnit(attacked);
            attackingUnit = null;
            attackedUnit = null;
            unitAttackAnimationDone = true;
        }

        UpdateUnitHud();
    }
    
    #endregion
    
    private void TryToEndTurn()
    {
        if(currentState != idleState) return;
        turnIsOver = true;
        TryEndTurnCommand();
    }
    
    [Command]
    private void TryEndTurnCommand()
    {
        turnIsOver = true;
    }

    public void EndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    private void RpcEndTurn()
    {
        ChangeState(idleState);
    }

    [Command]
    public void TryToAttack()
    {
        Debug.Log("Attack");
    }

    [Command]
    public void TryToUseAbility()
    {
        Debug.Log("Ability");
    }

    [ClientRpc]
    public void DisplayIfItsYourTurn(int playerTurn)
    {
        // TODO - Replace with disable button and play animation
        
        uiManager.EnableNextTurnButton(playerId == playerTurn);
    }

    #region hooks

    [Command]
    public void OnNothingClicked()
    {
        Debug.Log($"SERVER Clicked Nothing");
        clickedNothing = false;
    }
    
    private void OnNothingClickedValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;

        if (currentState is BasePlayerState basePlayerState && newValue)
        {
            basePlayerState.onNothingClickedTriggered = false;
        }
    }
    
    [Command]
    public void OnUnitClicked()
    {
        clickedUnit = false;
    }
    
    private void OnUnitClickedValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;

        if (currentState is BasePlayerState basePlayerState && newValue)
        {
            basePlayerState.onUnitClickedTriggered = false;
        }
    }
    
    [Command]
    public void OnHexClicked()
    {
        clickedHex = false;
    }
    
    private void OnHexClickedValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;

        if (currentState is BasePlayerState basePlayerState && newValue)
        {
            basePlayerState.onHexClickedTriggered = false;
        }
    }

    private void OnActionsLeftValueChanged(int prevValue, int newValue)
    {
        uiManager.UpdateActionsLeft(newValue);
    }

    private void OnAccessibleHexesReceivedValueChange(bool prevValue,bool newValue)
    {
        
    } 
    
    private void OnCanInputValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;
        if (newValue)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
            uiManager.AddButtonListeners(TryToEndTurn,TryToUseAbility);
            uiManager.UpdateActionsLeft(actionsLeft);
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
            uiManager.RemoveButtonListeners(TryToEndTurn,TryToUseAbility);
        }
        uiManager.RefreshUnitOutlines(allUnits,playerId);
    }

    #endregion

    #region Update UI
    
    public void RpcChangeDebugText()
    {
        uiManager.ChangeDebugText($"Player {playerId}, {currentState}");
    }
    
    [ClientRpc]
    public void RpcSetupUnitHuds()
    {
        uiManager.GenerateUnitHuds(allUnits);
    }
    
    [ClientRpc]
    public void RpcUpdateUnitHud()
    {
        Debug.Log("Rpc Updating Unit Hud");
        uiManager.UpdateUnitHud();
    }

    public void UpdateUnitHud()
    {
        uiManager.UpdateUnitHud();
        if(isServer) GameSM.instance.RefreshUnitHuds();
    }

    #endregion
}
