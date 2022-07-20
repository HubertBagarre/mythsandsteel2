using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Lilu-Angal")]
public class LiluAngalF : ScriptableUnit
{
    public override void AttackUnit(Unit attackingUnit, Unit attackedUnit)
    {
        var damage = attackingUnit.attackDamage;

        if (attackedUnit.className == Classes.AntiCavalry) damage += 2;
        
        if (attackedUnit.IsAlignBetweenEnemies()) damage = Convert.ToSByte(damage * 2);

        attackedUnit.TakeDamage(damage, 0, attackingUnit);
    }
}
