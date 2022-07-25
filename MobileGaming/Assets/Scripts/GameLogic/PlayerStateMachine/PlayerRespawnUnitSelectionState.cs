using System.Collections;
using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class PlayerRespawnUnitSelectionState : BasePlayerState
{
    public PlayerRespawnUnitSelectionState(PlayerSM stateMachine) : base(stateMachine)
    {
        sm = stateMachine;
    }

    public override void Enter()
    {
        sm.ActivateRespawnMenu(true);
    }

    public override void Exit()
    {
        sm.ActivateRespawnMenu(false);
    }
}
