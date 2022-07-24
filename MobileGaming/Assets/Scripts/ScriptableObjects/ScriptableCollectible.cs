using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableCollectible : ScriptableObject
{
    public virtual void OnPickedUp(Unit unitThatPickedUp,Hex hexWithThisCollectible){}
}
