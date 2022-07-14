using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Ability")]
public class ScriptableAbility : ScriptableObject
{
    public virtual void OnAttackTriggered(Unit attackingUnit,Unit attackedUnit) { }
}
