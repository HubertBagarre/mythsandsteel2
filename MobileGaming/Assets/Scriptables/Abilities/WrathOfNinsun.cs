using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Abilities/Wrath of Ninsun")]
public class WrathOfNinsun : ScriptableAbility,IAbilityCallBacks
{
    public IEnumerable<Hex> AbilitySelectables(Unit castingUnit)
    {
        return castingUnit.currentHex.GetNeighborsInRange(abilityRange).Where(hex => hex.movementCost != sbyte.MaxValue && !hex.HasUnitOfPlayer(castingUnit.playerId));;
    }
    
    public void OnAbilityTargetingHexes(Unit castingUnit, IEnumerable<Hex> targetedHexes, PlayerSM player)
    {
        var castingPlayerId = player.playerId;
        var previousScriptables = new Dictionary<Hex, int>();
        foreach (var hex in targetedHexes)
        {
            if (hex.HasUnitOfPlayer(Convert.ToSByte(player.playerId == 0 ? 1 : 0)))
            {
                if (player.ConsumeFaith(3))
                {
                    sbyte damage = 4;
                    if (hex.currentUnit.IsAlignBetweenEnemies()) damage += Convert.ToSByte(4);
                    
                    hex.currentUnit.TakeDamage(0,damage,castingUnit);
                }
            }
            
            if (hex.currentUnit == null ) SummonStorm(hex);
        }

        void SummonStorm(Hex hex)
        {
            previousScriptables.Add(hex,hex.currentTileID);
            hex.ApplyTileServer(3);
        }
        
        CallbackManager.OnPlayerTurnStart += RemoveStorms;
        
        void RemoveStorms(PlayerSM playerSm)
        {
            Debug.Log("Removing Storm");
            if(GameSM.instance.currentPlayer != castingPlayerId) return;
            foreach (var pair in previousScriptables)
            {
                pair.Key.ApplyTileServer(pair.Value);
            }
            
            CallbackManager.OnPlayerTurnStart -= RemoveStorms;
        }
    }
}
