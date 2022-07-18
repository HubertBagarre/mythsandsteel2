using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Abilities/Wrath of Ninsun")]
public class TestAbility : ScriptableAbility,IAbilityCallBacks
{
    public IEnumerable<Hex> AbilitySelectables(Unit castingUnit)
    {
        return castingUnit.currentHex.GetNeighborsInRange(abilityRange).Where(hex => hex.movementCost != sbyte.MaxValue && !hex.HasUnitOfPlayer(castingUnit.playerId));;
    }

    public void OnAbilityTargetingUnits(Unit castingUnit, IEnumerable<Unit> targetedUnits, sbyte playerId)
    {
        throw new System.NotImplementedException();
    }

    public void OnAbilityTargetingHexes(Unit castingUnit, IEnumerable<Hex> targetedHexes, sbyte playerId)
    {
        throw new System.NotImplementedException();
    }
}
