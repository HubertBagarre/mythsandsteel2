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

    private void Start()
    {
        button.onClick.AddListener(() => associatedPlayer.TryToRespawnUnit(associatedUnit));
    }
}
