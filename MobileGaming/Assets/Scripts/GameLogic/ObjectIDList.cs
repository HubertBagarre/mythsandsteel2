using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIDList : MonoBehaviour
{
    [field: SerializeField] public List<ScriptableFaction> factions { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableUnit> units { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableTile> tiles { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableAbility> abilities { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableUnitPlacement> unitPlacements { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableCollectible> collectibles { get; private set; } = new ();
    [field: SerializeField] public  List<ScriptableBuffInfo> buffInfos { get; private set; } = new ();
    
    public static ObjectIDList instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(this);
    }

    public static ScriptableFaction GetFactionScriptable(int index)
    {
        if (index < 0 || index >= instance.factions.Count) index = 0;
        return instance.factions[index];
    }
    
    public static ScriptableUnit GetUnitScriptable(int index)
    {
        if (index < 0 || index >= instance.units.Count) index = 0;
        return instance.units[index];
    }
    
    public static ScriptableTile GetTileScriptable(int index)
    {
        if (index < 0 || index >= instance.tiles.Count) index = 0;
        return instance.tiles[index];
    }
    
    public static ScriptableAbility GetAbilityScriptable(int index)
    {
        if (index < 0 || index >= instance.abilities.Count) index = 0;
        return instance.abilities[index];
    }
    
    public static ScriptableUnitPlacement GetUnitPlacementScriptable(int index)
    {
        if (index < 0 || index >= instance.unitPlacements.Count) index = 0;
        return instance.unitPlacements[index];
    }
    
    public static ScriptableCollectible GetCollectibleScriptable(int index)
    {
        if (index < 0 || index >= instance.collectibles.Count) index = 0;
        return instance.collectibles[index];
    }
    
    public static ScriptableBuffInfo GetBuffScriptable(int index)
    {
        if (index < 0 || index >= instance.buffInfos.Count) index = 0;
        return instance.buffInfos[index];
    }
}
