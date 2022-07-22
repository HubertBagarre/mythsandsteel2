using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FactionButton : MonoBehaviour
{
    public int factionIndex;
    public FactionAndPlacementSelector selector;
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        button.interactable = false;
        selector.SelectFaction(factionIndex);
        LobbyInfoContainer.onFactionChose?.Invoke(factionIndex);
    }
}
