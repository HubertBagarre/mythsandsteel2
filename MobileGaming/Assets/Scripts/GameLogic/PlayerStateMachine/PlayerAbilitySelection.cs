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
        private int selectionsLeft;
        private ScriptableAbility scriptableAbility;
        private IAbilityCallBacks scriptableAbilityCallbacks;

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
            
            sm.abilityIndexToUse = abilityCastingUnit.abilityScriptableId;        
            sm.CmdSetAbilityIndexToUse(abilityCastingUnit.abilityScriptableId);
            scriptableAbility = ObjectIDList.instance.abilities[sm.abilityIndexToUse];
            
            if(scriptableAbility is not IAbilityCallBacks casted)
            {
                Debug.LogWarning("SELECTED UNIT ABILITY AS NO INTERFACE, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }
            
            scriptableAbilityCallbacks = casted;
            
            selectionsLeft = scriptableAbility.abilityTargetCount;
            
            sm.isAskingForAbilitySelectionWithHexes = scriptableAbility.abilityTargetHexes;
            sm.isAskingForAbilitySelectionWithUnits = !scriptableAbility.abilityTargetHexes;
            
            sm.DisplayAbilityConfirmPanel(true);

            receivedSelectablesForAbility = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAbilitySelectable(abilityCastingUnit);
            
            sm.CmdGetAbilitySelectables(scriptableAbility.abilityTargetHexes);
        }

        private void ClientSideSetAbilitySelectable(Unit castingUnit)
        {
            var selectableHexes = scriptableAbilityCallbacks.AbilitySelectables(castingUnit);
            
            foreach (var hex in selectableHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Selectable);
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            sm.abilitySelectablesReceived = false;
            sm.CmdResetAbilitySelectableReceivedTrigger();
            
            sm.DisplayAbilityConfirmPanel(false);
            sm.DisplayAbilityButton(false);
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Normal);
            }
        }
    }
}

