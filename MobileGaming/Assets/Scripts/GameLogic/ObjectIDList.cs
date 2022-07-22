using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIDList : MonoBehaviour
{
    public List<ScriptableFaction> factions { get; private set; } = new List<ScriptableFaction>();
    public  List<ScriptableUnit> units { get; private set; } = new List<ScriptableUnit>();
    public  List<ScriptableTile> tiles { get; private set; } = new List<ScriptableTile>();
    public  List<ScriptableAbility> abilities { get; private set; } = new List<ScriptableAbility>();
    
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
}
