using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TimeManager : MonoBehaviour
{
    private FogManager fogManager; 
    public static TimeManager Instance { get; private set; }

    public bool IsTimePaused { get; private set; }

    public event Action<int> OnDayChanged; 

    [Header("Internal Clock")]
    [SerializeField] private GameTimeStamp _timestamp;

    [Tooltip("Length of a full game day in real-time minutes")]
    public float realMinutesPerDay = 25f;

    [Header("Day/Night Speed")]
    public float daySpeedMultiplier = 1f;
    public float nightSpeedMultiplier = 1.25f;

    [Header("Day and Night Cycle")]
    public Transform sunTransform;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _clockText;

    // One game day = 24 in-game hours = 86,400 seconds
    private const float SecondsPerGameDay = 24f * 3600f;

    private float _accumulatedSeconds;
    private float _timeScale;

    public event Action<int> OnMinuteChanged;
    public event Action<int> OnHourChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // Start at Day 1, 8:00:00
        _timestamp = new GameTimeStamp(1, 12, 0, 0);

        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        fogManager = FindObjectOfType<FogManager>(); 
    }

    private void Start()
    {
        float realSecondsPerDay = realMinutesPerDay * 60f;
        _timeScale = SecondsPerGameDay / realSecondsPerDay;

        UpdateClockUI();
    }

    private void Update()
    {
        if (IsTimePaused)
            return;

        float speedMultiplier = IsNightTime() ? nightSpeedMultiplier : daySpeedMultiplier;
        _accumulatedSeconds += Time.deltaTime * _timeScale * speedMultiplier;

        while (_accumulatedSeconds >= 1f)
        {
            _accumulatedSeconds -= 1f;
            TickOneSecond();
        }

        UpdateSunMovement(); 
    }

    public void PauseTime()
    {
        IsTimePaused = true;
    }

    public void ResumeTime()
    {
        IsTimePaused = false;
    }

    private bool IsNightTime()
    {
        int hour = _timestamp.Hour;
        return (hour >= 22 || hour < 6);
    }

    private void TickOneSecond()
    {
        int previousHour = _timestamp.Hour;
        int previousMinute = _timestamp.Minute;

        _timestamp.AddOneSecond();

        if (previousHour < 6 && _timestamp.Hour >= 6)
        {
            OnDayChanged?.Invoke(_timestamp.Day);
        } 

        if (_timestamp.Second == 0)
        {
            OnMinuteChanged?.Invoke(_timestamp.Minute);
            UpdateClockUI();

            if (_timestamp.Minute == 0)
            {
                OnHourChanged?.Invoke(_timestamp.Hour);
            }
        }
    } 

    public void SetTime(int hour, int minute = 0, int second = 0)
    {
        _timestamp = new GameTimeStamp(
            _timestamp.Day,
            hour,
            minute,
            second
        );

        UpdateClockUI();
        UpdateSunMovement();

        OnHourChanged?.Invoke(hour);
        OnMinuteChanged?.Invoke(minute);
    }

    void UpdateSunMovement()
    {
        float hours = _timestamp.Hour + _timestamp.Minute / 60f + _timestamp.Second / 3600f;

        float sunAngle = GetSunAngle(hours);

        if (sunTransform)
            sunTransform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
    }

    float GetSunAngle(float hours)
    {
        if (fogManager == null)
            return 90f;

        float dawn = fogManager.dawnStartHour;
        float day = fogManager.dayStartHour;
        float dusk = fogManager.duskStartHour;
        float night = fogManager.nightStartHour;

        float noon = (day + dusk) * 0.5f; // dynamic noon

        // Dawn → Noon
        if (hours >= dawn && hours < noon)
        {
            float t = Mathf.InverseLerp(dawn, noon, hours);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(50f, 90f, t);
        }

        // Noon → Dusk
        if (hours >= noon && hours < dusk)
        {
            float t = Mathf.InverseLerp(noon, dusk, hours);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(90f, 130f, t);
        }

        // Dusk → Night
        if (hours >= dusk && hours < night)
        {
            float t = Mathf.InverseLerp(dusk, night, hours);
            return Mathf.Lerp(130f, 220f, t);
        }

        // Night → Dawn (continuous)
        if (hours >= night || hours < dawn)
        {
            float totalNightDuration = (24f - night) + dawn;

            float timeSinceNightStart = (hours >= night)
                ? (hours - night)
                : (hours + (24f - night));

            float t = timeSinceNightStart / totalNightDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            return Mathf.Lerp(220f, 50f, t);
        } 

        return 90f;
    } 

    private void UpdateClockUI()
    {
        if (_clockText == null) return;

        int roundedMinute = _timestamp.Minute - (_timestamp.Minute % 2);
        _clockText.text = $"{_timestamp.Hour:00}:{roundedMinute:00}";
    } 

    public GameTimeStamp GetGameTimeStamp()
    {
        return new GameTimeStamp(_timestamp);
    }

    public void SetTimeScale(float timeScale)
    {
        _timeScale = timeScale;
    }
}