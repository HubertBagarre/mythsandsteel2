using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Lilu-Angal")]
public class LiluAngal : ScriptableUnit
{
    public override void AttackUnit(Unit liluAngalUnit, Unit attackedUnit)
    {
        var damage = liluAngalUnit.attackDamage;

        if (attackedUnit.className == Classes.AntiCavalry) damage += 2;
        
        if (attackedUnit.IsAlignBetweenEnemies()) damage = Convert.ToSByte(damage * 2);

        attackedUnit.TakeDamage(damage, 0, liluAngalUnit);
    }
}
