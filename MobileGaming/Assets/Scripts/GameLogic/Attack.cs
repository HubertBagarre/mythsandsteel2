using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Attack : MonoBehaviour, ICancelable
{
    sbyte physicDamageDone;
    sbyte magicDamageDone;
    sbyte totalDamage;

    public void DoAttack(Unit atkUnit, Unit defUnit)
    {
        totalDamage = 0;
        physicDamageDone = Convert.ToSByte(atkUnit.physicDamage - defUnit.physicDef);
        magicDamageDone = Convert.ToSByte(atkUnit.magicDamage - defUnit.magicDef);

        if (physicDamageDone >= 0) totalDamage += physicDamageDone;
        if (magicDamageDone >= 0) totalDamage += magicDamageDone;

        defUnit.TakeDamage(totalDamage);
    }

    public void OnDo()
    {
        
    }

    public void OnUndo()
    {

    }
}
