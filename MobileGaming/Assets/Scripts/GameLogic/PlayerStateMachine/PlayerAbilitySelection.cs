using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerAbilitySelection : BasePlayerState
    {
        private Unit abilityCastingUnit;
        private bool receivedSelectablesForAbility;
        
        public PlayerAbilitySelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();

            abilityCastingUnit = sm.selectedUnit;
                
            if (abilityCastingUnit == null)
            {
                Debug.LogWarning("NO UNIT SELECTED, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }

            if(!abilityCastingUnit.hasAbility)
            {
                Debug.LogWarning("SELECTED UNIT HAS NO ABILITY, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }
            
            sm.isAskingForAbilitySelectionWithHexes = abilityCastingUnit.abilityTargetHexes;
            sm.isAskingForAbilitySelectionWithUnits = !abilityCastingUnit.abilityTargetHexes;
            
            
            sm.DisplayAbilityConfirmPanel(true);

            receivedSelectablesForAbility = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAbilitySelectable(abilityCastingUnit);
            
            sm.GetAccessibleHexesForUnitMovement();
        }

        private void ClientSideSetAbilitySelectable(Unit unitToGetAbilitySelectables)
        {
            //fonction on ScriptableAbility, put everything ability related in scriptableAbility, add scriptableAbilities to objectsID
            
            var enemyUnits = sm.allUnits.Where(unit => unit.playerId != unitToGetAbilitySelectables.playerId);
            
        }

        public override void Exit()
        {
            base.Exit();
            
            sm.abilitySelectablesReceived = false;
            sm.CmdResetAccessibleHexesTrigger();
            
            sm.DisplayAbilityConfirmPanel(false);
            sm.DisplayAbilityButton(false);
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }
}

