using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit")]
public class ScriptableUnit : ScriptableObject
{
    public enum Classes
    {
        Cavalry,
        Infantry,
        AntiCavalry,
    };
    
    [Header("Info")]
    public string unitName;
    public string faction;
    public Classes className;

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
    
    public virtual void SetupEvents(Unit affectedUnit) { }
    
    public virtual void AttackUnit(Unit attackingUnit, Unit attackedUnit)
    {
        Debug.Log($"{attackingUnit} is attacking {attackedUnit} !!");

        attackedUnit.TakeDamage(attackingUnit.attackDamage, 0, attackingUnit);
    }

    public virtual void TakeDamage(Unit targetUnit, sbyte physicalDamage,sbyte magicalDamage, Unit sourceUnit = null)
    {
        if (sourceUnit == null) sourceUnit = targetUnit;
        
        physicalDamage -= targetUnit.physicDef;
        if (physicalDamage < 0) physicalDamage = 0;
        magicalDamage -= targetUnit.magicDef;
        if (magicalDamage < 0) magicalDamage = 0;
        
        Debug.Log($"{targetUnit} took {physicalDamage} physical Damage and {magicalDamage} magical Damage from {sourceUnit}!!");
        
        targetUnit.currentHp -= Convert.ToSByte(physicalDamage + magicalDamage) ;
        
        if (targetUnit.currentHp <= 0)
        {
            targetUnit.KillUnit((physicalDamage>0),(magicalDamage>0),sourceUnit);
        }
    }

    public virtual void KillUnit(Unit killedUnit,bool physicalDeath,bool magicalDeath,Unit killer)
    {
        Debug.Log($"{killedUnit} was killed by {killer} !! Physical Death : {physicalDeath}, Magical Death : {magicalDeath}");
        
        killedUnit.currentHex.OnUnitExit(killedUnit);
        killedUnit.currentHex = null;
        killedUnit.gameObject.SetActive(false);
        killedUnit.RpcSetUnitActive(false);
    }
}
