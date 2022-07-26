using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    
    [Header("Unit Portrait")]
    [SerializeField] private Button allyUnitPortraitButton;
    [SerializeField] private Transform allyUnitPortraitParent;
    [SerializeField] private Image allyUnitPortraitImage;
    [SerializeField] private TextMeshProUGUI allyUnitPortraitTextTop;
    [SerializeField] private TextMeshProUGUI allyUnitPortraitTextBot;

    [Header("GameObjects")]
    [SerializeField] private UnitHud unitHudPrefab;
    [SerializeField] private Transform unitHudParent;
    [SerializeField] private GameObject abilityGameObject;
    [SerializeField] private GameObject abilitySelectionGameObject;

    [Header("Unit Respawn")]
    [SerializeField] private GameObject unitRespawnMenuGameObject;
    [SerializeField] private Transform unitRespawnParent;
    [SerializeField] private RespawnUnitButton respawnUnitButtonPrefab;
    private readonly List<RespawnUnitButton> respawnUnitButtons = new ();

    [Header("Other")]
    [SerializeField] private Color allyOutlineColor;
    [SerializeField] private Color enemyOutlineColor;

    [Header("Unit Hud")]
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

    public void AddButtonListeners(UnityAction nextTurnAction,UnityAction abilityAction,UnityAction confirmAbilityAction,UnityAction cancelAbilityAction,UnityAction faithButtonAction)
    {
        nextTurnButton.onClick.AddListener(nextTurnAction);
        abilityButton.onClick.AddListener(abilityAction);
        abilityConfirmButton.onClick.AddListener(confirmAbilityAction);
        abilityCancelButton.onClick.AddListener(cancelAbilityAction);
        faithButton.onClick.AddListener(faithButtonAction);
    }

    public void RemoveButtonListeners(UnityAction nextTurnAction,UnityAction abilityAction,UnityAction confirmAbilityAction,UnityAction cancelAbilityAction,UnityAction faithButtonAction)
    {
        nextTurnButton.onClick.RemoveListener(nextTurnAction);
        abilityButton.onClick.RemoveListener(abilityAction);
        abilityConfirmButton.onClick.RemoveListener(confirmAbilityAction);
        abilityCancelButton.onClick.RemoveListener(cancelAbilityAction);
        faithButton.onClick.RemoveListener(faithButtonAction);
    }
    
    public void UpdateAbilitySelectionText(string moText)
    {
        abilitySelectionText.text = $"Select {moText}";
    }

    public void EnableAbilityConfirmButton(bool value)
    {
        abilityConfirmButton.interactable = value;
    }
    
    public void SetActiveRespawnMenu(bool value)
    {
        unitRespawnMenuGameObject.SetActive(value);
    }

    public void InitializeRespawnButtons(IEnumerable<Unit> units,PlayerSM player)
    {
        respawnUnitButtons.Clear();
        foreach (var unit in units)
        {
            var respawnUnitButton = Instantiate(respawnUnitButtonPrefab, unitRespawnParent);
            respawnUnitButton.associatedUnit = unit;
            respawnUnitButton.gameObject.SetActive(false);
            respawnUnitButton.associatedPlayer = player;
            respawnUnitButtons.Add(respawnUnitButton);
        }
    }

    public void ActivateRespawnButtons(int playerId,bool interactable)
    {
        foreach (var button in respawnUnitButtons)
        {
            button.gameObject.SetActive(button.associatedUnit.isDead && button.associatedUnit.playerId == playerId);
            button.button.interactable = interactable;
        }
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
            UnitHud unitHud = Instantiate(unitHudPrefab,unitHudParent);
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

    #region Unit Portrait

    public void UpdateUnitPortrait(Unit unit)
    {
        allyUnitPortraitImage.sprite = ObjectIDList.GetUnitScriptable(unit.unitScriptableId).portraitImage;
        allyUnitPortraitTextTop.text =
            $"{unit.currentHp}<sprite=8>\n{unit.physicDef}<sprite=24>\n{unit.magicDef}<sprite=1>";
        allyUnitPortraitTextBot.text =
            $"{unit.attacksLeft}<sprite=39>\n{unit.attackDamage}<sprite=21>\n{unit.move}<sprite=43>";
    }

    #endregion
    
}
