using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Tiles/Basic")]
public class ScriptableTile : ScriptableObject
{
    public GameObject model;
    public sbyte movementCost;
}
