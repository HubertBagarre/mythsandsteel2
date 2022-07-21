using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/No Faction")]
public class ScriptableFaction : ScriptableObject
{
    [TextArea]
    public string beliefAbilityDescription;
    
    [TextArea]
    public string culturalAbilityDescription;
    
    public virtual void SetupEvents(){}
}
