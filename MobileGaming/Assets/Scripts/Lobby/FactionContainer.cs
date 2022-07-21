using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FactionContainer : NetworkBehaviour
{
    [SyncVar] public int factionIndex;

    public delegate void EpicEvent(int number);
    
    public static EpicEvent onFactionChose;

    private void Start()
    {
        if(isLocalPlayer) onFactionChose = CmdSetFaction;
    }

    [Command]
    public void CmdSetFaction(int index)
    {
        Debug.Log($"Hello from {netId}");
        factionIndex = index;
    }
}
