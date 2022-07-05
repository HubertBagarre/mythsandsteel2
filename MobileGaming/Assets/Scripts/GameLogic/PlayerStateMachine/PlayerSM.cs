using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using PlayerStates;
using TMPro;

public class PlayerSM : StateMachine
{
    [HideInInspector] public PlayerIdleState idleState;
    [HideInInspector] public PlayerMovementSelection movementSelectionState;
    [HideInInspector] public PlayerAbilitySelection abilitySelectionState;

    [Header("Managers")] [SerializeField] private HexGrid hexGrid;

    [Header("Selection")]
    [ReadOnly]public Unit selectedUnit;
    [ReadOnly]public Hex selectedHex;
    
    [Header("Inputs")]
    private PlayerInputManager inputManager;
    [SerializeField] private LayerMask layersToHit;
    [ReadOnly] public bool clickedUnit;
    [ReadOnly] public bool clickedHex;
    private Camera cam;
    
    [Header("Debug")]
    public TextMeshProUGUI debugText;

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
        clickedUnit = false;
        clickedHex = false;
        CanInput(true);
        return idleState;
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

    public void CanInput(bool value)
    {
        if (value)
        {
            inputManager.OnStartTouch += TryToSelectUnitOrTile;
        }
        else
        {
            inputManager.OnStartTouch -= TryToSelectUnitOrTile;
        }
    }
}
