using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RespawnUnitButton : MonoBehaviour
{
    public Unit associatedUnit;
    public PlayerSM associatedPlayer;
    
    [Header("Components")]
    public Button button;
    public TextMeshProUGUI textComponent;
    public Image imageComponent;

    private void Start()
    {
        button.onClick.AddListener(() => associatedPlayer.TryToRespawnUnit(associatedUnit));
    }

    private void OnEnable()
    {
        textComponent.text = $"{8 + associatedPlayer.faithModifier}<sprite=0>";
        imageComponent.sprite = ObjectIDList.GetUnitScriptable(associatedUnit.unitScriptableId).portraitImage;
    }
}
