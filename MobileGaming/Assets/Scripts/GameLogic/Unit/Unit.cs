using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEditor;

public class Unit : NetworkBehaviour
{
    [Header("Identification")]
    public string unitName;
    [SyncVar] public sbyte playerId;

    [Header("Position")]
    [SyncVar] public sbyte hexCol;
    [SyncVar] public sbyte hexRow;
    [SyncVar(hook = nameof(OnHexChange))] public Hex currentHex;

    [Header("Base Stats")]
    [SyncVar] public string faction;
    [SyncVar] public string className;
    [SyncVar] public sbyte baseMaxHp;
    [SyncVar] public sbyte basePhysicDef;
    [SyncVar] public sbyte baseMagicDef;
    [SyncVar] public sbyte baseAtkPerTurn;
    [SyncVar] public sbyte baseAttackDamage;
    [SyncVar] public sbyte baseMagicDamage;
    [SyncVar] public sbyte baseRange;
    [SyncVar] public sbyte baseMove;
    public bool hasAbility => abilityScriptable != null;
    
    [Header("Current Stats")]
    [SyncVar] public sbyte maxHp;
    [SyncVar] public sbyte currentHp;
    public bool isDead => currentHp <= 0;
    [SyncVar] public sbyte physicDef;
    [SyncVar] public sbyte magicDef;
    [SyncVar] public sbyte attacksPerTurn;
    [SyncVar] public sbyte attacksLeft;
    [SyncVar] public sbyte attackDamage;
    [SyncVar] public sbyte attackRange;
    [SyncVar] public sbyte move;
    [SyncVar] public sbyte currentAbilityCost;
    [SyncVar] public bool hasBeenActivated;
    [SyncVar] public bool canUseAbility;

    [Header("Components")]
    public NetworkAnimator animator;
    public Outline outlineScript;

    [Header("Scriptables")]
    [SyncVar(hook = nameof(OnScriptableUnitIdChange))] public byte unitScriptableId;
    public ScriptableUnit unitScriptable;
    [SyncVar(hook = nameof(OnScriptableAbilityIdChange))] public byte abilityScriptableId;
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
        attackRange = baseRange;
        move = baseMove;
        hasBeenActivated = false;
        canUseAbility = true;
    }

    public void LinkUnitScriptable(byte id)
    {
        if (id > ObjectIDList.instance.units.Count) id = 0;
        unitScriptableId = id;
        
        var newUnitScriptable = ObjectIDList.instance.units[unitScriptableId];
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

        abilityScriptableId = unitScriptable.abilityScriptableId;
        abilityScriptable = ObjectIDList.instance.abilities[abilityScriptableId];
        currentAbilityCost = Convert.ToSByte((abilityScriptableId == 0) ? 0 : abilityScriptable.baseCost);

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
        unitScriptable.AttackUnit(this,attackedUnit);
    }

    public void TakeDamage(sbyte physicalDamage,sbyte magicalDamage, Unit sourceUnit = null)
    {
        unitScriptable.TakeDamage(this,physicalDamage,magicalDamage,sourceUnit);
    }

    public void KillUnit(bool physicalDeath,bool magicalDeath,Unit killer)
    {
        unitScriptable.KillUnit(this,physicalDeath,magicalDeath,killer);
    }
    
    public bool AreEnemyUnitsInRange()
    {
        var enemyPlayer = playerId == 0 ? Convert.ToSByte(1) : Convert.ToSByte(0);
        return currentHex.GetNeighborsInRange(attackRange).Any(hex => hex.HasUnitOfPlayer(enemyPlayer));
    }

    [ClientRpc]
    public void RpcSetUnitActive(bool value)
    {
        gameObject.SetActive(value);
    }

    #region Hooks

    private void OnHexChange(Hex previousHex, Hex newHex)
    {
        if(newHex == null) return;
        if(previousHex != null) previousHex.OnUnitExit(this);
        newHex.OnUnitEnter(this);
    }

    private void OnScriptableUnitIdChange(byte prevId,byte newId)
    {
        unitScriptable = ObjectIDList.instance.units[newId];
    }
    
    private void OnScriptableAbilityIdChange(byte prevId,byte newId)
    {
        abilityScriptable = ObjectIDList.instance.abilities[newId];
    }

    #endregion

    

   
}
