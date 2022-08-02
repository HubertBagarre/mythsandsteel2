using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/No Faction")]
public class ScriptableFaction : ScriptableObject
{
    [TextArea(7,15)]
    public string beliefAbilityDescription;
    
    [TextArea(7,15)]
    public string culturalAbilityDescription;
    
    [TextArea(7,15)]
    public string loreDescription;
    
    [TextArea(7,15)]
    public string strategyDescription;

    public Sprite factionIllustration;
    
    public virtual void SetupEvents(PlayerSM player){}
}
