using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityCallBacks
{
    public IEnumerable<Hex> AbilitySelectables(Unit castingUnit);
    public void OnAbilityTargetingUnits(Unit castingUnit,IEnumerable<Unit> targetedUnits,sbyte playerId);
    public void OnAbilityTargetingHexes(Unit castingUnit,IEnumerable<Hex> targetedHexes,sbyte playerId);
}
