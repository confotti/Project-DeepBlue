using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class PlayerStats : MonoBehaviour
{
    public Action OnDeath;

    private PlayerMovement _playerMovement;
    private SanityManager _sanityManager;

    [Header("Respawn")]
    [SerializeField] private Transform _respawnPoint; 

    [Header("Stats")]
    [SerializeField] private int _baseMaxOxygen = 60;
    [SerializeField, InspectorReadOnly] private int _currentMaxOxygen;
    [SerializeField] private float _oxygenGainPerSecond = 50;
    [SerializeField] private float _timeToDrown = 5;
    [SerializeField] private float _drownReduction = 2.5f;

    [SerializeField] private int _maxHealth = 100;

    [Header("UI")]
    [SerializeField] private UIPort _uiPort;

    private float _currentOxygen;
    private float _previousOxygen;
    private float _currentDrownTime;
    private int _currentHealth;

    private Coroutine _lowWarningRoutine;
    private Coroutine _criticalWarningRoutine;

    private bool _dead = false;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _sanityManager = GetComponent<SanityManager>();
        _currentMaxOxygen = _baseMaxOxygen;
    }

    private void Start()
    {
        InitializeStats();
    }

    private void Update()
    {
        //*HandleOxygen();
    }

    private void InitializeStats()
    {
        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;

        _currentHealth = _maxHealth;

        _uiPort.OnSetMaxOxygen?.Invoke(_currentMaxOxygen);
    }

    /*private void HandleOxygen()
    {
        if (!_playerMovement.IsSwimming)
            ChangeOxygen(_oxygenGainPerSecond * Time.deltaTime);
        else
            ChangeOxygen(-Time.deltaTime);

        ChangeDrownTime(_currentOxygen == 0
            ? Time.deltaTime
            : -Time.deltaTime * _drownReduction);
    } */

    public void ChangeHealth(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);

        _uiPort.UpdateHealthBar?.Invoke(_currentHealth / (float)_maxHealth);

        if (_currentHealth == 0)
            Death();
    }

    public void SetHealth(int amount)
    {
        _currentHealth = Mathf.Clamp(amount, 0, _maxHealth);

        _uiPort.UpdateHealthBar?.Invoke(_currentHealth / (float)_maxHealth);

        if (_currentHealth == 0)
            Death();
    }

    public void SetOxygenModifier(int amountAdded)
    {
        _currentMaxOxygen = _baseMaxOxygen + amountAdded;
        _uiPort.OnSetMaxOxygen?.Invoke(_currentMaxOxygen);

        _currentOxygen = Mathf.Clamp(_currentOxygen, 0, _currentMaxOxygen);
    }

    private void ChangeOxygen(float amount)
    {
        _currentOxygen = Mathf.Clamp(_currentOxygen + amount, 0, _currentMaxOxygen);
    }

    private void ChangeDrownTime(float amount)
    {
        _currentDrownTime = Mathf.Clamp(_currentDrownTime + amount, 0, _timeToDrown);

        _uiPort.UpdateDrownTime?.Invoke((1 - (_currentDrownTime / _timeToDrown)) * 3);

        if (_currentDrownTime == _timeToDrown)
            Death();
    }

    private void Death()
    {
        if (_dead) return;

        _dead = true;

        _sanityManager.ResetSanity();
        _sanityManager.SetDead(true);

        OnDeath?.Invoke();
        _uiPort.StartScreenFade(true, 0.2f, DeathFadeOutDone);
    }

    private void DeathFadeOutDone()
    {
        StartCoroutine(FadeWait(1));
        Respawn();
    }

    private IEnumerator FadeWait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        _uiPort.StartScreenFade(false, 1, null);
    }

    private void Respawn()
    {
        if (_respawnPoint != null)
        {
            transform.position = _respawnPoint.position;
            transform.rotation = _respawnPoint.rotation; // Optional
        }
        else
        {
            Debug.LogWarning("No respawn point assigned!");
        } 
        _dead = false;

        _sanityManager.SetDead(false);

        _playerMovement.StateMachine.ChangeState(_playerMovement.StandingState);

        SetHealth(_maxHealth);

        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;
        _currentDrownTime = 0;
    }
} 