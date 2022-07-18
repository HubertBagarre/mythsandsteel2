using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitCallBacks
{
    public sbyte OnAttackTriggered(Unit attackingUnit, Unit attackedUnit);
    public void OnAbilityTargetingUnits(IEnumerable<Unit> targetedUnits,sbyte playerId);
    public void OnAbilityTargetingHexes(IEnumerable<Hex> targetedHexes,sbyte playerId);
    public void OnDamageTaken(Unit attackedUnit, sbyte damage);
    public void OnDeath(Unit unit);
    public void OnPhysicalDamageTaken(Unit attackedUnit, sbyte damage);
    public void OnMagicalDamageTaken(Unit attackedUnit, sbyte damage);
    public void OnUnitEnterAdjacentHex(Unit thisUnit, Unit enteringUnit);
    public void OnUnitExitAdjacentHex(Unit thisUnit, Unit enteringUnit);
}
