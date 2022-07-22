using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit Placement")]
public class ScriptableUnitPlacement : ScriptableObject
{
    public UnitPlacement[] placements;
    public Sprite placementPreview;

    [Serializable]
    public class UnitPlacement
    {
        public int unitIndex;
        public Vector2[] positions;
    }
}
