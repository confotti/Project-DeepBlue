using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class PlayerStats : MonoBehaviour
{
    public Action OnDeath;

    public InventoryItemData oxygenTankItemData;

    [SerializeField] private PlayerMovement playerMovement;

    [Header("Stats")]
    [SerializeField] private int _maxOxygen = 45;
    [SerializeField] private float _oxygenGainPerSecond = 50;
    [SerializeField] private float _timeToDrown = 5;
    [SerializeField] private int _maxSanity = 100;
    [SerializeField] private int _maxHealth = 100;

    private float _currentOxygen;
    private float _currentDrownTime;
    private int _currentSanity;
    private int _currentHealth;

    private bool _dead = false;

    Coroutine oxygenCo;
    Coroutine sanityCo;

    [Header("UI")]
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenText;
    [SerializeField] private Image _dyingBlur;

    private void Start()
    {
        // Initialize stats
        _currentOxygen = _maxOxygen;
        _currentSanity = _maxSanity;
        _currentHealth = _maxHealth;

        //B�rja med max values 
        oxygenBar.maxValue = _maxOxygen;

        //S�nka sanity overtime 
        //sanityCo = StartCoroutine(DecreaseStatOverTime(() => _currentSanity, v => _currentSanity = v, 40, 1));
    }

    private void OnDestroy()
    {
        _dyingBlur.material.SetFloat("_Scale", 3);
    }

    private void Update()
    {
        if (!playerMovement.IsSwimming)
        {
            //StopCoroutine(oxygenCo);
            ChangeOxygen(_oxygenGainPerSecond * Time.deltaTime);
        }
        else if (playerMovement.IsSwimming)
        {
            //oxygenCo = StartCoroutine(DecreaseStatOverTime(() => _currentOxygen, v => _currentOxygen = v, 3, 3));
            ChangeOxygen(-Time.deltaTime);
        }

        ChangeDrownTime(_currentOxygen == 0 ? Time.deltaTime : -Time.deltaTime * 2.5f);

        if (GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(oxygenTankItemData, out var ab))
        {
            _maxOxygen = 75;
            oxygenBar.maxValue = 75;
        }

        // Update UI
        oxygenBar.value = _currentOxygen;
        oxygenText.text = Mathf.RoundToInt(_currentOxygen).ToString();
    }

    private IEnumerator DecreaseStatOverTime(System.Func<int> getter, System.Action<int> setter, int interval, int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            int current = getter();
            if (current > 0)
                setter(Mathf.Max(current - amount, 0));
        }
    }

    public void ChangeOxygen(float amount)
    {
        _currentOxygen = Mathf.Clamp(_currentOxygen + amount, 0, _maxOxygen);
    }

    public void ChangeSanity(int amount)
    {
        _currentSanity = Mathf.Clamp(_currentSanity + amount, 0, _maxSanity);
    }

    public void ChangeHealth(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
        if (_currentHealth == 0) OnDeath?.Invoke();
    }

    public void ChangeDrownTime(float amount)
    {
        _currentDrownTime = Mathf.Clamp(_currentDrownTime + amount, 0, _timeToDrown);
        _dyingBlur.material.SetFloat("_Scale", (1 - (_currentDrownTime / _timeToDrown)) * 3);
        if (_currentDrownTime == _timeToDrown)
        {
            _dead = true;
            OnDeath?.Invoke();
        } 
    }
    
}
