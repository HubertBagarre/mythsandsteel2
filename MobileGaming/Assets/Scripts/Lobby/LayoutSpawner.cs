using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayoutSpawner : MonoBehaviour
{
    [SerializeField] private NetworkCanvasHUDRoomScene hudManager;
    
    [SerializeField] private Transform factionLayoutTransform,unitPlacementLayoutTransform,unitFormationDetailLayoutTransform;
    [SerializeField] private FactionButton factionButtonPrefab;
    [SerializeField] private UnitPlacementButton unitPlacementButtonPrefab;
    [SerializeField] private UnitInfoButton unitInfoButtonPrefab;


    private readonly List<UnitPlacementButton> unitPlacementButtons = new ();
    private readonly List<FactionButton> factionButtons = new ();
    private readonly List<UnitInfoButton> infoButtons = new();

    private void Start()
    {
        InstantiateButtons();

        factionButtons[0].OnClick();
        unitPlacementButtons[0].OnClick();

        StartCoroutine(RefreshLayouts());
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
        
        for (var i = 0; i < ObjectIDList.instance.units.Count; i++)
        {
            var item = Instantiate(unitInfoButtonPrefab, unitFormationDetailLayoutTransform);
            item.unitIndex = i;
            infoButtons.Add(item);
        }
        
        unitPlacementLayoutTransform.localPosition = Vector3.zero;
        
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator RefreshLayouts()
    {
        yield return new WaitForSeconds(1f);
        Canvas.ForceUpdateCanvases();
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
        hudManager.OnFactionSelected(index);
        Canvas.ForceUpdateCanvases();
    }
    
    public void SelectUnitPlacement(int index)
    {
        hudManager.OnFormationSelected(index);
        Canvas.ForceUpdateCanvases();
    }

    public void UpdateUnitFormationDetails(int index)
    {
        var placement = ObjectIDList.GetUnitPlacementScriptable(index);

        foreach (var infoButton in infoButtons)
        {
            infoButton.unitCount = 0;
        }

        foreach (var unitPlacement in placement.placements)
        {
            var infoButton = infoButtons[unitPlacement.unitIndex];
            infoButton.unitCount = unitPlacement.positions.Length;
            infoButton.UpdateButtonState();
        }
        
        Canvas.ForceUpdateCanvases();
    }
}
