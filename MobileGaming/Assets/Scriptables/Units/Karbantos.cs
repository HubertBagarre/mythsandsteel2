using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit/Karbantos")]
public class Karbantos : ScriptableUnit
{
    public override void AttackUnit(Unit karbantosUnit, Unit attackedUnit)
    {
        var physicalDamage = karbantosUnit.attackDamage;
        if (attackedUnit.className == Classes.Infantry) physicalDamage += 3;

        var player = karbantosUnit.player;
        sbyte magicalDamage = 0;
        if(karbantosUnit.NumberOfAdjacentEnemyUnits() == 1)
            if (player.ConsumeFaith(3))
                magicalDamage = 2;
        
        attackedUnit.TakeDamage(physicalDamage, magicalDamage, karbantosUnit);

        if (attackedUnit.isDead) return;
        if(player.ConsumeFaith(2)) karbantosUnit.KnockBackUnit(attackedUnit,GetKnockBackDirection(karbantosUnit,attackedUnit));
    }
    

    private int GetKnockBackDirection(Unit attackingUnit,Unit attackedUnit)
    {
        for (var i = 0; i < 6; i++)
        {
            var hex = attackingUnit.currentHex.neighbours[i];
            if (hex == null) continue;
            if (hex.currentUnit == attackedUnit)
                return i;
        }

        return 0;
    }
}
