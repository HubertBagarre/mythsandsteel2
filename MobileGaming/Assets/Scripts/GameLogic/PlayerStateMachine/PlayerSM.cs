using System.Collections;
using Mirror;
using NaughtyAttributes;
using UnityEngine;
using PlayerStates;
using TMPro;
using UnityEngine.UI;

public class PlayerSM : StateMachine
{
    public PlayerIdleState idleState;
    public PlayerMovementSelection movementSelectionState;
    public PlayerAbilitySelection abilitySelectionState;
    public PlayerInactiveState inactiveState;
    public PlayerInAnimationState animationState;

    [Header("Network")] [SyncVar] public int playerId;
    [SyncVar(hook = nameof(OnCanInputValueChanged))] public bool canSendInfo;
    
    [Header("Game Information")]
    public readonly SyncHashSet<Unit> allUnits = new ();
    public readonly SyncHashSet<Hex> allHexes = new ();

    [Header("Unit Movement")]
    [SyncVar] public bool isAskingForUnitMovement;
    [SyncVar] public bool accessibleHexesReceived;
    public readonly SyncHashSet<Hex> accessibleHexes = new();


    [Header("User Interface")] 
    [SerializeField] private TextMeshProUGUI actionsLeftText;
    
    [SerializeField] private Color allyOutlineColor;
    [SerializeField] private Color enemyOutlineColor;
    
    [Header("Selection")]
    [SyncVar,ReadOnly] public Unit selectedUnit;
    [SyncVar,ReadOnly] public Hex selectedHex;

    [Header("Inputs")] private PlayerInputManager inputManager;

    [SerializeField] private Button nextPhaseButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button abilityButton;

    [SerializeField] private LayerMask layersToHit;
    [SyncVar,ReadOnly] public bool clickedUnit;
    [SyncVar,ReadOnly] public bool clickedHex;
    private Camera cam;

    [Header("Debug")] public TextMeshProUGUI debugText;
    
    [Header("Actions")]
    [SyncVar] public int maxActions;
    [SyncVar(hook = nameof(OnActionsLeftValueChanged))] public int actionsLeft;
    
    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        movementSelectionState = new PlayerMovementSelection(this);
        abilitySelectionState = new PlayerAbilitySelection(this);
        inactiveState = new PlayerInactiveState(this);
        animationState = new PlayerInAnimationState(this);
        
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
        
        clickedUnit = false;
        clickedHex = false;

        RefreshUnitOutlines();
        
        return idleState;
    }
    
    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        Debug.Log($"Entering {currentState}");
        debugText.text = $"Player {playerId}, {currentState}";
    }

    private void TryToSelectUnitOrTile(Vector2 screenPosition,float time)
    {
        var ray = cam.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out var hit,layersToHit)) return;
        
        var objectHit = hit.transform;
        
        Debug.Log(objectHit);
        
        SendHitInfo(objectHit);
    }
    
    [Command]
    private void SendHitInfo(Transform t)
    {
        selectedHex = t.GetComponent<Hex>();
        clickedHex = selectedHex;
        if (clickedHex)
        {
            if (selectedHex.currentUnit != null)
            {
                selectedUnit = selectedHex.currentUnit;
                clickedHex = false;
                clickedUnit = true;
                return;
            }
        }
        
        selectedUnit = t.GetComponent<Unit>();
        clickedUnit = selectedUnit;
    }

    
    
    public void SetUnitsAndHexesArrays(Unit[] units, Hex[] hexes)
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
        Debug.Log($"Set Lists, they have {allUnits.Count} and {allHexes.Count} elements");
    }
    
    public void RefreshUnitOutlines()
    {
        if(!isLocalPlayer) return;
        
        Debug.Log($"Refreshing Outline of {allUnits.Count} units");
        
        foreach (var unit in allUnits)
        {
            unit.outlineScript.OutlineColor = unit.playerId == playerId ? allyOutlineColor : enemyOutlineColor;
        }
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
        Debug.Log("Moving Camera");
        MoveCamera(anchorPos,camPos);
    }

    #endregion
    
    #region Unit Movement
    
    [Command]
    public void GetPathForUnit()
    {
        isAskingForUnitMovement = true;
    }

    public void ColorAccessibleHexes()
    {
        accessibleHexesReceived = true;
        RpcColorAccessibleHexes();
    }
    
    [ClientRpc]
    private void RpcColorAccessibleHexes()
    {
        if(!isLocalPlayer) return;
        foreach (var hex in allHexes)
        {
            var state = accessibleHexes.Contains(hex) ? Hex.HexColors.Selectable : Hex.HexColors.Unselectable;
            hex.ChangeHexColor(state);
        }
    }
    
    
    

    [Command]
    public void TryToMoveUnit(Unit unitToMove,Hex[] path)
    {
        if(!canSendInfo || unitToMove.playerId != playerId) return;
        if(!unitToMove.hasBeenActivated) actionsLeft--;
        unitToMove.hasBeenActivated = true;
        ServerMoveUnit(unitToMove,path);
        RpcMoveUnit(unitToMove,path);
    }

    private void ServerMoveUnit(Unit unitToMove, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unitToMove, path,true));
    }
    
    [ClientRpc]
    private void RpcMoveUnit(Unit unitToMove, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unitToMove, path,false));
    }
    
    private IEnumerator MoveUnitRoutine(Unit unit, Hex[] path,bool decreaseUnitMovement)
    {
        foreach (var hex in path)
        {
            unit.currentHex.OnUnitExit(unit);
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;
            
            hex.OnUnitEnter(unit);
            
            if(decreaseUnitMovement) hex.DecreaseUnitMovement(unit);
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion
    
    [Command]
    public void TryToEndTurn()
    {
        GameSM.instance.playerTurnOver = true;
    }

    public void EndTurn()
    {
        ChangeState(idleState);
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
        
        var textToDisplay = (playerId == playerTurn) ? actionsLeft.ToString() : "E";
        actionsLeftText.text = $"{textToDisplay}";
    }

    #region hooks

    private void OnActionsLeftValueChanged(int prevValue, int newValue)
    {
        actionsLeftText.text = newValue.ToString();
    }
    
    private void OnCanInputValueChanged(bool prevValue,bool newValue)
    {
        if(!isLocalPlayer) return;
        if (newValue)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
            nextPhaseButton.onClick.AddListener(TryToEndTurn);
            attackButton.onClick.AddListener(TryToAttack);
            abilityButton.onClick.AddListener(TryToUseAbility);
            
            Debug.Log("You Can Send");
            
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
            nextPhaseButton.onClick.RemoveListener(TryToEndTurn);
            attackButton.onClick.RemoveListener(TryToAttack);
            abilityButton.onClick.RemoveListener(TryToUseAbility);
            
            Debug.Log("You Can't Send");
        }
        RefreshUnitOutlines();
    }

    #endregion
    
    
}
