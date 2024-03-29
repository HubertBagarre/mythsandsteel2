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
    }
    
    public override void AttackUnit(Unit vetrivelUnit, Unit attackedUnit)
    {
        var physicalDamage = vetrivelUnit.attackDamage;
        if (attackedUnit.className == Classes.Cavalry) physicalDamage += 3;
        
        var player = vetrivelUnit.player;
        physicalDamage = vetrivelUnit.AdjacentUnits().Where(unit => player.ConsumeFaith(1)).Aggregate(physicalDamage, (current, unit) => (sbyte) (current + 1));

        attackedUnit.TakeDamage(physicalDamage, 0, vetrivelUnit);
    }

    public override void KillUnit(Unit killedUnit, bool physicalDeath, bool magicalDeath, Unit killer)
    {
        Debug.Log($"Killed unit hex in scriptable : {killedUnit.currentHex}");
        
        CallbackManager.OnAnyUnitHexExit -= UpdateAdjacentVetrivelsPhysicalDef;

        CallbackManager.OnAnyUnitHexEnter -= UpdateAdjacentVetrivelsPhysicalDef;
        
        base.KillUnit(killedUnit, physicalDeath, magicalDeath, killer);
    }
    
    private void UpdateAdjacentVetrivelsPhysicalDef(Unit unit,Hex hex)
    {
        foreach (var adjacentUnit in hex.AdjacentUnits())
        {
            if (adjacentUnit.unitScriptable is Vetrivel && !adjacentUnit.isDead)
            {
                adjacentUnit.physicDef = Convert.ToSByte(adjacentUnit.basePhysicDef + adjacentUnit.NumberOfAdjacentEnemyUnits());
            }
        }
        if (unit.unitScriptable is Vetrivel && !unit.isDead)
        {
            unit.physicDef = Convert.ToSByte(unit.basePhysicDef + unit.NumberOfAdjacentEnemyUnits());
        }
    }
}
