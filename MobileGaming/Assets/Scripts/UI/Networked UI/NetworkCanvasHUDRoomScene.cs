using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvasHUDRoomScene : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnShowChanged))] private bool show = true;

    [Header("Global")]
    [SerializeField] private LayoutSpawner spawner;
    [SerializeField] private Button leftArrowButton,rightArrowButton,returnButton;
    [SerializeField] private GameObject leftArrowGameObject,rightArrowGameObject,canvasGameObject;
    [SerializeField] private RectTransform screenParent;

    [Header("Faction Selection Screen")]
    [SerializeField] private TextMeshProUGUI factionTitleText;
    [SerializeField] private TextMeshProUGUI factionBelieverAbilityText,factionCulturalAbilityText,factionLoreText,factionStrategyText;
    [SerializeField] private GameObject factionLoreInfoGameObject, factionStrategyInfoGameObject;
    [SerializeField] private Image factionIllustrationImage;
    [SerializeField] private Button leaveButton, factionLoreButton, factionStrategyButton, factionLoreBoxButton, factionStrategyBoxButton;
    
    [Header("Unit Placement Selection Screen")]
    [SerializeField] private TextMeshProUGUI unitFormationTitleText;
    [SerializeField] private GameObject unitFormationDetailsBoxGameObject;
    [SerializeField] private Button unitFormationDetailsButton,unitFormationDetailsBoxButton;
    [SerializeField] private Image unitFormationPreviewImage,unitFormationIllustrationImage;


    [SerializeField] private TextMeshProUGUI buttonReadyText,playerListText;
    [SerializeField] private Button readyButton;
    
    private int currentScreen;
    private bool readyButtonState;
    private NetworkRoomPlayer roomPlayer;
    
    private void Start()
    {
        if (!isLocalPlayer || !show)
        {
            canvasGameObject.SetActive(false);
            return;
        }

        currentScreen = 0;
        
        rightArrowButton.onClick.AddListener(GoToNextScreen);
        leftArrowButton.onClick.AddListener(GoToPreviousScreen);
        returnButton.onClick.AddListener(GoToPreviousScreen);
        
        factionLoreButton.onClick.AddListener(ToggleFactionLore);
        factionLoreBoxButton.onClick.AddListener(ToggleFactionLore);
        factionStrategyButton.onClick.AddListener(ToggleFactionStrategy);
        factionStrategyBoxButton.onClick.AddListener(ToggleFactionStrategy);
        
        unitFormationDetailsButton.onClick.AddListener(ToggleShowUnitsInFormationBox);
        unitFormationDetailsBoxButton.onClick.AddListener(ToggleShowUnitsInFormationBox);

        leaveButton.onClick.AddListener(ButtonStop);
        
        UpdateScreenSideButtons();

        readyButtonState = false;
        
        
        readyButton.onClick.AddListener(ButtonReady);
    }

    private void GoToNextScreen()
    {
        currentScreen++;
        UpdateScreenSideButtons();
    }

    private void GoToPreviousScreen()
    {
        currentScreen--;
        UpdateScreenSideButtons();
    }

    private void UpdateScreenSideButtons()
    {
        screenParent.DOLocalMove(Vector3.right * currentScreen * -1920,0.33f);
        
        leftArrowGameObject.SetActive(currentScreen>0);
        rightArrowGameObject.SetActive(currentScreen<2);
        
        factionStrategyInfoGameObject.SetActive(false);
        factionLoreInfoGameObject.SetActive(false);
        unitFormationDetailsBoxGameObject.SetActive(false);
    }

    #region Faction Screen

    public void OnFactionSelected(int index)
    {
        var faction = ObjectIDList.GetFactionScriptable(index);
        factionTitleText.text = faction.name;

        factionBelieverAbilityText.text = faction.beliefAbilityDescription;
        factionCulturalAbilityText.text = faction.culturalAbilityDescription;
        factionLoreText.text = faction.loreDescription;
        factionStrategyText.text = faction.strategyDescription;
        factionIllustrationImage.sprite = faction.factionIllustration;
    }

    private void ToggleFactionLore()
    {
        factionStrategyInfoGameObject.SetActive(false);
        factionLoreInfoGameObject.SetActive(!factionLoreInfoGameObject.activeSelf);
    }

    private void ToggleFactionStrategy()
    {
        factionLoreInfoGameObject.SetActive(false);
        factionStrategyInfoGameObject.SetActive(!factionStrategyInfoGameObject.activeSelf);
    }

    #endregion

    #region Unit Formation

    public void OnFormationSelected(int index)
    {
        var placement = ObjectIDList.GetUnitPlacementScriptable(index);

        unitFormationTitleText.text = placement.name;
        unitFormationPreviewImage.sprite = placement.placementPreview;
        unitFormationIllustrationImage.sprite = placement.placementIllustration;
        
        spawner.UpdateUnitFormationDetails(index);
    }
    
    private void ToggleShowUnitsInFormationBox()
    {
        unitFormationDetailsBoxGameObject.SetActive(!unitFormationDetailsBoxGameObject.activeSelf);
    }

    #endregion



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
        return;
        
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
