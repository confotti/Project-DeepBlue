using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    public static FogManager Instance { get; private set; }

    [Header("Transition Settings")]
    public float transitionTime = 2f; // How long the transition should roughly take
    public Camera mainCamera;

    private List<FogTrigger> activeTriggers = new List<FogTrigger>();

    private float currentFogStart;
    private float currentFogEnd;
    private Color currentFogColor;
    private Color currentBackgroundColor;

    // Velocities for SmoothDamp
    private float fogStartVel;
    private float fogEndVel;
    private Color fogColorVel;
    private Color bgColorVel;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        currentFogStart = RenderSettings.fogStartDistance;
        currentFogEnd = RenderSettings.fogEndDistance;
        currentFogColor = RenderSettings.fogColor;
        if (mainCamera != null) currentBackgroundColor = mainCamera.backgroundColor;
    }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        float dayFactor = TimeManager.Instance.GetDayFactor();

        // Default fog (outside any triggers)
        float targetFogStart = 50f;
        float targetFogEnd = 150f;
        Color targetFogColor = Color.gray;
        Color targetBackgroundColor = mainCamera != null ? mainCamera.backgroundColor : Color.black;

        if (activeTriggers.Count > 0)
        {
            // Use the last trigger entered as priority
            FogTrigger activeTrigger = activeTriggers[activeTriggers.Count - 1];
            activeTrigger.GetTargetSettings(dayFactor, out targetFogStart, out targetFogEnd, out targetFogColor, out targetBackgroundColor);
        }

        float smoothTime = transitionTime;

        // SmoothDamp for float values
        currentFogStart = Mathf.SmoothDamp(currentFogStart, targetFogStart, ref fogStartVel, smoothTime);
        currentFogEnd = Mathf.SmoothDamp(currentFogEnd, targetFogEnd, ref fogEndVel, smoothTime);

        // Approximate SmoothDamp for colors
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, Time.deltaTime / smoothTime);
        currentBackgroundColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, Time.deltaTime / smoothTime);

        // Apply to scene
        RenderSettings.fogStartDistance = currentFogStart;
        RenderSettings.fogEndDistance = currentFogEnd;
        RenderSettings.fogColor = currentFogColor;

        if (mainCamera != null && mainCamera.clearFlags == CameraClearFlags.SolidColor)
        {
            mainCamera.backgroundColor = currentBackgroundColor;
        }
    }

    public void RegisterTrigger(FogTrigger trigger)
    {
        if (!activeTriggers.Contains(trigger)) activeTriggers.Add(trigger);
    }

    public void UnregisterTrigger(FogTrigger trigger)
    {
        if (activeTriggers.Contains(trigger)) activeTriggers.Remove(trigger);
    }
}