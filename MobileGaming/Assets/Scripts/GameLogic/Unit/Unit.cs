using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NaughtyAttributes;

public class Unit : NetworkBehaviour
{
    public string unitName;
    public string faction;
    public string className;
    [SyncVar] public sbyte playerId;

    [ReadOnly] public sbyte baseMaxHp;
    [ReadOnly] public sbyte basePhysicDef;
    [ReadOnly] public sbyte baseMagicDef;
    [ReadOnly] public sbyte baseAtkPerTurn;
    [ReadOnly] public sbyte baseDamage;
    [ReadOnly] public sbyte baseRange;
    [ReadOnly] public sbyte baseMove;

    [SyncVar] public sbyte MaxHp;
    [SyncVar] public sbyte PhysicDef;
    [SyncVar] public sbyte MagicDef;
    [SyncVar] public sbyte AtkPerTurn;
    [SyncVar] public sbyte Damage;
    [SyncVar] public sbyte Range;
    [SyncVar] public sbyte Move;

    public NetworkAnimator Animator;
    public ScriptableUnit unitScriptable;
    public ScriptableAbility abilityScriptable;
    public ScriptableAbility damageModifier;
    public Outline outlineScript;
}
