using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Internal Clock")]
    [SerializeField]
    GameTimeStamp timestamp;
    public float timeScale = 1.0f;

    [Header("Day and Night Cycle")]
    //transform the directional light
    public Transform sunTransform;
    Vector3 sunAngle;

    private void Awake()
    {
        //if there is more than one instance, destroy the extra
        if (Instance != null && Instance != this)
        {
            Destroy(this); 
        }
        else
        {
            //set the static instance to this instance 
            Instance = this; 
        }
    }

    private void Start()
    {
        timestamp = new GameTimeStamp(1, 6, 0);

        StartCoroutine(TimeUpdate());
    }

    IEnumerator TimeUpdate()
    {
        while (true)
        {
            Tick(); 

            yield return new WaitForSeconds(1 / timeScale); 
        }
    }

    void Tick()
    {
        timestamp.UpdateClock();
        UpdateSunMovement(); 
    }

    void UpdateSunMovement()
    {
        int timeInMinutes = GameTimeStamp.HoursToMinutes(timestamp.hour) + timestamp.minute;
        float angle = timeInMinutes <= 15 * 60 ? 0.2f * timeInMinutes : 180f + 0.6f * (timeInMinutes - 15 * 60);
        sunAngle = new Vector3(angle, 0, 0);
        sunTransform.rotation = Quaternion.Slerp(sunTransform.rotation, Quaternion.Euler(sunAngle), 1f * Time.deltaTime);
    }

    // Returns a value 0 → 1 representing day/night blend
    public float GetDayFactor()
    {
        int currentMinutes = GameTimeStamp.HoursToMinutes(timestamp.hour) + timestamp.minute;
        float gameHour = currentMinutes / 60f;
        float dayFactor = 1f;

        if (gameHour >= 15f && gameHour <= 20f)
        {
            if (gameHour <= 16f)
            {
                float t = Mathf.InverseLerp(15f, 16f, gameHour);
                dayFactor = 1f - Mathf.SmoothStep(0f, 1f, t);
            }
            else
            {
                float t = Mathf.InverseLerp(19f, 20f, gameHour);
                dayFactor = Mathf.SmoothStep(0f, 1f, t);
            }
        }
        else
        {
            dayFactor = 1f;
        }

        return dayFactor;
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        return new GameTimeStamp(timestamp);
    }
} 
