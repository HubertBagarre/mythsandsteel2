using Mirror;

public class LobbyInfoContainer : NetworkBehaviour
{
    [SyncVar] public int factionIndex;
    [SyncVar] public int unitPlacementIndex;

    public delegate void EpicEvent(int number);
    
    public static EpicEvent onFactionChose;
    public static EpicEvent onUnitSelectionChose;

    private void Start()
    {
        if(isLocalPlayer) onFactionChose = CmdSetFaction;
        if(isLocalPlayer) onUnitSelectionChose = CmdSetUnitPlacement;
    }

    [Command]
    public void CmdSetFaction(int index)
    {
        factionIndex = index;
    }
    
    [Command]
    public void CmdSetUnitPlacement(int index)
    {
        unitPlacementIndex = index;
    }
}
