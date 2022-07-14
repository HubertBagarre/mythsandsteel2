using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit")]
public class ScriptableUnit : ScriptableObject, IUnitCallBacks
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
    public ScriptableAbility abilityScriptable;
    public ScriptableAbility damageModifier;

    [Header("Animations")]
    public Animation idle;
    public Animation movement;
    public Animation attack;
    public Animation ability;
    public Animation takeDamage;
    public Animation death;
    
    
    public void OnAttackTriggered(Unit attackingUnit, Unit attackedUnit)
    {
    }

    public void OnDamageTaken(Unit attackedUnit, sbyte damage)
    {
    }

    public void OnDeath(Unit unit)
    {
    }

    public void OnPhysicalDamageTaken(Unit attackedUnit, sbyte damage)
    {
    }

    public void OnMagicalDamageTaken(Unit attackedUnit, sbyte damage)
    {
    }

    public void OnUnitEnterAdjacentHex(Unit thisUnit, Unit enteringUnit)
    {
    }

    public void OnUnitExitAdjacentHex(Unit thisUnit, Unit enteringUnit)
    {
        
    }
}
