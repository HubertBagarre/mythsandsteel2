using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    private Camera cam;
    private Vector3 offset;
    private Unit assignedUnit;

    public void SetVariables(Vector2 pos, Unit unit,Camera c)
    {
        offset = (Vector3)pos;
        assignedUnit = unit;
        cam = c;
    }
    
    public void UpdateHud()
    {
        if(assignedUnit == null) return;
        if (assignedUnit.isDead)
        {
            textComponent.text = string.Empty;
            return;
        }
        var unitPosition = assignedUnit.transform.position;
        var newPosition = cam.WorldToScreenPoint(unitPosition) + offset;
        transform.position = newPosition;
        textComponent.text = assignedUnit.currentHp.ToString();
    }

}
