using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDGameScene : MonoBehaviour
{
    [SerializeField] private Button[] leaveButtons;
    
    private void Start()
    {
        foreach (var leaveButton in leaveButtons)
        {
            leaveButton.onClick.AddListener(ButtonStop);
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
}
