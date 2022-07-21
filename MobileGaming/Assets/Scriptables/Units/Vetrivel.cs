using System;
using System.Linq;
using CallbackManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Vetrivel")]
public class Vetrivel : ScriptableUnit
{
    public override void SetupEvents(Unit vetrivelUnit)
    {
        CallbackManager.OnAnyUnitHexExit += UpdateAdjacentVetrivelsPhysicalDef;

        CallbackManager.OnAnyUnitHexEnter += UpdateAdjacentVetrivelsPhysicalDef;
        
        void UpdateAdjacentVetrivelsPhysicalDef(Unit unit,Hex hex)
        {
            Debug.Log($"They are {hex.AdjacentUnits().Count()} adjacent units to {hex}");
            foreach (var adjacentUnit in hex.AdjacentUnits())
            {
                Debug.Log($"{adjacentUnit}, is of type {adjacentUnit.unitScriptable.GetType()}. Is Vetrivel : {adjacentUnit.unitScriptable is Vetrivel} Isn't dead : {!adjacentUnit.isDead} ");
                if (adjacentUnit.unitScriptable is Vetrivel && !adjacentUnit.isDead)
                {
                    Debug.Log($"Number of enemy unit adjacent to {adjacentUnit} : {adjacentUnit.NumberOfAdjacentEnemyUnits()}");
                    adjacentUnit.physicDef = Convert.ToSByte(adjacentUnit.basePhysicDef + adjacentUnit.NumberOfAdjacentEnemyUnits());
                }
            }
            if (unit.unitScriptable is Vetrivel && !unit.isDead)
            {
                Debug.Log($"Number of enemy unit adjacent to {unit} : {unit.NumberOfAdjacentEnemyUnits()}");
                unit.physicDef = Convert.ToSByte(unit.basePhysicDef + unit.NumberOfAdjacentEnemyUnits());
            }
        }
    }

    public override void AttackUnit(Unit vetrivelUnit, Unit attackedUnit)
    {
        var physicalDamage = vetrivelUnit.attackDamage;
        if (attackedUnit.className == Classes.Cavalry) physicalDamage += 3;
        
        var player = vetrivelUnit.player;
        physicalDamage = vetrivelUnit.AdjacentUnits().Where(unit => player.ConsumeFaith(1)).Aggregate(physicalDamage, (current, unit) => (sbyte) (current + 1));

        attackedUnit.TakeDamage(physicalDamage, 0, vetrivelUnit);
    }
}
