using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] private Transform canvas;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI actionsLeftText;
    [SerializeField] private TextMeshProUGUI faithCountText;
    [SerializeField] private TextMeshProUGUI victoryPointText;
    [SerializeField] private TextMeshProUGUI abilitySelectionText;
    
    [Header("Buttons")]
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button abilityConfirmButton;
    [SerializeField] private Button abilityCancelButton;
    [SerializeField] private Button faithButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button allyUnitPortraitButton;

    [Header("GameObjects")]
    [SerializeField] private UnitHud unitHudPrefab;
    [SerializeField] private GameObject abilityGameObject;
    [SerializeField] private GameObject abilitySelectionGameObject;
    [SerializeField] private GameObject allyUnitPortraitGameObject;
    
    [Header("Other")]
    [SerializeField] private Color allyOutlineColor;
    [SerializeField] private Color enemyOutlineColor;

    [Header("Moving Things")]
    [SerializeField] private Vector2 unitHudOffset;
    private Dictionary<Unit, UnitHud> unitHudDict = new();

    public static PlayerUIManager instance;

    public void Awake()
    {
        if (isLocalPlayer) instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void ChangeDebugText(string text)
    {
        debugText.text = text;
    }
    
    public void UpdateActionsLeft(int actionsLeft)
    {
        actionsLeftText.text = actionsLeft.ToString();
    }
    
    public void UpdateFaithCount(int faithCount)
    {
        faithCountText.text = faithCount.ToString();
    }
    
    public void UpdateVictoryPoint(int victoryPoint)
    {
        victoryPointText.text = victoryPoint.ToString();
    }

    public void EnableNextTurnButton(bool value)
    {
        nextTurnButton.interactable = value;
        if (!value) actionsLeftText.text = string.Empty;
    }

    public void EnableAbilityButton(bool value,bool interactable)
    {
        abilityGameObject.SetActive(value);
        if (value) abilityButton.interactable = interactable;
    }

    public void EnableAbilitySelection(bool value)
    {
        abilitySelectionGameObject.SetActive(value);
        abilityGameObject.SetActive(!value);
    }

    public void AddButtonListeners(UnityAction nextTurnAction,UnityAction abilityAction,UnityAction confirmAbilityAction,UnityAction cancelAbilityAction)
    {
        nextTurnButton.onClick.AddListener(nextTurnAction);
        abilityButton.onClick.AddListener(abilityAction);
        abilityConfirmButton.onClick.AddListener(confirmAbilityAction);
        abilityCancelButton.onClick.AddListener(cancelAbilityAction);
    }

    public void RemoveButtonListeners(UnityAction nextTurnAction,UnityAction abilityAction,UnityAction confirmAbilityAction,UnityAction cancelAbilityAction)
    {
        nextTurnButton.onClick.RemoveListener(nextTurnAction);
        abilityButton.onClick.RemoveListener(abilityAction);
        abilityConfirmButton.onClick.RemoveListener(confirmAbilityAction);
        abilityCancelButton.onClick.RemoveListener(cancelAbilityAction);
    }
    
    public void UpdateAbilitySelectionText(string moText)
    {
        abilitySelectionText.text = $"Select {moText}";
    }

    public void EnableAbilityConfirmButton(bool value)
    {
        abilityConfirmButton.interactable = value;
    }
    
    #region Unit Hud

    public void RefreshUnitOutlines(IEnumerable<Unit> allUnits,int playerId)
    {
        foreach (var unit in allUnits)
        {
            unit.outlineScript.OutlineColor = unit.playerId == playerId ? allyOutlineColor : enemyOutlineColor;
        }
    }
    
    public void GenerateUnitHuds(IEnumerable<Unit> units)
    {
        foreach (var unit in units)
        {
            UnitHud unitHud = Instantiate(unitHudPrefab,canvas);
            unitHud.SetVariables(unitHudOffset,unit,Camera.main);
            unitHud.UpdateHud();
            unitHudDict.Add(unit,unitHud);
        }
    }

    public void UpdateUnitHud()
    {
        foreach (var unitHud in unitHudDict.Values)
        {
            unitHud.UpdateHud();
        }
    }

    #endregion
}
