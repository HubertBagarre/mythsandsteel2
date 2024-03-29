using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using DG.Tweening;
using UnityEngine;
using Mirror;
using QuickOutline;
using Random = UnityEngine.Random;

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
    [SyncVar] public bool canAttack;
    [SyncVar] public bool canMove;
    [SyncVar] public bool canUseAbility;

    public readonly SyncList<BaseUnitBuff> currentBuffs = new();

    [Header("Components")]
    public Animator animator;
    public Outline outlineScript;
    public Transform modelParent;

    [Header("Scriptables")]
    [SyncVar(hook = nameof(OnScriptableUnitIdChange))] public byte unitScriptableId;
    public ScriptableUnit unitScriptable;
    [SyncVar(hook = nameof(OnScriptableAbilityIdChange))] public byte abilityScriptableId;
    public ScriptableAbility abilityScriptable;
    
    [Header("Animation Info")]
    [SyncVar] public float walkSpeedMulitplier = 0.75f;
    [SyncVar] public float walkDuration;
    [SyncVar] public float attackPart1Duration;
    [SyncVar] public float abilityPart1Duration;
    [SyncVar] public float deathDuration;

    private void Start()
    {
        if(isServer) return;
        var unitModel = ModelSpawner.UpdateUnitModel(this);
        outlineScript = unitModel.GetComponent<Outline>();
        animator = unitModel.GetComponent<Animator>();
    }

    public void ResetUnitStats()
    {
        maxHp = baseMaxHp;
        currentHp = maxHp;
        physicDef = basePhysicDef;
        magicDef = baseMagicDef;
        attacksPerTurn = baseAtkPerTurn;
        attacksLeft = 0;
        attackDamage = baseAttackDamage;
        attackRange = baseRange;
        move = 0;
        hasBeenActivated = true;
        canUseAbility = false;
        canMove = false;
        canAttack = false;
        unitScriptable.SetupEvents(this);
        
        ReplaceModel();
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

        walkSpeedMulitplier = unitScriptable.walkSpeedMultiplier;
        if (walkSpeedMulitplier == 0) walkSpeedMulitplier = 1;
        var prefabAnimator = unitScriptable.modelPrefab.GetComponent<Animator>();
        if (prefabAnimator != null)
        {
            if (prefabAnimator.runtimeAnimatorController is AnimatorOverrideController overrideController)
            {
                walkDuration = overrideController.animationClips[1].length;
                attackPart1Duration = overrideController.animationClips[2].length;
                abilityPart1Duration = overrideController.animationClips[3].length;
                deathDuration = overrideController.animationClips[4].length;
            }
        }
        
        ResetUnitStats();
    }

    private void ReplaceModel()
    {
        var unitModel = ModelSpawner.UpdateUnitModel(this);
        outlineScript = unitModel.GetComponent<Outline>();
        animator = unitModel.GetComponent<Animator>();
        if(animator != null) animator.SetFloat("IdleOffset",Random.Range(0,0.5f));
        gameObject.SetActive(!isDead);

        RpcReplaceModel(!isDead);
        if(player != null) player.RpcUIUpdateUnitHud();
    }

    [ClientRpc]
    private void RpcReplaceModel(bool showModel)
    {
        outlineScript = ModelSpawner.UpdateUnitModel(this).GetComponent<Outline>();
        gameObject.SetActive(showModel);
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

        CallbackManager.UnitAttack(this, attackedUnit);
    }

    public void TakeDamage(sbyte physicalDamage,sbyte magicalDamage, Unit sourceUnit = null)
    {
        if(!isDead) unitScriptable.TakeDamage(this,physicalDamage,magicalDamage,sourceUnit);
    }

    public void KillUnit(bool physicalDeath,bool magicalDeath,Unit killer)
    {
        Debug.Log($"Killed unit hex before scriptable : {this.currentHex}");
        
        unitScriptable.KillUnit(this,physicalDeath,magicalDeath,killer);

        player.UIUpdateDeathCount(); 
    }

    public void HealUnit(int value)
    {
        unitScriptable.HealUnit(this,value);
    }

    public void RespawnUnit(Hex targetHex)
    {
        ResetUnitStats();
        
        ServerSideKnockBackAnim(this,targetHex);
        RpcClientSideKnockBackAnim(this,targetHex,true);
        
        CallbackManager.UnitRespawned(this);
        
        player.UIUpdateDeathCount();
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
        RpcClientSideKnockBackAnim(unit,targetHex,false);
    }

    private void ServerSideKnockBackAnim(Unit unit, Hex hex)
    {
        if (unit.currentHex != null)
        {
            var unitCurrentHex = unit.currentHex;
                
            unitCurrentHex.OnUnitExit(unit);
                
            unitCurrentHex.currentUnit = unitCurrentHex.previousUnit;
            unitCurrentHex.previousUnit = null;
        }
        
        hex.currentUnit = unit;
        unit.currentHex = hex;
        unit.hexCol = hex.col;
        unit.hexRow = hex.row;
        
        hex.OnUnitEnter(unit);
        
        //TODO - Play Animation
        unit.transform.position = hex.transform.position + Vector3.up * 2f;
    }
    
    [ClientRpc]
    private void RpcClientSideKnockBackAnim(Unit unit, Hex hex,bool respawnAnimation)
    {
        //TODO - Play Animation
        unit.transform.position = hex.transform.position + Vector3.up * 2f;
    }

    public void AddBuff(BaseUnitBuff buff)
    {
        buff.assignedUnit = this;
        buff.AddBuff();
    }
    
    [ClientRpc]
    public void RpcSetUnitActive(bool value)
    {
        if (value)
        {
            gameObject.SetActive(true);
            PlayDeathAnimation(false);
            return;
        }

        StartCoroutine(PlayDeathAnimationRoutine());


    }

    private IEnumerator PlayDeathAnimationRoutine()
    {
        PlayDeathAnimation(true);
        yield return new WaitForSeconds(deathDuration);
        gameObject.SetActive(false);
    }

    
    public void LookAt(Hex hex)
    {
        var dir = hex.transform.position - transform.position;
        var lookRotation = Quaternion.LookRotation(dir);
        var rotation = lookRotation.eulerAngles;
        
        transform.DOLocalRotate(new Vector3(0f, rotation.y, 0f),0.1f);
    }

    public void LookAt(Unit unit)
    {
        if(unit.currentHex!= null) LookAt(unit.currentHex);
    }

    public void PlayAttackAnimation()
    {
        if (animator == null) return;
        
        animator.SetTrigger("Attack");
    }
    
    public void PlayAbilityAnimation()
    {
        if (animator == null) return;
        
        animator.SetTrigger("Ability");
    }

    public void PlayWalkingAnimation(bool value)
    {
        if (animator == null) return;
        
        animator.SetBool("IsWalking",value);
    }
    
    private void PlayDeathAnimation(bool value)
    {
        if (animator == null) return;
        
        animator.SetBool("IsDead",value);
    }

    #region Helpers
    
    public bool IsOnHexOfType(byte id)
    {
        if (currentHex == null) return false;
        return currentHex.currentTileID == id;
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

    #endregion
    

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
