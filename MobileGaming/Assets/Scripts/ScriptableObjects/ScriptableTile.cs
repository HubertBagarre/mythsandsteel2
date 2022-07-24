using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Tiles/Basic")]
public class ScriptableTile : ScriptableObject
{
    public GameObject model;
    public sbyte movementCost;
    
    public virtual void SetupEvents(Hex hex) { }
    
    public virtual void OnHexChanged(Hex hex) { }

    public virtual void OnHexEnter(Unit unit,Hex hex) { }

    public virtual void OnHexExit(Unit unit,Hex hex) { }
    
    
}
