using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Karbantos")]
public class TestUnit : ScriptableUnit, IUnitCallBacks
{
    public sbyte OnAttackTriggered(Unit attackingUnit, Unit attackedUnit)
    {
        Debug.Log("One More Damage");
        return 1;
    }

    public void OnAbilityTargetingUnits(IEnumerable<Unit> targetedUnits,sbyte playerId)
    {
        
    }

    public void OnAbilityTargetingHexes(IEnumerable<Hex> targetedHexes,sbyte playerId)
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
