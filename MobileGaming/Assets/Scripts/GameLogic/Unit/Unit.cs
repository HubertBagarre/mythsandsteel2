using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NaughtyAttributes;

public class Unit : NetworkBehaviour
{
    [Header("Identification")]
    public string unitName;
    [SyncVar] public sbyte playerId;
    public Hex currentHex;

    [Header("Base Stats")]
    public string faction;
    public string className;
    [ReadOnly] public sbyte baseMaxHp;
    [ReadOnly] public sbyte basePhysicDef;
    [ReadOnly] public sbyte baseMagicDef;
    [ReadOnly] public sbyte baseAtkPerTurn;
    [ReadOnly] public sbyte basePhysicDamage;
    [ReadOnly] public sbyte baseMagicDamage;
    [ReadOnly] public sbyte baseRange;
    [ReadOnly] public sbyte baseMove;

    [Header("Current Stats")]
    [SyncVar] public sbyte maxHp;
    [SyncVar] public sbyte actualHp;
    [SyncVar] public sbyte physicDef;
    [SyncVar] public sbyte magicDef;
    [SyncVar] public sbyte atkPerTurn;
    [SyncVar] public sbyte physicDamage;
    [SyncVar] public sbyte magicDamage;
    [SyncVar] public sbyte range;
    [SyncVar] public sbyte move;

    [Header("Components")]
    public NetworkAnimator Animator;
    public Outline outlineScript;
    
    [Header("Scriptables")]
    public ScriptableUnit unitScriptable;
    public ScriptableAbility abilityScriptable;
    public ScriptableAbility damageModifier;

    public void TakeDamage(sbyte damageTaken)
    {
        actualHp -= damageTaken;

        if (actualHp <= 0) Death();
    }

    public void Death()
    {

    }
}
