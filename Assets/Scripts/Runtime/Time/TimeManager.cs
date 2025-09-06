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

    [Header("Color Grading Settings")]
    public Volume postProcessingVolume;
    private ColorAdjustments colorAdjustments; 

    [Header("Day and Night Cycle")]
    //transform the directional light
    public Transform sunTransform;
    Vector3 sunAngle;
    [Header("Fog Settings")]
    public float dayFogStart = 0f;
    public float dayFogEnd = 150f;
    public float nightFogStart = 0f;
    public float nightFogEnd = 50f;
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = Color.black;

    // Fog interpolation speed
    public float fogTransitionSpeed = 1.0f;

    // Current interpolated fog values
    private float currentFogStart;
    private float currentFogEnd;
    private Color currentFogColor;
    private Color currentBackgroundColor;
    private float targetFogStart;
    private float targetFogEnd;
    private Color targetFogColor;
    private Color targetBackgroundColor; 

    [Header("Background Color Settings")]
    public Color dayBackgroundColor = new Color(0.5f, 0.8f, 1f);  // Light blue
    public Color nightBackgroundColor = new Color(0f, 0f, 0.1f);  // Dark blue/black 
    public Camera mainCamera; 

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
        currentFogStart = RenderSettings.fogStartDistance;
        currentFogEnd = RenderSettings.fogEndDistance;
        currentFogColor = RenderSettings.fogColor;

        if (mainCamera != null)
        {
            currentBackgroundColor = mainCamera.backgroundColor;
        }

        StartCoroutine(TimeUpdate());

        if (postProcessingVolume != null && postProcessingVolume.profile != null)
        {
            postProcessingVolume.profile.TryGet(out colorAdjustments);
        } 
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
        //converts the current time to minutes 
        int timeInMinutes = GameTimeStamp.HoursToMinutes(timestamp.hour) + timestamp.minute;

        //during daytime
        //sun moves .2 degees a minute
        //during nighttime
        //sun moves .6 degrees a minute

        float sunAngle = 0;
        if (timeInMinutes <= 15 * 60)
        {
            sunAngle = .2f * timeInMinutes; 
        }
        else if(timeInMinutes > 15 * 60)
        {
            sunAngle = 180f + .6f * (timeInMinutes - (15 * 60)); 
        }

        //Apply angle to the dir light
        //sunTransform.eulerAngles = new Vector3(sunAngle, 0, 0); 
        this.sunAngle = new Vector3(sunAngle, 0, 0);

        int currentMinutes = timeInMinutes;

        // Convert to game hour
        float gameHour = currentMinutes / 60f;
        float dayFactor = 1f;

        if (gameHour >= 15f && gameHour <= 20f)
        {
            if (gameHour <= 16f)
            {
                // From 15 to 16 → dayFactor: 1 → 0 (fade out)
                float t = Mathf.InverseLerp(15f, 16f, gameHour);
                dayFactor = 1f - Mathf.SmoothStep(0f, 1f, t);
            }
            else
            {
                // From 19 to 20 → dayFactor: 0 → 1 (fade in)
                float t = Mathf.InverseLerp(19f, 20f, gameHour);
                dayFactor = Mathf.SmoothStep(0f, 1f, t);
            }
        }
        else
        {
            // Full day outside 15–20
            dayFactor = 1f;
        } 

        targetFogStart = Mathf.Lerp(nightFogStart, dayFogStart, dayFactor);
        targetFogEnd = Mathf.Lerp(nightFogEnd, dayFogEnd, dayFactor);
        targetFogColor = Color.Lerp(nightFogColor, dayFogColor, dayFactor);
        targetBackgroundColor = Color.Lerp(nightBackgroundColor, dayBackgroundColor, dayFactor); 

        // ---- CAMERA BACKGROUND COLOR CONTROL ----
        if (mainCamera != null && mainCamera.clearFlags == CameraClearFlags.SolidColor)
        {
            mainCamera.backgroundColor = Color.Lerp(nightBackgroundColor, dayBackgroundColor, dayFactor);
        }
    }

    private void Update()
    {
        sunTransform.rotation = Quaternion.Slerp(sunTransform.rotation, Quaternion.Euler(sunAngle), 1f * Time.deltaTime);

        // Smooth fog transition
        currentFogStart = Mathf.Lerp(currentFogStart, targetFogStart, fogTransitionSpeed * Time.deltaTime);
        currentFogEnd = Mathf.Lerp(currentFogEnd, targetFogEnd, fogTransitionSpeed * Time.deltaTime);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, fogTransitionSpeed * Time.deltaTime);

        RenderSettings.fogStartDistance = currentFogStart;
        RenderSettings.fogEndDistance = currentFogEnd;
        RenderSettings.fogColor = currentFogColor;

        // Smooth camera background transition
        if (mainCamera != null && mainCamera.clearFlags == CameraClearFlags.SolidColor)
        {
            currentBackgroundColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, fogTransitionSpeed * Time.deltaTime);
            mainCamera.backgroundColor = currentBackgroundColor;
        } 
    }

    //function to skip time

    public void SkipTime(GameTimeStamp timeToSkipTo)
    {
        //converts to minutes
        int timeToSkipInMinutes = GameTimeStamp.TimeStampInMinutes(timeToSkipTo);
        Debug.Log("Time Skip to:" + timeToSkipInMinutes);
        int timeNowInMinutes = GameTimeStamp.TimeStampInMinutes(timestamp);
        Debug.Log("Time Now: " + timeNowInMinutes);

        int differenceInMinutes = timeToSkipInMinutes - timeNowInMinutes; 
        Debug.Log("Skip " + differenceInMinutes + "minutes");

        //check if the timestamp to skip to has already been reached
        if (differenceInMinutes <= 0) return;

        for (int i = 0; i < differenceInMinutes; i++)
        {
            Tick(); 
        }
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        //return a cloned instance
        return new GameTimeStamp(timestamp); 
    }
}
