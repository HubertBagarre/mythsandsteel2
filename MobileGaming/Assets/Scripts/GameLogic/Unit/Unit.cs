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
    [SyncVar,ReadOnly] public sbyte baseAttackDamage;
    [SyncVar,ReadOnly] public sbyte baseMagicDamage;
    [SyncVar,ReadOnly] public sbyte baseRange;
    [SyncVar,ReadOnly] public sbyte baseMove;

    [Header("Current Stats")]
    [SyncVar] public sbyte maxHp;
    [SyncVar] public sbyte currentHp;
    [SyncVar] public sbyte physicDef;
    [SyncVar] public sbyte magicDef;
    [SyncVar] public sbyte attacksPerTurn;
    [SyncVar] public sbyte attacksLeft;
    [SyncVar] public sbyte attackDamage;
    [SyncVar] public sbyte range;
    [SyncVar] public sbyte move;
    [SyncVar] public bool hasBeenActivated;
    [SyncVar] public bool canUseAbility;

    [Header("Components")]
    public NetworkAnimator animator;
    public Outline outlineScript;
    
    [Header("Scriptables")]
    public ScriptableUnit unitScriptable;
    public ScriptableAbility attackAbility;
    public ScriptableAbility abilityScriptable;
    

    
    public void ResetUnitStats()
    {
        maxHp = baseMaxHp;
        currentHp = maxHp;
        physicDef = basePhysicDef;
        magicDef = baseMagicDef;
        attacksPerTurn = baseAtkPerTurn;
        attacksLeft = attacksPerTurn;
        attackDamage = baseAttackDamage;
        range = baseRange;
        move = baseMove;
        hasBeenActivated = false;
        canUseAbility = true;
    }

    public void LinkUnitScriptable(ScriptableUnit newUnitScriptable)
    {
        unitScriptable = newUnitScriptable;

        unitName = unitScriptable.unitName;
        faction = unitScriptable.faction;
        className = unitScriptable.className;
        baseMaxHp = unitScriptable.baseMaxHp;
        basePhysicDef = unitScriptable.basePhysicDef;
        baseMagicDef = unitScriptable.baseMagicDef;
        baseAtkPerTurn = unitScriptable.baseAtkPerTurn;
        baseAttackDamage = unitScriptable.baseDamage;
        baseMagicDamage = 0;
        baseRange = unitScriptable.baseRange;
        baseMove = unitScriptable.baseMove;
        
        ResetUnitStats();
    }
    
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
    
    public void AttackUnit(Unit attackedUnit)
    {
        Debug.Log($"Attacking {attackedUnit} !!");

        sbyte moreDamage = 0;
        if (unitScriptable is IUnitCallBacks scriptableAdded) moreDamage = scriptableAdded.OnAttackTriggered(this, attackedUnit);

        attackedUnit.TakePhysicalDamage(Convert.ToSByte(attackDamage + moreDamage));
        
    }

    public void TakeDamage(sbyte damage)
    {
        Debug.Log($"Took {damage} !!");
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Death();
        }
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnDamageTaken(this,damage);
    }

    public void TakePhysicalDamage(sbyte damage)
    {
        damage -= physicDef;
        TakeDamage(damage);
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnPhysicalDamageTaken(this,damage);
    }

    public void TakeMagicalDamage(sbyte damage)
    {
        damage -= magicDef;
        TakeDamage(damage);
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnMagicalDamageTaken(this,damage);
    }

    #region Unit CallBacks
    
    public void OnUnitEnterAdjacentHex(Unit enteringUnit)
    {
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnUnitEnterAdjacentHex(this,enteringUnit);
    }

    public void OnUnitExitAdjacentHex(Unit exitingUnit)
    {
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnUnitExitAdjacentHex(this,exitingUnit);
    }
    
    #endregion
    
    
    public void Death()
    {
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnDeath(this);
    }

    public void OnHexChange(Hex previousHex, Hex newHex)
    {
        if(previousHex != null) previousHex.OnUnitExit(this);
        newHex.OnUnitEnter(this);
    }

   
}
