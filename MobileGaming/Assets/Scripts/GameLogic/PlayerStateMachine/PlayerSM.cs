using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using Mirror;
using UnityEngine;
using PlayerStates;

public class PlayerSM : StateMachine
{
    public PlayerIdleState idleState;
    public PlayerMovementSelection movementSelectionState;
    public PlayerAbilitySelection abilitySelectionState;
    public PlayerInactiveState inactiveState;
    public PlayerUnitInAnimationState unitInAnimationState;
    public PlayerRespawnUnitSelectionState respawnUnitSelectionState;

    [Header("Actions")]
    [SyncVar] public int maxActions;
    [SyncVar] public int factionId;
    [SyncVar(hook = nameof(OnActionsLeftValueChanged))] public int unitsToActivate;
    [SyncVar(hook = nameof(OnFaithValueChanged))] public int faith;
    [SyncVar] public int faithModifier;
    [SyncVar(hook = nameof(OnVictoryPointValueChanged))] public int victoryPoints;
    
    private Camera cam;
    public int unitPlacementPresetId;
    
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
    
    [Header("Unit Ability")]
    public readonly SyncHashSet<Hex> abilitySelectableHexes = new();
    [SyncVar] public byte abilityIndexToUse;
    [SyncVar] public Unit castingUnit;
    [SyncVar] public bool isAskingForAbilityResolve;
    public readonly SyncHashSet<Hex> selectedHexesForAbility = new();
    
    [Header("Unit Attack")]
    [SyncVar] public Unit attackingUnit;
    [SyncVar] public Unit attackedUnit;

    [Header("Ability Selection")]
    [SyncVar] public int entitiesToSelect;

    [Header("Unit Respawning")]
    [SyncVar] public Unit unitToRespawn;

    [Header("User Interface")]
    [SerializeField] private PlayerUIManager uiManager;
    
    [Header("Selection")]
    [SyncVar] public Unit selectedUnit;
    [SyncVar] public Hex selectedHex;

    [Header("Inputs")]
    private PlayerInputManager inputManager;
    [SerializeField] private LayerMask layersToHit;
    
    [Header("Trigger Bools")]
    [SyncVar(hook = nameof(OnUnitClickedValueChanged))] public bool clickedUnit;
    [SyncVar(hook = nameof(OnHexClickedValueChanged))] public bool clickedHex;
    [SyncVar(hook = nameof(OnNothingClickedValueChanged))] public bool clickedNothing;
    [SyncVar] public bool isAskingForAccessibleHexesForUnitMovement;
    [SyncVar(hook = nameof(OnAccessibleHexesReceivedValueChange))] public bool accessibleHexesReceived;
    [SyncVar] public bool isAskingForUnitMovement;
    [SyncVar] public bool isAskingForAttackResolve;
    [SyncVar] public bool isAskingForAbilitySelectables;
    [SyncVar(hook = nameof(OnAbilitySelectablesReceivedValueChange))] public bool abilitySelectablesReceived;
    [SyncVar] public bool unitAbilityAnimationDone;
    [SyncVar] public bool unitMovementAnimationDone;
    [SyncVar] public bool unitAttackAnimationDone;
    [SyncVar] public bool turnIsOver;
    
    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        movementSelectionState = new PlayerMovementSelection(this);
        abilitySelectionState = new PlayerAbilitySelection(this);
        inactiveState = new PlayerInactiveState(this);
        unitInAnimationState = new PlayerUnitInAnimationState(this);
        respawnUnitSelectionState = new PlayerRespawnUnitSelectionState(this);
        
