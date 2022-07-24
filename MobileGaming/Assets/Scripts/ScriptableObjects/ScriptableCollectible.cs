using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableCollectible : ScriptableObject
{
    public GameObject collectibleModelPrefab;
    
    public virtual void OnPickedUp(Unit unitThatPickedUp,Hex hexWithThisCollectible){}
}
