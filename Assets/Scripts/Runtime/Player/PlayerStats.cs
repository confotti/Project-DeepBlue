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
    [SerializeField] private float _drownReduction = 2.5f;

    [SerializeField] private int _maxSanity = 100;
    [SerializeField] private int _maxHealth = 100;

    private float _currentOxygen;
    private float _currentDrownTime;
    private int _currentSanity;
    private int _currentHealth;

    private bool _dead = false;

    [Header("UI")]
    [SerializeField] private UIPort _uiPort;
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenText;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _dyingBlur;

    private void Start()
    {
        // Initialize stats
        _currentOxygen = _maxOxygen;
        _currentSanity = _maxSanity;
        _currentHealth = _maxHealth;

        //B�rja med max values 
        oxygenBar.maxValue = _maxOxygen;
    }

    private void OnDestroy()
    {
        if(_dyingBlur) _dyingBlur.material.SetFloat("_Scale", 3);
    }

    private void Update()
    {
        if(!playerMovement.IsSwimming)
        {
            ChangeOxygen(_oxygenGainPerSecond * Time.deltaTime);
        }
        else if(playerMovement.IsSwimming)
        {
            ChangeOxygen(-Time.deltaTime);
        }

        ChangeDrownTime(_currentOxygen == 0 ? Time.deltaTime : -Time.deltaTime * _drownReduction);

        if(GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(oxygenTankItemData, out var ab))
        {
            _maxOxygen = 75;
            oxygenBar.maxValue = 75;
        }

        // Update UI
        oxygenBar.value = _currentOxygen;
        oxygenText.text = Mathf.RoundToInt(_currentOxygen).ToString();
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
        if(_healthBar) _healthBar.fillAmount = _currentHealth / (float)_maxHealth;
        if(_currentHealth == 0) Death();
    }

    public void ChangeDrownTime(float amount)
    {
        _currentDrownTime = Mathf.Clamp(_currentDrownTime + amount, 0, _timeToDrown);
        if(_dyingBlur) _dyingBlur.material.SetFloat("_Scale", (1 - (_currentDrownTime / _timeToDrown)) * 3);
        if(_currentDrownTime == _timeToDrown)
        {
            Death();
        } 
    }

    private void Death()
    {
        _dead = true;
        OnDeath?.Invoke();
        //This has to disable player controls somehow

        _uiPort.StartScreenFade(true, 2, DeathFadeOutDone);
    }

    private void DeathFadeOutDone()
    {
        //Respawn here. 
        StartCoroutine(FadeWait(1));
    }
    
    private void DeathFadeBackDone()
    {
        //Re-enable player controls and stuff. 
    }

    private IEnumerator FadeWait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        _uiPort.StartScreenFade(false, 1, DeathFadeBackDone);
    }

    /*
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

    oxygenCo = StartCoroutine(DecreaseStatOverTime(() => _currentOxygen, v => _currentOxygen = v, 3, 3));
    */
}
