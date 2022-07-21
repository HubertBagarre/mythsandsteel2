using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Vetrivel")]
public class Vetrivel : ScriptableUnit
{
    public override void AttackUnit(Unit vetrivelUnit, Unit attackedUnit)
    {
        var physicalDamage = vetrivelUnit.attackDamage;
        if (attackedUnit.className == Classes.Cavalry) physicalDamage += 3;
        
        var player = vetrivelUnit.player;
        physicalDamage = vetrivelUnit.AdjacentUnits().Where(unit => player.ConsumeFaith(1)).Aggregate(physicalDamage, (current, unit) => (sbyte) (current + 1));

        attackedUnit.TakeDamage(physicalDamage, 0, vetrivelUnit);
    }
}
