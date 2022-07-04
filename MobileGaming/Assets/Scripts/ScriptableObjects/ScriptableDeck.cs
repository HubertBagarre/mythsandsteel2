using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Deck")]
public class ScriptableDeck : ScriptableObject
{
    public List<ScriptableUnit> units;

    //public List<Buff> factionBuffs;
}
