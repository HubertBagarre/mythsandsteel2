using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitEffect : MonoBehaviour, ICancelable
{
    sbyte physicDamageDone;
    sbyte magicDamageDone;
    sbyte totalDamage;

    public void DoAttack(Unit atkUnit, Unit defUnit)
    {
        totalDamage = 0;
        physicDamageDone = Convert.ToSByte(atkUnit.attackDamage - defUnit.physicDef);

        if (physicDamageDone >= 0) totalDamage += physicDamageDone;
        if (magicDamageDone >= 0) totalDamage += magicDamageDone;

        //defUnit.TakeDamage(totalDamage);
    }

    public enum statEnum {physicDef, magicDef, atkPerTurn, physicDamage, magicDamage, range, move, actualHp
    }

    public void ChangeStat(Unit defUnit, statEnum statToChange, sbyte modifier)
    {
        switch (statToChange)
        {
            case statEnum.physicDef:
                defUnit.physicDef += modifier;
                break;
            case statEnum.magicDef:
                defUnit.magicDef += modifier;
                break;
            case statEnum.atkPerTurn:
                if (defUnit.attacksPerTurn + modifier >= 0) defUnit.attacksPerTurn += modifier;
                break;
            case statEnum.physicDamage:
                if (defUnit.attackDamage + modifier >= 0) defUnit.attackDamage += modifier;
                break;
            case statEnum.magicDamage:
                //if (defUnit.magicDamage + modifier >= 0) defUnit.magicDamage += modifier;
                break;
            case statEnum.range:
                if (defUnit.attackRange + modifier >= 0) defUnit.attackRange += modifier;
                break;
            case statEnum.move:
                if (defUnit.move + modifier >= 0) defUnit.move += modifier;
                break;
            case statEnum.actualHp:
                //defUnit.TakeDamage(modifier);
                break;
        }
    }

    public void OnDo()
    {
        
    }

    public void OnUndo()
    {

    }
}