        inputManager = PlayerInputManager.instance;
    }

    public void InitiateVariables()
    {
        maxActions = 3;
        faith = 6;
        faithModifier = 0;
        victoryPoints = 0;
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
        isAskingForAbilitySelectables = false;
        abilitySelectablesReceived = false;
        unitAbilityAnimationDone = false;
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
        UIChangeDebugText();
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
                CmdSendHexClicked(objectHex);
                return;
            }
            
            var objectUnit = objectHit.GetComponent<Unit>();
            if (objectUnit != null)
            {
                if(objectUnit.player == this) uiManager.UpdateUnitPortrait(objectUnit);
                CmdSendUnitClicked(objectUnit);
            }
            
            return;
        }

        CmdSendNothingClicked();
    }


    [Command]
    public void CmdSendUnitClicked(Unit unit)
    {
        Debug.Log($"{unit} got clicked");
        selectedUnit = unit;
        clickedUnit = true;
    }
    
    [Command]
    public void CmdSendHexClicked(Hex hex)
    {
        Debug.Log($"{hex} got clicked");
        selectedHex = hex;
        clickedHex = true;
    }
    
    [Command]
    public void CmdSendNothingClicked()
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
            if (unit.playerId == playerId) unit.player = this;
        }
        foreach (var hex in hexes)
        {
            allHexes.Add(hex);
        }
    }
    
    #region Camera Management
    private void MoveCamera(Vector3 anchorPos,Vector3 camPos,bool flipped)
    {
        var camTransform = cam.transform;
        var camAnchorRotation = camTransform.parent;
        var camAnchor = camAnchorRotation.parent;
        camAnchor.localPosition = anchorPos;
        camAnchor.localRotation = Quaternion.identity;
        camAnchorRotation.localPosition = Vector3.zero;
        camAnchorRotation.localRotation = Quaternion.identity;
        camTransform.localPosition = camPos;
        camTransform.localRotation = Quaternion.Euler(new Vector3(75,0,0));
        if(flipped) camAnchor.localRotation = Quaternion.Euler(new Vector3(0,180,0));
    }

    [ClientRpc]
    public void RpcMoveCamera(Vector3 anchorPos,Vector3 camPos)
    {
        if(!isLocalPlayer) return;
        MoveCamera(anchorPos,camPos,playerId == 0);
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
    public void CmdGetAccessibleHexesForUnitMovement()
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
    public void CmdResetAccessibleHexesTrigger()
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

    public void LateExitMovementSelection()
    {
        StartCoroutine(ExitMovementSelectionRoutine());
    }

    private IEnumerator ExitMovementSelectionRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        if(currentState == movementSelectionState) ChangeState(idleState);
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
        if(!unitToMove.hasBeenActivated) unitsToActivate--;
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
                var unitCurrentHex = unit.currentHex;
                
                unitCurrentHex.OnUnitExit(unit);
                
                unitCurrentHex.currentUnit = unitCurrentHex.previousUnit;
                unitCurrentHex.previousUnit = null;

                hex.DecreaseUnitMovement(unit);
                
                hex.previousUnit = hex.currentUnit;
                hex.currentUnit = unit;
                unit.currentHex = hex;
                unit.hexCol = hex.col;
                unit.hexRow = hex.row;
            
                hex.OnUnitEnter(unit);
            }
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;
            
            //TODO - Wait for animation to finish
            
            yield return new WaitForSeconds(0.5f);
            
        }
        
        unit.currentHex.currentUnit = unit;

        if (isServer)
        {
            unitMovementAnimationDone = true;
            if (unit.move == 0) unitMovementUnit.canMove = false;
            CallbackManager.UnitMove(unit,unit.currentHex);
        }
    }
    
    [Command]
    public void CmdResetMovementAnimationDoneTrigger()
    {
        unitMovementAnimationDone = false;
    }

    #endregion
    
    #region Unit Attack
    
    [Command]
    public void CmdResetAttackAnimationDoneTrigger()
    {
        unitAttackAnimationDone = false;
    }
    
    [Command]
    public void CmdSetAttackingUnits(Unit attacking,Unit attacked)
    {
        attackingUnit = attacking;
        attackedUnit = attacked;
    }
    
    [Command]
    public void CmdTryToResolveAttack()
    {
        isAskingForAttackResolve = true;
    }

    public void ServerAttackResolve(Unit attacking,Unit attacked)
    {
        if(!attacking.hasBeenActivated) unitsToActivate--;
        attacking.hasBeenActivated = true;
        attacking.canUseAbility = false;
        attacking.canMove = false;
        
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
    }
    
    #endregion
    
    #region Unit Ability Selection

    private void TryToUseAbility()
    {
        if (currentState == idleState || currentState == movementSelectionState)
        {
            ChangeState(abilitySelectionState);
        }
        
    }

    public void ExitAbilitySelection()
    {
        if(currentState != abilitySelectionState) return;
        ChangeState(idleState);
        if (selectedUnit == null) return;
        
        if (unitToRespawn != null)
        {
            unitToRespawn = null;
            CmdResetRespawnUnit();
            return;
        }
        
        if(selectedUnit.move > 0 || selectedUnit.attacksLeft > 0) CmdSendUnitClicked(selectedUnit);
    }

    public void TryToLaunchAbility()
    {
        if(currentState != abilitySelectionState) return;
        if(abilitySelectionState.selectionsLeft > 0 || abilitySelectionState.selectedHexes.Count == 0) return;
        ChangeState(unitInAnimationState);
        CmdTryToUseAbility();
    }

    [Command]
    public void CmdSelectHexForAbility(Hex hex)
    {
        if(!selectedHexesForAbility.Contains(hex)) selectedHexesForAbility.Add(hex);
    }
    
    [Command]
    public void CmdDeselectHexForAbility(Hex hex)
    {
        if(selectedHexesForAbility.Contains(hex)) selectedHexesForAbility.Remove(hex);
    }
    
    [Command]
    private void CmdTryToUseAbility()
    {
        isAskingForAbilityResolve = true;
    }

    public void DisplayAbilityButton(bool value,bool interactable = true)
    {
        uiManager.EnableAbilityButton(value,interactable);
    }

    public void DisplayAbilityConfirmPanel(bool value)
    {
        uiManager.EnableAbilitySelection(value);
    }
    
    [Command]
    public void CmdSetAbilityIndexToUse(Unit unit,byte index)
    {
        castingUnit = unit;
        abilityIndexToUse = index;
    }
    
    [Command]
    public void CmdGetAbilitySelectables(Unit respawningUnit)
    {
        Debug.Log("ASKING FOR ABILITY SELECTABLES");
        if (respawningUnit != null) unitToRespawn = respawningUnit;
        isAskingForAbilitySelectables = true;
        abilitySelectablesReceived = false;
    }
    
    [Command]
    public void CmdResetAbilitySelectableReceivedTrigger()
    {
        abilitySelectablesReceived = false;
    }
    
    public void SetAbilitySelectables(IEnumerable<Hex> hexes)
    {
        abilitySelectableHexes.Clear();
        
        foreach (var hex in hexes)
        {
            abilitySelectableHexes.Add(hex);
        }
        abilitySelectablesReceived = true;
        Debug.Log("Received Hexes");
    }

    #endregion
    
    #region Unit Ability Resolve
    
    [Command]
    public void CmdResetAbilityAnimationDoneTrigger()
    {
        unitAbilityAnimationDone = false;
    }
    
    public void ServerAbilityResolve(Unit casting,IEnumerable<Hex> targets)
    {
        if(!casting.hasBeenActivated) unitsToActivate--;
        casting.hasBeenActivated = true;
        casting.canUseAbility = false;
        casting.canMove = false;
        casting.canAttack = false;

        var ability = casting.abilityScriptable;
        if (unitToRespawn != null) ability = ObjectIDList.GetAbilityScriptable(1);
        StartCoroutine(PlayAbilityAnimationRoutine(casting,ability as IAbilityCallBacks,targets));
    }

    [ClientRpc]
    public void RpcAbilityResolve(Unit casting,Hex[] targets)
    {
        var ability = casting.abilityScriptable;
        if (unitToRespawn != null) ability = ObjectIDList.GetAbilityScriptable(1);
        StartCoroutine(PlayAbilityAnimationRoutine(casting,ability as IAbilityCallBacks,targets));
    }
    
    private IEnumerator PlayAbilityAnimationRoutine(Unit casting,IAbilityCallBacks ability,IEnumerable<Hex> targets)
    {
        Debug.Log("ABILITY ANIMATION ROUTINE");
        
        if(isServer) unitAbilityAnimationDone = false;
        
        yield return null;
        
        //TODO - Play Animation
        
        yield return new WaitForSeconds(0.5f);
        
        if (isServer)
        {
            var targetedHexes = targets as Hex[] ?? targets.ToArray();
            ability.OnAbilityTargetingHexes(casting,targetedHexes,this);
            castingUnit = null;
            selectedHexesForAbility.Clear();
            unitAbilityAnimationDone = true;
            if(ability is not RespawnUnit) CallbackManager.UnitAbilityCasted(casting,targetedHexes.ToArray());
        }
    }
    
    #endregion

    #region Faith Gestion

    public bool ConsumeFaith(int value)
    {
        value += faithModifier;
        if (value > faith) return false;
        faith -= value;
        return true;
    }
    

    #endregion

    #region Unit Respawning

    public void TryToRespawnUnit(Unit unit)
    {
        Debug.Log($"{this} is trying to respawn {unit}");
        unitToRespawn = unit;
        ChangeState(abilitySelectionState);
    }

    [Command]
    public void CmdResetRespawnUnit()
    {
        unitToRespawn = null;
    }

    #endregion

    private void TryToEndTurn()
    {
        if(currentState != idleState) return;
        turnIsOver = true;
        CmdTryEndTurnCommand();
    }
    
    [Command]
    private void CmdTryEndTurnCommand()
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
    public void CmdCheckVictoryConditions()
    {
        var gameSM = GameSM.instance;
        if(allUnits.Count <= 0) return;
        if (gameSM == null) return;
        if(gameSM.CheckIfPlayerWon()) gameSM.ChangeState(gameSM.endingState);
    }

    #region hooks

    [Command]
    public void CmdOnNothingClicked()
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
    public void CmdOnUnitClicked()
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
    public void CmdOnHexClicked()
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

    private void OnFaithValueChanged(int prevValue, int newValue)
    {
        UIUpdateFaithCount();
    }
    
    private void OnVictoryPointValueChanged(int prevValue, int newValue)
    {
        UIUpdateVictoryPointCount();
    }

    private void OnAccessibleHexesReceivedValueChange(bool prevValue,bool newValue)
    {
        
    } 
    
    private void OnAbilitySelectablesReceivedValueChange(bool prevValue,bool newValue)
    {
        
    }
    
    private void OnCanInputValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;
        if (newValue)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
            uiManager.AddButtonListeners(TryToEndTurn,TryToUseAbility,TryToLaunchAbility,ExitAbilitySelection,ToggleRespawnMenu);
            uiManager.UpdateActionsLeft(unitsToActivate);
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
            uiManager.RemoveButtonListeners(TryToEndTurn,TryToUseAbility,TryToLaunchAbility,ExitAbilitySelection,ToggleRespawnMenu);
        }
    }

    #endregion

    #region Update UI

    private void UIChangeDebugText()
    {
        uiManager.ChangeDebugText($"Player {playerId}, {currentState}");
    }
    
    [ClientRpc]
    public void DisplayIfItsYourTurn(int playerTurn)
    {
        // TODO - Replace with disable button and play animation
        
        uiManager.EnableNextTurnButton(playerId == playerTurn);
    }

    public void UIUpdateFaithCount()
    {
        uiManager.UpdateFaithCount(faith);
    }
    
    public void UIUpdateVictoryPointCount()
    {
        uiManager.UpdateVictoryPoint(victoryPoints);
    }

    public void UIUpdateAbilitySelectionLeft(int value,string moText)
    {
        uiManager.UpdateAbilitySelectionText($"{value} {moText}");
        
        uiManager.EnableAbilityConfirmButton(value == 0);
    }
    
    [ClientRpc]
    public void RpcUISetupUnitHuds()
    {
        uiManager.GenerateUnitHuds(allUnits);
        uiManager.InitializeRespawnButtons(allUnits,this);
        uiManager.RefreshUnitOutlines(allUnits,playerId);
    }
    
    [ClientRpc]
    public void RpcUIUpdateUnitHud()
    {
        uiManager.UpdateUnitHud();
        if(isLocalPlayer) uiManager.RefreshUnitOutlines(allUnits,playerId);
    }

    public void UIUpdateUnitHud()
    {
        uiManager.UpdateUnitHud();
    }

    public void ToggleRespawnMenu()
    {
        if(currentState == idleState) ChangeState(respawnUnitSelectionState);
        else if(currentState == respawnUnitSelectionState) ChangeState(idleState);
    }

    public void ActivateRespawnMenu(bool value)
    {
        if(value) uiManager.ActivateRespawnButtons(playerId,faith >= 8 - faithModifier);
        uiManager.SetActiveRespawnMenu(value);
    }

    #endregion

    [ClientRpc]
    public void RpcOnEndGame(int winner)
    {
        if(!isLocalPlayer) return;
        var moreText = winner == playerId ? "It's you !" : "It's not you !";
        Debug.Log($"Player {winner} won ! {moreText}");
        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
