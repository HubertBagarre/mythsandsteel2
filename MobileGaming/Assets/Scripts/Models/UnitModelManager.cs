using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitModelManager : MonoBehaviour
{
    public static GameObject UpdateUnitModel(Unit unit)
    {
        if (unit.modelParent.childCount > 0) Destroy(unit.modelParent.GetChild(0).gameObject);

        return Instantiate(unit.unitScriptable.modelPrefab, unit.modelParent);
    }
}
