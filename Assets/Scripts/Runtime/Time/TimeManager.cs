using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TimeManager : MonoBehaviour
{
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
        _timestamp = new GameTimeStamp(1, 18, 0, 0); 
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

        UpdateSunMovement();
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
        float dayProgress = (_timestamp.Hour * 3600 + _timestamp.Minute * 60 + _timestamp.Second) / SecondsPerGameDay;

        float sunRotation = dayProgress * 360f;

        if (sunTransform)
            sunTransform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0f);
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