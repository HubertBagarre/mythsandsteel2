using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityCallBacks
{
    public IEnumerable<Hex> AbilitySelectables(Unit castingUnit);
    public void OnAbilityTargetingHexes(Unit castingUnit,IEnumerable<Hex> targetedHexes,PlayerSM player);
}
