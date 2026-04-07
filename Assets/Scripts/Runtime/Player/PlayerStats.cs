using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerStats : MonoBehaviour
{
    public Action OnDeath;

    private PlayerMovement _playerMovement;

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

    [SerializeField] private int _maxSanity = 100;
    [SerializeField] private int _maxHealth = 100;

    private float _currentOxygen;
    private float _previousOxygen;
    private float _currentDrownTime;
    private int _currentHealth;

    [Header("Sanity")]
    [SerializeField] private Volume sanityVolume;
    [SerializeField] private float sanityEffectStart = 30f;
    [SerializeField] private float sanityLerpSpeed = 1.5f;

    [SerializeField] private int sanityDamagePerTick = 5;
    [SerializeField] private float sanityDamageInterval = 1.5f;

    [SerializeField] private float sanityDrainPerSecond = 2f;
    [SerializeField] private float sanityRecoveryPerSecond = 1.5f;

    [SerializeField, InspectorReadOnly]
    private float _currentSanity;

    private float sanityDamageTimer;
    private float targetWeight;

    [Header("Sanity Heartbeat")]
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private float heartbeatLowSanity = 25f;

    [Header("UI")]
    [SerializeField] private UIPort _uiPort;
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenText;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _dyingBlur;

    [Header("Warnings")]
    [SerializeField] private TextMeshProUGUI lowOxygenWarning;
    [SerializeField] private TextMeshProUGUI criticalOxygenText;

    private Coroutine _lowWarningRoutine;
    private Coroutine _criticalWarningRoutine;

    private bool _dead = false;
    private bool warningLightsActive = false;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _currentMaxOxygen = _baseMaxOxygen;
    }

    private void Start()
    {
        InitializeStats();
    }

    private void Update()
    {
        HandleSanity();
        HandleOxygen();
        HandleUI();
    }

    private void OnEnable()
    {
        LightBehaviour.OnWarningLightsChanged += OnWarningLightsChanged;
    }

    private void OnDisable()
    {
        LightBehaviour.OnWarningLightsChanged -= OnWarningLightsChanged;
    }

    private void InitializeStats()
    {
        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;

        _currentSanity = _maxSanity;
        _currentHealth = _maxHealth;

        oxygenBar.maxValue = _currentMaxOxygen;

        if (lowOxygenWarning)
            lowOxygenWarning.gameObject.SetActive(false);

        if (criticalOxygenText)
            criticalOxygenText.gameObject.SetActive(false);
    }

    private void HandleSanity()
    {
        HandleSanityDrain();
        HandleHeartbeat();
        HandleSanityDamage();
        UpdateSanityEffects();
    }

    private void HandleSanityDrain()
    {
        if (IsNight() && warningLightsActive)
            ChangeSanity(-sanityDrainPerSecond * Time.deltaTime);
        else
            ChangeSanity(sanityRecoveryPerSecond * Time.deltaTime);
    }

    private void HandleSanityDamage()
    {
        if (_currentSanity > 0)
        {
            sanityDamageTimer = 0f;
            return;
        }

        sanityDamageTimer += Time.deltaTime;

        if (sanityDamageTimer >= sanityDamageInterval)
        {
            sanityDamageTimer = 0f;
            ChangeHealth(-sanityDamagePerTick);
        }
    }

    private void HandleHeartbeat()
    {
        if (_currentSanity <= heartbeatLowSanity && !_dead)
        {
            if (!heartbeatSource.isPlaying)
                heartbeatSource.Play();

            float intensity = 1f - (_currentSanity / heartbeatLowSanity);
            heartbeatSource.pitch = Mathf.Lerp(1f, 1.5f, intensity);
            heartbeatSource.volume = Mathf.Lerp(0.2f, 1f, intensity);
        }
        else
        {
            if (heartbeatSource.isPlaying)
                heartbeatSource.Stop();
        }
    }

    private void UpdateSanityEffects()
    {
        if (sanityVolume == null) return;

        if (_dead)
        {
            sanityVolume.weight = 0f;
            return;
        }

        if (_currentSanity <= sanityEffectStart && warningLightsActive)
        {
            float t = 1f - (_currentSanity / sanityEffectStart);
            targetWeight = t;
        }
        else
        {
            targetWeight = 0f;
        }

        sanityVolume.weight = Mathf.Lerp(
            sanityVolume.weight,
            targetWeight,
            Time.deltaTime * sanityLerpSpeed
        );
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
        UpdateOxygenUI();
        HandleOxygenWarnings();
    }

    private void UpdateOxygenUI()
    {
        oxygenBar.value = _currentOxygen;
        oxygenText.text = Mathf.RoundToInt(_currentOxygen).ToString();
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

        if (_healthBar)
            _healthBar.fillAmount = _currentHealth / (float)_maxHealth;

        if (_currentHealth == 0)
            Death();
    }

    public void SetHealth(int amount)
    {
        _currentHealth = Mathf.Clamp(amount, 0, _maxHealth);

        if (_healthBar)
            _healthBar.fillAmount = _currentHealth / (float)_maxHealth;

        if (_currentHealth == 0)
            Death();
    }

    public void ChangeSanity(float amount)
    {
        _currentSanity = Mathf.Clamp(_currentSanity + amount, 0, _maxSanity);
    }

    public void SetOxygenModifier(int amountAdded)
    {
        _currentMaxOxygen = _baseMaxOxygen + amountAdded;
        oxygenBar.maxValue = _currentMaxOxygen;

        _currentOxygen = Mathf.Clamp(_currentOxygen, 0, _currentMaxOxygen);
    }

    private void ChangeOxygen(float amount)
    {
        _currentOxygen = Mathf.Clamp(_currentOxygen + amount, 0, _currentMaxOxygen);
    }

    private void ChangeDrownTime(float amount)
    {
        _currentDrownTime = Mathf.Clamp(_currentDrownTime + amount, 0, _timeToDrown);

        if (_dyingBlur)
            _dyingBlur.material.SetFloat("_Scale", (1 - (_currentDrownTime / _timeToDrown)) * 3);

        if (_currentDrownTime == _timeToDrown)
            Death();
    }

    private void Death()
    {
        if (_dead) return;

        _dead = true;

        // Reset sanity
        _currentSanity = _maxSanity;

        // Stop effects
        if (heartbeatSource && heartbeatSource.isPlaying)
            heartbeatSource.Stop();

        if (sanityVolume)
            sanityVolume.weight = 0f;

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

        _playerMovement.StateMachine.ChangeState(_playerMovement.StandingState);

        SetHealth(_maxHealth);

        _currentOxygen = _currentMaxOxygen;
        _previousOxygen = _currentOxygen;
        _currentDrownTime = 0;
    }

    private bool IsNight()
    {
        if (TimeManager.Instance == null) return false;

        int hour = TimeManager.Instance.GetGameTimeStamp().Hour;
        return (hour >= 22 || hour < 6);
    }

    private void OnWarningLightsChanged(bool active)
    {
        warningLightsActive = active;
    } 
} 