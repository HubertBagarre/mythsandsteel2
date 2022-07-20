using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Karbantos")]
public class TestUnit : ScriptableUnit
{
    public override void AttackUnit(Unit attackingUnit, Unit attackedUnit)
    {
        var damage = attackingUnit.attackDamage;
        if (IsUnitSurrounded(attackedUnit))
        {
            damage = Convert.ToSByte(damage * 2);
        }

        attackedUnit.TakeDamage(damage, 0, attackingUnit);
    }

    private bool IsUnitSurrounded(Unit unit)
    {
        var currentHex = unit.currentHex;
        var enemyPlayer = Convert.ToSByte(unit.playerId == 0 ? 1 : (0));
        for (var i = 0; i < 3; i++)
        {
            var hex1 = currentHex.neighbours[i];
            var hex2 = currentHex.neighbours[i + 3];
            if (hex1 == null || hex2 == null) continue;
            if (hex1.HasUnitOfPlayer(enemyPlayer) && hex2.HasUnitOfPlayer(enemyPlayer))
                return true;
        }
        return false;
    }
}
