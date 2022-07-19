using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

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
        
        Debug.Log($"Applying unit {unitScriptableId} ({ObjectIDList.instance.units[unitScriptableId].name})");
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


    public bool AreEnemyUnitsInRange()
    {
        var enemyPlayer = playerId == 0 ? Convert.ToSByte(1) : Convert.ToSByte(0);
        return currentHex.GetNeighborsInRange(attackRange).Any(hex => hex.HasUnitOfPlayer(enemyPlayer));
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
        currentHex.currentUnit = null;
        currentHex = null;
        gameObject.SetActive(false);
        RpcSetUnitActive(false);
        if (unitScriptable is IUnitCallBacks scriptableAdded) scriptableAdded.OnDeath(this);
    }
    

    [ClientRpc]
    private void RpcSetUnitActive(bool value)
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
