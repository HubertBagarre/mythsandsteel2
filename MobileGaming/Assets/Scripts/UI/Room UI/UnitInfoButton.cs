using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UnitInfoButton : MonoBehaviour
{
    public int unitIndex;
    public int unitCount;
    [SerializeField] private TextMeshProUGUI unitNameText,unitDefensiveStatText, unitOffensiveStat1Text, unitOffensiveStat2Text,unitCountText;
    [SerializeField] private Image unitPortraitImage;
    [SerializeField] private Button button;
    
    private void Start()
    {
        var unit = ObjectIDList.GetUnitScriptable(unitIndex);

        unitNameText.text = unit.name;
        unitDefensiveStatText.text = $"{unit.baseMaxHp}<sprite=39>\n{unit.baseMagicDef}<sprite=21>\n{unit.basePhysicDef}<sprite=43>";
        unitOffensiveStat1Text.text = $"{unit.baseAtkPerTurn}<sprite=39>\n{unit.baseDamage}<sprite=21>\n{unit.baseMove}<sprite=43>";
        unitOffensiveStat2Text.text = $"{unit.baseRange}<sprite=37>";

        unitPortraitImage.sprite = unit.portraitImage;
        
        unitCountText.text = $"{unitCount}";
        
        gameObject.SetActive(unitCount > 0);
        
        button.onClick.AddListener(OnClick);
    }

    public void UpdateButtonState()
    {
        unitCountText.text = $"{unitCount}";
        gameObject.SetActive(unitCount > 0);
    }

    private void OnClick()
    {
        Debug.LogWarning("Not Implemented");
    }
}
