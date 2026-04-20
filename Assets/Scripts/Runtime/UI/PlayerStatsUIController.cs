using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private UIPort _uiPort;
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenText;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _dyingBlur;

    void OnEnable()
    {
        _uiPort.OnSetMaxOxygen += SetMaxOxygen;
        _uiPort.UpdateOxygenUI += UpdateOxygenUI;
        _uiPort.UpdateHealthBar += UpdateHealthBar;
        _uiPort.UpdateDrownTime += UpdateDrownTime;
    }

    void OnDisable()
    {
        _uiPort.OnSetMaxOxygen -= SetMaxOxygen;
        _uiPort.UpdateOxygenUI -= UpdateOxygenUI;
        _uiPort.UpdateHealthBar -= UpdateHealthBar;
        _uiPort.UpdateDrownTime -= UpdateDrownTime;
    }

    private void SetMaxOxygen(int value)
    {
        oxygenBar.maxValue = value;
    }

    private void UpdateOxygenUI(float value)
    {
        oxygenBar.value = value;
        oxygenText.text = Mathf.RoundToInt(value).ToString();
    }

    private void UpdateHealthBar(float fillAmount)
    {
        _healthBar.fillAmount = fillAmount;
    }

    private void UpdateDrownTime(float value)
    {
        _dyingBlur.material.SetFloat("_Scale", value);
    }
}
