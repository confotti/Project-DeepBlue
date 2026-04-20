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
    [SerializeField] private GameObject _submarine;
    [SerializeField] private Vector3 _respawnPositionOffset;
    private Vector3 RespawnPoint => _submarine.transform.position + _respawnPositionOffset;

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

    [Header("Warnings")]
    [SerializeField] private TextMeshProUGUI lowOxygenWarning;
    [SerializeField] private TextMeshProUGUI criticalOxygenText;

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
        HandleOxygen();
        HandleUI();
    }

    private void InitializeStats()
    {
        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;

        _currentHealth = _maxHealth;

        _uiPort.OnSetMaxOxygen?.Invoke(_currentMaxOxygen);

        if (lowOxygenWarning)
            lowOxygenWarning.gameObject.SetActive(false);

        if (criticalOxygenText)
            criticalOxygenText.gameObject.SetActive(false);
    }

    private void HandleOxygen()
    {
        if (!_playerMovement.IsSwimming)
            ChangeOxygen(_oxygenGainPerSecond * Time.deltaTime);
        else
            ChangeOxygen(-Time.deltaTime);

        ChangeDrownTime(_currentOxygen == 0
            ? Time.deltaTime
            : -Time.deltaTime * _drownReduction);
    }

    private void HandleUI()
    {
        _uiPort.UpdateOxygenUI?.Invoke(_currentOxygen);
        HandleOxygenWarnings();
    }

    private void HandleOxygenWarnings()
    {
        bool swimming = _playerMovement.IsSwimming;

        if (_previousOxygen > 30f && _currentOxygen <= 30f && swimming)
        {
            if (_lowWarningRoutine != null)
                StopCoroutine(_lowWarningRoutine);

            _lowWarningRoutine = StartCoroutine(ShowLowOxygenWarning());
        }

        if (_previousOxygen > 15f && _currentOxygen <= 15f && swimming)
        {
            if (_criticalWarningRoutine != null)
                StopCoroutine(_criticalWarningRoutine);

            _criticalWarningRoutine = StartCoroutine(ShowCriticalOxygenWarning());
        }

        _previousOxygen = _currentOxygen;
    }

    private IEnumerator ShowLowOxygenWarning()
    {
        if (!lowOxygenWarning) yield break;

        lowOxygenWarning.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        lowOxygenWarning.gameObject.SetActive(false);
    }

    private IEnumerator ShowCriticalOxygenWarning()
    {
        if (!criticalOxygenText) yield break;

        criticalOxygenText.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < 4f)
        {
            Color color = Color.Lerp(Color.red, Color.black, Mathf.PingPong(Time.time * 6f, 1f));
            criticalOxygenText.color = color;

            timer += Time.deltaTime;
            yield return null;
        }

        criticalOxygenText.gameObject.SetActive(false);
    }

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
        _uiPort.StartScreenFade(true, 2, DeathFadeOutDone);
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
        transform.position = RespawnPoint;
        _dead = false;

        _sanityManager.SetDead(false);

        _playerMovement.StateMachine.ChangeState(_playerMovement.StandingState);

        SetHealth(_maxHealth);

        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;
        _currentDrownTime = 0;
    }
} 