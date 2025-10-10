﻿using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Internal Clock")]
    [SerializeField]
    GameTimeStamp timestamp;

    [Tooltip("Length of a full game day in real-time minutes")]
    public float realMinutesPerDay = 25f;

    [Header("Day and Night Cycle")]
    public Transform sunTransform;
    Vector3 sunAngle;

    // One game day = 20 in-game hours = 72,000 seconds
    private const float SecondsPerGameDay = 20f * 3600f;

    // Store total game time in seconds
    private float totalGameSeconds;

    private float timeScale; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Start at Day 1, 6:00:00 
        timestamp = new GameTimeStamp(1, 6, 0, 0);

        totalGameSeconds = GameTimeStamp.TimeStampInSeconds(timestamp);

        float realSecondsPerDay = realMinutesPerDay * 60f;
        timeScale = SecondsPerGameDay / realSecondsPerDay;
    }

    private void Update()
    {
        totalGameSeconds += Time.deltaTime * timeScale;

        timestamp = SecondsToTimeStamp((int)totalGameSeconds);

        UpdateSunMovement();
    }

    void UpdateSunMovement()
    {
        float dayProgress = (totalGameSeconds % SecondsPerGameDay) / SecondsPerGameDay;

        // Full 360° rotation in one 20-hour day
        float sunRotation = dayProgress * 360f;

        // Rotate around X, står rakt upp vid 12
        sunAngle = new Vector3(sunRotation - 90f, 170f, 0f);
        sunTransform.rotation = Quaternion.Euler(sunAngle);
    }

    private GameTimeStamp SecondsToTimeStamp(int totalSeconds)
    {
        int day = totalSeconds / (20 * 3600);
        int hour = (totalSeconds / 3600) % 20;
        int minute = (totalSeconds / 60) % 60;
        int second = totalSeconds % 60;
        return new GameTimeStamp(day, hour, minute, second);
    }

    public float GetDayFactor()
    {
        float hours = GameTimeStamp.TimeStampInHours(timestamp) % 20f;
        float factor = Mathf.Cos((hours / 20f) * Mathf.PI * 2f) * 0.5f + 0.5f;
        return factor;
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        return new GameTimeStamp(timestamp);
    }
}