using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    public static GameObject UpdateUnitModel(Unit unit)
    {
        foreach (Transform child in unit.modelParent)
        {
            Destroy(child.gameObject);
        }

        return Instantiate(unit.unitScriptable.modelPrefab, unit.modelParent);
    }
    
    public static GameObject UpdateHexModel(Hex hex)
    {
        if (hex.modelParent.childCount > 0) Destroy(hex.modelParent.GetChild(0).gameObject);
        
        return !hex.shouldBeRendered ? null : Instantiate(ObjectIDList.GetTileScriptable(hex.currentTileID).model, hex.modelParent);
    }

    public static GameObject UpdateHexCollectible(Hex hex)
    {
        if (hex.modelPropsParent.childCount > 0) Destroy(hex.modelPropsParent.GetChild(0).gameObject);
        
        return !hex.hasCollectible ? null : Instantiate(ObjectIDList.GetCollectibleScriptable(hex.currentCollectibleId).collectibleModelPrefab, hex.modelPropsParent);
    }
}
