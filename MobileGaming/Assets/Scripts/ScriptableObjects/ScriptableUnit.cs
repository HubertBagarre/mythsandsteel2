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
    public byte abilityScriptableId;

    [Header("Animations")]
    public Animation idleAnimation;
    public Animation movementAnimation;
    public Animation attackAnimation;
    public Animation abilityAnimation;
    public Animation takeDamageAnimation;
    public Animation deathAnimation;
}
