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
    
    public void OnAbilityTargetingHexes(Unit castingUnit, IEnumerable<Hex> targetedHexes, PlayerSM player)
    {
        Debug.Log($"{castingUnit} (of layer {castingUnit.playerId}) is using {name} on {targetedHexes.Count()} target(s)");
    }
}
