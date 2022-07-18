using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit")]
public class ScriptableUnit : ScriptableObject
{
    [Header("Info")]
    public string unitName;
    public string faction;
    public string className;

    [Header("Stats")]
    public sbyte baseMaxHp;
    public sbyte basePhysicDef;
    public sbyte baseMagicDef;
    public sbyte baseAtkPerTurn;
    public sbyte baseDamage;
    public sbyte baseRange;
    public sbyte baseMove;

    [Header("Abilities")]
    public bool hasAbility;
    public bool abilityTargetHexes;
    public byte abilityTargetCount;
    public byte abilityRange;

    [Header("Animations")]
    public Animation idle;
    public Animation movement;
    public Animation attack;
    public Animation ability;
    public Animation takeDamage;
    public Animation death;
}
