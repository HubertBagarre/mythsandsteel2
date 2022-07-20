using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CallbackManagement;
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

        var castingPlayerId = player.playerId;
        var previousScriptables = new Dictionary<Hex, int>();
        foreach (var hex in targetedHexes)
        {
            previousScriptables.Add(hex,hex.currentTileID);
            hex.ApplyTileServer(3);
        }
        
        CallbackManager.OnPlayerTurnStart += RemoveStorms;
        
        void RemoveStorms(PlayerSM playerSm)
        {
            if(GameSM.instance.currentPlayer != castingPlayerId) return;
            foreach (var pair in previousScriptables)
            {
                pair.Key.ApplyTileServer(pair.Value);
            }
        }
    }
}
