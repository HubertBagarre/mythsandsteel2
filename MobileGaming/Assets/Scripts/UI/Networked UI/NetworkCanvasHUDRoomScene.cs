using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDRoomScene : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnShowChanged))] private bool show = true;

    [SerializeField] private Button leaveButton, readyButton;
    [SerializeField] private TextMeshProUGUI buttonReadyText,playerListText;
    [SerializeField] private GameObject canvasGameObject;
    private bool readyButtonState;
    private NetworkRoomPlayer roomPlayer;
    
    
    private void Start()
    {
        if (!isLocalPlayer || !show)
        {
            canvasGameObject.SetActive(false);
            return;
        }
        readyButtonState = false;
        leaveButton.onClick.AddListener(ButtonStop);
        readyButton.onClick.AddListener(ButtonReady);
    }
    
    private void AssignRoomPlayer()
    {
        var room = (NewNetworkRoomManager) NetworkManager.singleton;
        if(room == null) Debug.LogWarning("Couldn't convert the NetworkManager to a RoomManager");
        if(room.roomSlots.Count <= 0) return;
        foreach (var roomSlot in room.roomSlots.Where(roomSlot => roomSlot.isLocalPlayer))
        {
            roomPlayer = roomSlot;
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

    public void SetRoomHUDActive(bool value)
    {
        show = value;
        canvasGameObject.SetActive(value);
    }

    private void OnShowChanged(bool prevValue, bool newValue)
    {
        canvasGameObject.SetActive(newValue);
    }

    public void UpdatePlayerList(List<NetworkRoomPlayer> players)
    {
        var msg = string.Empty;
        if (players.Count <= 0)
        {
            playerListText.text = msg;
            return;
        }

        for (var index = 0; index < players.Count; index++)
        {
            msg += $"Player {index}";
            if(players[index].netId == netId) msg += " (vous)";
            msg += "\n";
        }
        
        playerListText.text = msg;
    }
}
