using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Faction/Vultures Of Eannatum")]
public class FactionVulturesOfEannatum : ScriptableFaction
{
    public override void SetupEvents(PlayerSM player)
    {
        var targetPlayer = player;
        
        CallbackManager.OnPlayerTurnStart += VoEGainFaithOnTurnStart;

        CallbackManager.OnPlayerTurnStart += VoEDamageEnemyUnitsNearVoid;

        CallbackManager.OnPlayerTurnEnd += VoEGainFaithOnTurnEnd;

        void VoEGainFaithOnTurnStart(PlayerSM playerSm)
        {
            if (!GameSM.IsPlayerTurn(targetPlayer)) return;

            var unitTypes = new List<byte>();

            player.faith += 3;

            foreach (var unit in player.allUnits.Where(unit => !unit.isDead)
                         .Where(unit => unit.currentHex.currentTileID == 3)
                         .Where(unit => unit.playerId == player.playerId))
            {
                if (unitTypes.Contains(unit.unitScriptableId)) continue;

                unitTypes.Add(unit.unitScriptableId);
                player.faith += 3;
            }
        }

        void VoEGainFaithOnTurnEnd(PlayerSM playerSm)
        {
            if (!GameSM.IsPlayerTurn(targetPlayer)) return;

            foreach (var unit in player.allUnits.Where(unit => !unit.isDead)
                         .Where(unit => unit.currentHex.currentTileID == 3)
                         .Where(unit => unit.playerId == player.playerId))
            {
                player.faith++;
            }
        }

        void VoEDamageEnemyUnitsNearVoid(PlayerSM playerSm)
        {
            if (!GameSM.IsPlayerTurn(targetPlayer)) return;

            foreach (var unit in player.allUnits.Where(unit => !unit.isDead)
                         .Where(unit => unit.playerId != player.playerId))
            {
                var neighbours = unit.currentHex.neighbours;
                var takeDamage = false;
                foreach (var hex in neighbours)
                {
                    if (hex == null)
                    {
                        takeDamage = true;
                    }
                    else if (hex.movementCost == 127)
                    {
                        takeDamage = true;
                    }
                }

                if (!takeDamage || unit.NumberOfAdjacentEnemyUnits() == 0) continue;

                var totalDamage = Convert.ToSByte(3 * unit.NumberOfAdjacentEnemyUnits());
                unit.TakeDamage(totalDamage, 0);
            }
        }
    }
}