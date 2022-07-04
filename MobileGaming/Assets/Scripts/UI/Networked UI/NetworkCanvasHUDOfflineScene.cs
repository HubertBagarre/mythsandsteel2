using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDOfflineScene : MonoBehaviour
{
    [SerializeField] private Button buttonClient;
    private NetworkRoomPlayer roomPlayer;
    
    private void Start()
    {
        buttonClient.onClick.AddListener(ButtonClient);
    }
    
    public void ButtonClient()
    {
        NetworkManager.singleton.StartClient();
    }
}