using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionSelector : MonoBehaviour
{
    [SerializeField] private List<FactionButton> factionButtons;
    
    [SerializeField] private TextMeshProUGUI factionTitleText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;
    
    [SerializeField] private Transform layoutTransform;
    [SerializeField] private FactionButton factionButtonPrefab;
    
    public static int selectedFaction;

    private void Start()
    {
        InstantiateButtons();
        
        factionButtons[0].OnClick();
    }

    private void InstantiateButtons()
    {
        for (var i = 0; i < ObjectIDList.instance.factions.Count; i++)
        {
            var item = Instantiate(factionButtonPrefab, layoutTransform);
            item.factionIndex = i;
            item.selector = this;
            item.button.onClick.AddListener(EnableAllButtons);
            factionButtons.Add(item);
        }
    }

    private void EnableAllButtons()
    {
        foreach (var fButton in factionButtons)
        {
            fButton.button.interactable = true;
        }
    }

    public void SelectFaction(int factionIndex)
    {
        selectedFaction = factionIndex;
        var faction = ObjectIDList.instance.factions[factionIndex];
        factionTitleText.text = faction.name;

        factionDescriptionText.text =
            $"Belief Ability: \n{faction.beliefAbilityDescription}\n \nCultural Ability: \n{faction.beliefAbilityDescription}";

    }
}
