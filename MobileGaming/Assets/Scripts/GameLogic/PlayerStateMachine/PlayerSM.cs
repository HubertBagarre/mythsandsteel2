using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerStates;
using TMPro;

public class PlayerSM : StateMachine
{
    [HideInInspector] public PlayerIdleState idleState;
    [HideInInspector] public PlayerMovementSelection movementSelectionState;
    [HideInInspector] public PlayerAbilitySelection abilitySelectionState;

    [Header("Managers")] [SerializeField] private HexGrid hexGrid;
    
    [Header("Inputs")]
    private PlayerInputManager inputManager;
    [SerializeField] private LayerMask hexLayer;
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
        return idleState;
    }

    public override void ChangeState(BaseState newState)
    {
        base.ChangeState(newState);
        debugText.text = currentState.ToString();
    }
}
