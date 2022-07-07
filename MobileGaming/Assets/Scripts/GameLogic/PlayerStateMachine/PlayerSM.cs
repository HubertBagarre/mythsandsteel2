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
    

    [Header("Managers")]
    public HexGrid hexGrid;
    public GameSM gameManager;
    public NetworkIdentity identity;
    
    [Header("Selection")]
    [ReadOnly]public Unit selectedUnit;
    [ReadOnly]public Hex selectedHex;
    
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
            CanInput(false);
            return inactiveState;
        }
        
        ResetInstances();
        
        clickedUnit = false;
        clickedHex = false;
        
        CanInput(true);
        return idleState;
    }

    public void ResetInstances()
    {
        hexGrid = HexGrid.instance;
        gameManager = GameSM.instance;
        identity = GetComponent<NetworkIdentity>();
    }

    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        debugText.text = currentState.ToString();
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
    
    [Command]
    public void TryToMoveUnit(Unit unitToMove,Hex[] path)
    {
        Debug.Log($"Player is trying to move {unitToMove}, in position {unitToMove.hexGridIndex} ");
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
    
    public void CanInput(bool value)
    {
        if (value)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
            nextPhaseButton.onClick.AddListener(TryToEnterNextPhase);
            attackButton.onClick.AddListener(TryToAttack);
            abilityButton.onClick.AddListener(TryToUseAbility);
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
            nextPhaseButton.onClick.RemoveListener(TryToEnterNextPhase);
            attackButton.onClick.RemoveListener(TryToAttack);
            abilityButton.onClick.RemoveListener(TryToUseAbility);
        }
    }
}
