using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NaughtyAttributes;

public class TestSyncVar : NetworkBehaviour
{
    [SyncVar(hook = nameof(Say)),ReadOnly] public sbyte value;
    public KeyCode keycode;
    
    private void Say(sbyte prevV,sbyte newV)    
    {
        if(!isLocalPlayer) return;
        Debug.Log($"Changed value, was {prevV}, now is {newV}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(keycode)) value++;
    }
}
