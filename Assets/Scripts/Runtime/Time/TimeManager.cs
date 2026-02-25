using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Internal Clock")]
    [SerializeField] private GameTimeStamp _timestamp;

    [Tooltip("Length of a full game day in real-time minutes")]
    public float realMinutesPerDay = 25f;

    [Header("Day and Night Cycle")]
    public Transform sunTransform;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _clockText;

    // One game day = 24 in-game hours = 86,400 seconds
    private const float SecondsPerGameDay = 24f * 3600f;
    // Store total game time in seconds
    private float _accumulatedSeconds;
    private float _timeScale;

    public event Action<int> OnMinuteChanged;
    public event Action<int> OnHourChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        // Start at Day 1, 8:00:00 
        _timestamp = new GameTimeStamp(1, 8, 0, 0);

        float realSecondsPerDay = realMinutesPerDay * 60f;
        _timeScale = SecondsPerGameDay / realSecondsPerDay;

        UpdateClockUI();
    }

    private void Update()
    {
        _accumulatedSeconds += Time.deltaTime * _timeScale;

        while(_accumulatedSeconds >= 1f)
        {
            _accumulatedSeconds -= 1f;
            TickOneSecond();
        }
    }

    private void TickOneSecond()
    {
        _timestamp.AddOneSecond();

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

    void UpdateSunMovement()
    {
        float dayProgress = (_timestamp.Hour * 3600 + _timestamp.Minute * 60 + _timestamp.Second) / SecondsPerGameDay;

        // Full 360° rotation in one 24-hour day
        float sunRotation = dayProgress * 360f;

        // Rotate around X, står rakt upp vid 12
        sunTransform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0f);
    }

    private void UpdateClockUI()
    {
        if (_clockText == null) return;

        _clockText.text = $"{_timestamp.Hour:00}:{_timestamp.Minute - (_timestamp.Minute % 5):00}";
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        return new GameTimeStamp(_timestamp);
    }
    

    //Allows pausing time if we need to
    public void SetTimeScale(float timeScale)
    {
        _timeScale = timeScale;
    }
}