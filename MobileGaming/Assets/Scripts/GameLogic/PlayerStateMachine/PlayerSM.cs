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

    [Header("Network")]
    [SyncVar] public int playerId;
    [SyncVar(hook = nameof(CanInput))] public bool canSendInfo;
    
    [Header("Managers")]
    public HexGrid hexGrid;
    public GameSM gameManager;

    [Header("Selection")]
    [ReadOnly] public Unit selectedUnit;
    [ReadOnly] public Hex selectedHex;
    
    [Header("Inputs")]
    private PlayerInputManager inputManager;

    [SerializeField] private Button nextPhaseButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button abilityButton;
    
    [SerializeField] private LayerMask layersToHit;
    [ReadOnly] public bool clickedUnit;
    [ReadOnly] public bool clickedHex;
    private Camera cam;
    
    [Header("Debug")]
    public TextMeshProUGUI debugText;

    private bool isMovingUnit;

    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        movementSelectionState = new PlayerMovementSelection(this);
        abilitySelectionState = new PlayerAbilitySelection(this);
        
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
        
        return idleState;
    }

    public void ResetInstances()
    {
        hexGrid = HexGrid.instance;
        if(isServer) gameManager = GameSM.instance;
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
    
    #region UnitMovement

    [Command]
    public void TryToMoveUnit(Unit unitToMove,Hex[] path)
    {
        if(!canSendInfo || unitToMove.playerId != playerId) return;
        ServerMoveUnit(unitToMove,path);
        RpcMoveUnit(unitToMove,path);
    }

    private void ServerMoveUnit(Unit unitToMove, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unitToMove, path));
    }
    
    [ClientRpc]
    private void RpcMoveUnit(Unit unitToMove, Hex[] path)
    {
        StartCoroutine(MoveUnitRoutine(unitToMove, path));
    }
    
    private IEnumerator MoveUnitRoutine(Unit unit, Hex[] path)
    {
        isMovingUnit = true;
        foreach (var hex in path)
        {
            unit.currentHex.OnUnitExit(unit);
            
            unit.transform.position = hex.transform.position + Vector3.up * 2f;
            
            hex.OnUnitEnter(unit);
            
            yield return new WaitForSeconds(0.5f);
        }

        isMovingUnit = false;
    }

    #endregion
    
    public void TryToEnterNextPhase()
    {
        Debug.Log("Next Phase");
    }

    public void TryToAttack()
    {
        Debug.Log("Attack");
    }

    public void TryToUseAbility()
    {
        Debug.Log("Ability");
    }
    
    
    public void CanInput(bool prevValue,bool newValue)
    {
        if (newValue)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
            nextPhaseButton.onClick.AddListener(TryToEnterNextPhase);
            attackButton.onClick.AddListener(TryToAttack);
            abilityButton.onClick.AddListener(TryToUseAbility);
            
            Debug.Log("You Can Send");
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
            nextPhaseButton.onClick.RemoveListener(TryToEnterNextPhase);
            attackButton.onClick.RemoveListener(TryToAttack);
            abilityButton.onClick.RemoveListener(TryToUseAbility);
            
            Debug.Log("You Can't Send");
        }
    }
}
