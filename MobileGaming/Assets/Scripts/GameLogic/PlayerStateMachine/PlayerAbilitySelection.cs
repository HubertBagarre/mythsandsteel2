using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerStates
{
    public class PlayerAbilitySelection : BasePlayerState
    {
        private Unit abilityCastingUnit;
        private bool receivedSelectablesForAbilityTrigger;
        public int selectionsLeft;
        public readonly List<Hex> selectedHexes = new ();

        private ScriptableAbility scriptableAbility;
        private IAbilityCallBacks scriptableAbilityCallbacks;

        public PlayerAbilitySelection(PlayerSM stateMachine) : base(stateMachine)
        {
            sm = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            selectedHexes.Clear();

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

            sm.castingUnit = abilityCastingUnit;
            sm.abilityIndexToUse = abilityCastingUnit.abilityScriptableId;        
            sm.CmdSetAbilityIndexToUse(abilityCastingUnit,abilityCastingUnit.abilityScriptableId);
            scriptableAbility = ObjectIDList.instance.abilities[sm.abilityIndexToUse];
            
            if(scriptableAbility is not IAbilityCallBacks casted)
            {
                Debug.LogWarning("SELECTED UNIT ABILITY AS NO INTERFACE, RETURNING TO IDLE");
                sm.ChangeState(sm.idleState);
                return;
            }
            
            scriptableAbilityCallbacks = casted;
            
            selectionsLeft = scriptableAbility.abilityTargetCount;

            sm.isAskingForAbilitySelectables = true;

            sm.DisplayAbilityConfirmPanel(true);
            
            receivedSelectablesForAbilityTrigger = false;
            
            foreach (var hex in sm.allHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Unselectable);
            }

            ClientSideSetAbilitySelectable(abilityCastingUnit);
            
            sm.CmdGetAbilitySelectables();
        }

        private void ClientSideSetAbilitySelectable(Unit castingUnit)
        {
            var selectableHexes = scriptableAbilityCallbacks.AbilitySelectables(castingUnit);
            
            sm.entitiesToSelect = scriptableAbility.abilityTargetCount;
            UpdateAbilitySelectionText();
            
            foreach (var hex in selectableHexes)
            {
                hex.ChangeHexColor(Hex.HexColors.Selectable);
                if(selectedHexes.Contains(hex)) hex.ChangeHexColor(Hex.HexColors.Selected);
            }
        }
        
        public override void UpdateLogic()
        {
            if (!sm.abilitySelectablesReceived) return;
            if(!receivedSelectablesForAbilityTrigger) OnAbilitySelectablesReceived();
            base.UpdateLogic();
        }

        private void OnAbilitySelectablesReceived()
        {
            receivedSelectablesForAbilityTrigger = true;

            selectionsLeft = sm.entitiesToSelect;

            UpdateAbilitySelectionText();
            
            foreach (var hex in sm.abilitySelectableHexes)                                             
            {                                                                                
                hex.ChangeHexColor(Hex.HexColors.Selectable);                                  
            }                     
        }

        private void UpdateAbilitySelectionText()
        {
            var moText = scriptableAbility.abilityTargetHexes ? "Tile" : "Unit";
            if (selectionsLeft > 1) moText += "s";
            sm.UIUpdateAbilitySelectionLeft(selectionsLeft,moText);
        }

        private void SelectHexForAbility(Hex hex)
        {
            if(selectionsLeft <= 0) return;
            
            selectionsLeft--;
            
            selectedHexes.Add(hex);

            sm.CmdSelectHexForAbility(hex);
            
            hex.ChangeHexColor(Hex.HexColors.Selected);
            
            UpdateAbilitySelectionText();
        }
        
        private void DeselectHexForAbility(Hex hex)
        {
            if(!selectedHexes.Contains(hex)) return;

            selectionsLeft++;
            
            selectedHexes.Remove(hex);

            sm.CmdDeselectHexForAbility(hex);
            
            ClientSideSetAbilitySelectable(abilityCastingUnit);
        }
        
        private void TryToSelectHex(Hex hex)
        {
            if (sm.abilitySelectableHexes.Contains(hex))
            {
                if (!selectedHexes.Contains(hex))
                {
                    SelectHexForAbility(hex);
                    return;
                }
                
                DeselectHexForAbility(hex);
                return;
            }
            
            sm.ExitAbilitySelection();
        }
        
        protected override void OnHexClicked()
        {
            base.OnHexClicked();
            TryToSelectHex(sm.selectedHex);
        }

        protected override void OnUnitClicked()
        {
            base.OnUnitClicked();
            if(sm.selectedUnit.currentHex != null) TryToSelectHex(sm.selectedUnit.currentHex);
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

