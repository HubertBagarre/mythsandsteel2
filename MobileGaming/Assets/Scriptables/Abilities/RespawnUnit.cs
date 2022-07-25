using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Abilities/Respawn Unit")]
public class RespawnUnit : ScriptableAbility, IAbilityCallBacks
{
    public IEnumerable<Hex> AbilitySelectables(Unit castingUnit)
    {
        return castingUnit.player.allHexes.Where(hex => hex.respawnableUnitTeam == castingUnit.playerId).Where(hex => hex.currentUnit == null);
    }

    public void OnAbilityTargetingHexes(Unit castingUnit, IEnumerable<Hex> targetedHexes, PlayerSM player)
    {
        var targetHex = (targetedHexes as Hex[] ?? targetedHexes.ToArray())[0];

        if(player.ConsumeFaith(8)) castingUnit.RespawnUnit(targetHex);
        
        Debug.Log($"Respawning {castingUnit} on {targetHex}");
    }
}
