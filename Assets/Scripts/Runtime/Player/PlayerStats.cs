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
    [SerializeField] private int maxOxygen = 45;
    [SerializeField] private int maxSanity = 100;
    [SerializeField] private int maxHealth = 100;

    private int currentOxygen;
    private int currentSanity;
    private int currentHealth; 

    Coroutine oxygenCo;
    Coroutine sanityCo;

    bool swimCheck;

    [Header("UI")]
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenText;

    private void Start()
    {
        // Initialize stats
        currentOxygen = maxOxygen;
        currentSanity = maxSanity;
        currentHealth = maxHealth;

        //B�rja med max values 
        oxygenBar.maxValue = maxOxygen;

        //S�nka sanity overtime 
        sanityCo = StartCoroutine(DecreaseStatOverTime(() => currentSanity, v => currentSanity = v, 40, 1));
    }

    private void Update()
    {
        if (!playerMovement.IsSwimming && swimCheck)
        {
            swimCheck = false;
            StopCoroutine(oxygenCo);
            currentOxygen = maxOxygen;
        }
        else if (playerMovement.IsSwimming && !swimCheck)
        {
            swimCheck = true;
            oxygenCo = StartCoroutine(DecreaseStatOverTime(() => currentOxygen, v => currentOxygen = v, 3, 3));
        }

        if (GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(oxygenTankItemData, out var ab))
        {
            maxOxygen = 75;
            oxygenBar.maxValue = 75; 
            currentOxygen = 75;
        }

        // Update UI
        oxygenBar.value = currentOxygen;
        oxygenText.text = currentOxygen.ToString();
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

    public void ChangeOxygen(int amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0, maxOxygen);
    }

    public void ChangeSanity(int amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
    }

    public void ChangeHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (currentHealth == 0) OnDeath?.Invoke();
    }
}
