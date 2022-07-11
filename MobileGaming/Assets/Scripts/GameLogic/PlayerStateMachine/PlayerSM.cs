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

    [Header("Network")] [SyncVar] public int playerId;
    [SyncVar(hook = nameof(OnCanInputValueChanged))] public bool canSendInfo;

    [Header("Managers")]
    public HexGrid hexGrid;

    [Header("User Interface")] 
    [SerializeField] private TextMeshProUGUI actionsLeftText;
    
    [SerializeField] private Color allyOutlineColor;
    [SerializeField] private Color enemyOutlineColor;
    
    [Header("Selection")] [ReadOnly] public Unit selectedUnit;
    [ReadOnly] public Hex selectedHex;

    [Header("Inputs")] private PlayerInputManager inputManager;

    [SerializeField] private Button nextPhaseButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button abilityButton;

    [SerializeField] private LayerMask layersToHit;
    [ReadOnly] public bool clickedUnit;
    [ReadOnly] public bool clickedHex;
    private Camera cam;

    [Header("Debug")] public TextMeshProUGUI debugText;
    
    [Header("Actions")]
    [SyncVar] public int maxActions;
    [SyncVar(hook = nameof(OnActionsLeftValueChanged))] public int actionsLeft;
    


    [Header("Movement")]
    public bool isMovingUnit;

    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        movementSelectionState = new PlayerMovementSelection(this);
        abilitySelectionState = new PlayerAbilitySelection(this);
        inactiveState = new PlayerInactiveState(this);
        
        inputManager = PlayerInputManager.instance;
        cam = Camera.main;
    }

    protected override BaseState GetInitialState()
    {
        if (!isLocalPlayer)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            return inactiveState;
        }
        
        ResetInstances();
        
        clickedUnit = false;
        clickedHex = false;

        RefreshUnitOutlines();
        
        return idleState;
    }

    public void ResetInstances()
    {
        hexGrid = HexGrid.instance;
    }

    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        debugText.text = $"Player {playerId}, {currentState}";
    }

    private void TryToSelectUnitOrTile(Vector2 screenPosition,float time)
    {
        var ray = cam.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out var hit,layersToHit)) return;
        
        var objectHit = hit.transform;
        selectedUnit = objectHit.GetComponent<Unit>();
        selectedHex = objectHit.GetComponent<Hex>();
        clickedUnit = selectedUnit;
        clickedHex = selectedHex;
    }

    public void RefreshUnitOutlines()
    {
        //StartCoroutine(OutlineUnits());
    }
    
    private IEnumerator OutlineUnits()
    {
        if(!isLocalPlayer) yield break;

        yield return new WaitUntil(() => hexGrid.isDoneLoadingMap);
        
        foreach (var unit in hexGrid.units)
        {
            unit.outlineScript.OutlineColor = unit.playerId == playerId ? allyOutlineColor : enemyOutlineColor;
        }
    }
    
    #region UnitMovement

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
        isMovingUnit = true;
        foreach (var hex in path)
        {
            unit.currentHex.OnUnitExit(unit);
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;
            
            hex.OnUnitEnter(unit);
            
            if(decreaseUnitMovement) hex.DecreaseUnitMovement(unit);
            
            yield return new WaitForSeconds(0.5f);
        }

        isMovingUnit = false;
    }

    #endregion
    
    [Command]
    public void TryToEndTurn()
    {
        GameSM.instance.playerTurnOver = true;
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
    }

    #endregion
    
    
}
