using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDRoomScene : MonoBehaviour
{
    [SerializeField] private Button leaveButton, readyButton;
    [SerializeField] private Toggle deckToggle, mapToggle;
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
    
    public void ButtonStop()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
    }

    public void ButtonReady()
    {
        if (!deckToggle.isOn)
        {
            Debug.Log("Please select a Deck");
            return;
        }
            
        if(!mapToggle.isOn)
        {
            Debug.Log("Please select a Map");
            return;
        }

        readyButtonState = !readyButtonState;
        buttonReadyText.text = readyButtonState ? "Not Ready" : "Ready";
        
        AssignRoomPlayer();
        roomPlayer.CmdChangeReadyState(readyButtonState);
    }
}
