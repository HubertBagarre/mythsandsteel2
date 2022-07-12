using System;
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
    [SyncVar] public sbyte hexGridIndex;
    
    [Header("Position")]
    [SyncVar] public sbyte hexCol;
    [SyncVar] public sbyte hexRow;
    [SyncVar(hook = nameof(OnHexChange))] public Hex currentHex;

    [Header("Base Stats")]
    [SyncVar,ReadOnly] public string faction;
    [SyncVar,ReadOnly] public string className;
    [SyncVar,ReadOnly] public sbyte baseMaxHp;
    [SyncVar,ReadOnly] public sbyte basePhysicDef;
    [SyncVar,ReadOnly] public sbyte baseMagicDef;
    [SyncVar,ReadOnly] public sbyte baseAtkPerTurn;
    [SyncVar,ReadOnly] public sbyte basePhysicDamage;
    [SyncVar,ReadOnly] public sbyte baseMagicDamage;
    [SyncVar,ReadOnly] public sbyte baseRange;
    [SyncVar,ReadOnly] public sbyte baseMove;

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
    [SyncVar] public bool hasBeenActivated;

    [Header("Components")]
    public NetworkAnimator animator;
    public Outline outlineScript;
    
    [Header("Scriptables")]
    public ScriptableUnit unitScriptable;
    public ScriptableAbility abilityScriptable;
    public ScriptableAbility damageModifier;


    public void ChangeTransformPosition(Vector3 newPos)
    {
        ServerChangePosition(newPos);
        RpcChangePosition(newPos);
    }
    
    private void ServerChangePosition(Vector3 newPos)
    {
        transform.position = newPos;
    }
    
    [ClientRpc]
    private void RpcChangePosition(Vector3 newPos)
    {
        transform.position = newPos;
        
    }
    
    public void TakeDamage(sbyte damageTaken)
    {
        actualHp -= damageTaken;

        if (actualHp <= 0) Death();
    }

    public void Death()
    {

    }

    public void OnHexChange(Hex previousHex, Hex newHex)
    {
        if(previousHex != null) previousHex.OnUnitExit(this);
        newHex.OnUnitEnter(this);
    }
}
