using UnityEngine;
using UnityEngine.UI;

public class Building_UI : MonoBehaviour
{
    private Button _button;
    private BuildingData _assignedData;
    private BuildDisplay _display;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void Init(BuildingData data)
    {
        _assignedData = data;
        _button.transform.GetChild(0).GetComponent<Image>().sprite = _assignedData.Icon;
    }

    private void OnButtonClicked()
    {
        BuildDisplay.OnPartChosen?.Invoke(_assignedData);
    }
}
