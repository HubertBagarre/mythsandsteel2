using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Buff Info")]
public class ScriptableBuffInfo : ScriptableObject
{
    public Sprite icon;
    
    [TextArea(3,5)]
    public string description;
}
