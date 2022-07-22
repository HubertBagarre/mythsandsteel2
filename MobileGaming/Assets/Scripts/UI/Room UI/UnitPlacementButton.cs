using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UnitPlacementButton : MonoBehaviour
{
    public int unitPlacementIndex;
    public FactionAndPlacementSelector selector;
    public Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        text.text = ObjectIDList.GetUnitPlacementScriptable(unitPlacementIndex).name;
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        button.interactable = false;
        selector.SelectUnitPlacement(unitPlacementIndex);
        LobbyInfoContainer.onUnitSelectionChose?.Invoke(unitPlacementIndex);
    }
}