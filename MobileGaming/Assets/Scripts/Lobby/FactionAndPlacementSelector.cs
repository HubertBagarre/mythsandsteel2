using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionAndPlacementSelector : MonoBehaviour
{
    [Header("Faction Buttons")]
    [SerializeField] private Transform factionLayoutTransform;
    [SerializeField] private FactionButton factionButtonPrefab;
    [SerializeField] private TextMeshProUGUI factionTitleText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;
    
    [Header("Unit Placement Buttons")]
    [SerializeField] private Transform unitPlacementLayoutTransform;
    [SerializeField] private UnitPlacementButton unitPlacementButtonPrefab;
    [SerializeField] private Image unitPlacementImagePreview;
    
    
    private readonly List<UnitPlacementButton> unitPlacementButtons = new ();
    private readonly List<FactionButton> factionButtons = new ();

    private void Start()
    {
        InstantiateButtons();

        factionButtons[0].OnClick();
        unitPlacementButtons[0].OnClick();
    }

    private void InstantiateButtons()
    {
        for (var i = 0; i < ObjectIDList.instance.factions.Count; i++)
        {
            var item = Instantiate(factionButtonPrefab, factionLayoutTransform);
            item.factionIndex = i;
            item.selector = this;
            item.button.onClick.AddListener(EnableAllFactionButtons);
            factionButtons.Add(item);
        }
        
        for (var i = 0; i < ObjectIDList.instance.unitPlacements.Count; i++)
        {
            var item = Instantiate(unitPlacementButtonPrefab, unitPlacementLayoutTransform);
            item.unitPlacementIndex = i;
            item.selector = this;
            item.button.onClick.AddListener(EnableAllUnitPlacementButtons);
            unitPlacementButtons.Add(item);
        }
    }

    private void EnableAllFactionButtons()
    {
        foreach (var factionButton in factionButtons)
        {
            factionButton.button.interactable = true;
        }
    }
    
    private void EnableAllUnitPlacementButtons()
    {
        foreach (var unitPlacementButton in unitPlacementButtons)
        {
            unitPlacementButton.button.interactable = true;
        }
    }

    public void SelectFaction(int index)
    {
        var faction = ObjectIDList.GetFactionScriptable(index);
        factionTitleText.text = faction.name;

        factionDescriptionText.text =
            $"Belief Ability: \n{faction.beliefAbilityDescription}\n \nCultural Ability: \n{faction.beliefAbilityDescription}";
    }
    
    public void SelectUnitPlacement(int index)
    {
        var placement = ObjectIDList.GetUnitPlacementScriptable(index);

        unitPlacementImagePreview.sprite = placement.placementPreview;
    }
}
