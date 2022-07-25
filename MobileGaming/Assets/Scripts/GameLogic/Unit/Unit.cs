using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class Unit : NetworkBehaviour
{
    [Header("Identification")]
    public string unitName;
    [SyncVar] public sbyte playerId;
    [SyncVar] public ScriptableUnit.Classes className;
    [SyncVar] public PlayerSM player;

    [Header("Position")]
    [SyncVar] public sbyte hexCol;
    [SyncVar] public sbyte hexRow;
    [SyncVar(hook = nameof(OnHexChange))] public Hex currentHex;

    [Header("Base Stats")]
    [SyncVar] public string faction;
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
    public Transform modelParent;

    [Header("Scriptables")]
    [SyncVar(hook = nameof(OnScriptableUnitIdChange))] public byte unitScriptableId;
    public ScriptableUnit unitScriptable;
    [SyncVar(hook = nameof(OnScriptableAbilityIdChange))] public byte abilityScriptableId;
    public ScriptableAbility abilityScriptable;

    private void Start()
    {
        outlineScript = ModelSpawner.UpdateUnitModel(this).GetComponent<Outline>();
    }

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
        unitScriptable.SetupEvents(this);
    }

    public void LinkUnitScriptable(byte id)
    {
        var newUnitScriptable = ObjectIDList.GetUnitScriptable(unitScriptableId);
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
        abilityScriptable = ObjectIDList.GetAbilityScriptable(abilityScriptableId);
        currentAbilityCost = Convert.ToSByte((abilityScriptableId == 0) ? 0 : abilityScriptable.baseCost);

        ReplaceModel();
        
        ResetUnitStats();
    }

    private void ReplaceModel()
    {
        outlineScript = ModelSpawner.UpdateUnitModel(this).GetComponent<Outline>();
        RpcReplaceModel();
    }

    [ClientRpc]
    private void RpcReplaceModel()
    {
        outlineScript = ModelSpawner.UpdateUnitModel(this).GetComponent<Outline>();
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

    public void KnockBackUnit(Unit unit, int direction)
    {
        var targetHex = unit.currentHex.neighbours[direction];
        if (targetHex == null)
        {
            unit.TakeDamage(0,3,this);
            return;
        }
        
        if (targetHex.currentUnit != null || targetHex.movementCost == sbyte.MaxValue)
        {
            unit.TakeDamage(0,3,this);
            return;
        }

        ServerSideKnockBackAnim(unit,targetHex);
        RpcClientSideKnockBackAnim(unit,targetHex);
    }

    private void ServerSideKnockBackAnim(Unit unit, Hex hex)
    {
        unit.currentHex.OnUnitExit(unit);
            
        hex.OnUnitEnter(unit);
        
        //TODO - Play Animation
        unit.transform.position = hex.transform.position + Vector3.up * 2f;
    }
    
    [ClientRpc]
    private void RpcClientSideKnockBackAnim(Unit unit, Hex hex)
    {
        //TODO - Play Animation
        unit.transform.position = hex.transform.position + Vector3.up * 2f;
    }
    
    public bool AreEnemyUnitsInRange()
    {
        var enemyPlayer = playerId == 0 ? Convert.ToSByte(1) : Convert.ToSByte(0);
        return currentHex.GetNeighborsInRange(attackRange).Any(hex => hex.HasUnitOfPlayer(enemyPlayer));
    }
    
    public bool IsAlignBetweenEnemies()
    {
        var enemyPlayer = Convert.ToSByte(playerId == 0 ? 1 : (0));
        for (var i = 0; i < 3; i++)
        {
            var hex1 = currentHex.neighbours[i];
            var hex2 = currentHex.neighbours[i + 3];
            if (hex1 == null || hex2 == null) continue;
            if (hex1.HasUnitOfPlayer(enemyPlayer) && hex2.HasUnitOfPlayer(enemyPlayer))
                return true;
        }
        return false;
    }

    public IEnumerable<Unit> AdjacentUnits()
    {
        if (currentHex == null) return new List<Unit>();
        return (from hex in currentHex.neighbours where hex != null where hex.currentUnit != null select hex.currentUnit).ToArray();
    }

    public int NumberOfAdjacentEnemyUnits()
    {
        return AdjacentUnits().Count(unit => unit.playerId != playerId);
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
        unitScriptable = ObjectIDList.GetUnitScriptable(newId);
    }
    
    private void OnScriptableAbilityIdChange(byte prevId,byte newId)
    {
        abilityScriptable = ObjectIDList.GetAbilityScriptable(newId);
    }

    #endregion

    

   
}
