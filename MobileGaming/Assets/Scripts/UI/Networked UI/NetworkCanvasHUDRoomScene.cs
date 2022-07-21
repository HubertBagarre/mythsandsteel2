using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDRoomScene : MonoBehaviour
{
    [Header("Selectors")]
    [SerializeField] private FactionSelector factionSelector;
    
    [SerializeField] private Button leaveButton, readyButton;
    [SerializeField] private TextMeshProUGUI buttonReadyText;
    private bool readyButtonState;
    private NetworkRoomPlayer roomPlayer;
    
    private void Start()
    {
        readyButtonState = false;
        leaveButton.onClick.AddListener(ButtonStop);
        readyButton.onClick.AddListener(ButtonReady);
        
    }
    
    private void AssignRoomPlayer()
    {
        var room = (NewNetworkRoomManager) NetworkManager.singleton;
        if(room == null) Debug.LogWarning("Couldn't convert the NetworkManager to a RoomManager");
        if(room.roomSlots.Count <= 0) return;
        foreach (var roomSlot in room.roomSlots)
        {
            if (roomSlot.isLocalPlayer) roomPlayer = roomSlot;
        }
    }

    private void ButtonStop()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
    }

    private void ButtonReady()
    {
        

        readyButtonState = !readyButtonState;
        buttonReadyText.text = readyButtonState ? "Not Ready" : "Ready";
        
        AssignRoomPlayer();
        roomPlayer.CmdChangeReadyState(readyButtonState);
    }
}
