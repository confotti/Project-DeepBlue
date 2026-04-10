using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SanityManager : MonoBehaviour
{
    private PlayerStats _playerStats;

    [Header("Sanity")]
    [SerializeField] private int _maxSanity = 100;

    [SerializeField] private Volume sanityVolume;
    [SerializeField] private float sanityEffectStart = 30f;
    [SerializeField] private float sanityLerpSpeed = 1.5f;

    [SerializeField] private int sanityDamagePerTick = 5;
    [SerializeField] private float sanityDamageInterval = 1.5f;

    [SerializeField] private float sanityDrainPerSecond = 2f;
    [SerializeField] private float sanityRecoveryPerSecond = 1.5f;

    private float _currentSanity;

    private float sanityDamageTimer;
    private float targetWeight;

    [Header("Heartbeat")]
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private float heartbeatLowSanity = 25f;

    private bool warningLightsActive = true; 
    private bool _dead;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        _currentSanity = _maxSanity;
    }

    private void Update()
    {
        HandleSanity();
        Debug.Log(_currentSanity); 
    }

    private void OnEnable()
    {
        LightSwitch.OnWarningLightsChanged += OnWarningLightsChanged;
    }

    private void OnDisable()
    {
        LightSwitch.OnWarningLightsChanged -= OnWarningLightsChanged;
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
            _playerStats.ChangeHealth(-sanityDamagePerTick);
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

    public void ChangeSanity(float amount)
    {
        _currentSanity = Mathf.Clamp(_currentSanity + amount, 0, _maxSanity);
    }

    public void ResetSanity()
    {
        _currentSanity = _maxSanity;
    }

    public void SetDead(bool dead)
    {
        _dead = dead;

        if (_dead && heartbeatSource.isPlaying)
            heartbeatSource.Stop();
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