using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/No Faction")]
public class ScriptableFaction : ScriptableObject
{
    [TextArea(7,15)]
    public string beliefAbilityDescription;
    
    [TextArea(7,15)]
    public string culturalAbilityDescription;
    
    public virtual void SetupEvents(PlayerSM player){}
}
