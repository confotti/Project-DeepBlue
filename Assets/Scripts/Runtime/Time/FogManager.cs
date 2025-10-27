using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("The fog color during night (lerps toward this color at night).")]
    public Color nightFogColor = new Color(0.5f, 0.05f, 0.1f);

    [Tooltip("Time (in hours) when day starts.")]
    public float dayStartHour = 6f;
    [Tooltip("Time (in hours) when night starts.")]
    public float nightStartHour = 20f;
    [Tooltip("Time (in hours) when dusk starts (transition to night).")]
    public float duskStartHour = 16f;
    [Tooltip("Lerp speed for color transitions.")]
    public float fogLerpSpeed = 0.5f;

    public List<Volume> fogVolume = new List<Volume>();

    // Store each fog's base color and attenuation distance
    private readonly List<Fog> fogOverrides = new List<Fog>();
    private readonly List<Color> originalFogColors = new List<Color>();
    private readonly List<float> originalAttenuation = new List<float>();

    private void Start()
    {
        RefreshFogVolumes();
    }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        GameTimeStamp time = TimeManager.Instance.GetGameTimeStamp();
        float hours = time.hour + time.minute / 60f;

        // Compute fogFactor (0 = night, 1 = day)
        float fogFactor = GetFogBlendFactor(hours);

        // Apply to all fog volumes
        for (int i = 0; i < fogOverrides.Count; i++)
        {
            if (fogOverrides[i] == null) continue;

            Fog fog = fogOverrides[i];

            // Lerp from nightFogColor (at night) to original color (at day)
            Color targetFogColor = Color.Lerp(nightFogColor, originalFogColors[i], fogFactor);
            fog.albedo.value = Color.Lerp(fog.albedo.value, targetFogColor, Time.deltaTime * fogLerpSpeed);

            // Keep the manually set attenuation distance
            fog.meanFreePath.value = originalAttenuation[i];
        }
    }

    private float GetFogBlendFactor(float hours)
    {
        // Returns 1 during full day, 0 during full night, and smoothly blends between
        if (hours >= duskStartHour && hours <= nightStartHour)
        {
            // Dusk: 1 → 0
            return Mathf.InverseLerp(nightStartHour, duskStartHour, hours);
        }
        else if (hours >= nightStartHour || hours < dayStartHour)
        {
            // Night
            return 0f;
        }
        else if (hours >= dayStartHour && hours < duskStartHour)
        {
            // Day
            return 1f;
        }
        else
        {
            return 1f;
        }
    }

    [ContextMenu("Refresh Fog Volumes")]
    public void RefreshFogVolumes()
    {
        fogVolume.Clear();
        fogOverrides.Clear();
        originalFogColors.Clear();
        originalAttenuation.Clear();

        Volume[] allVolumes = FindObjectsOfType<Volume>();
        foreach (var vol in allVolumes)
        {
            if (vol.profile != null && vol.profile.TryGet<Fog>(out Fog fog))
            {
                fogVolume.Add(vol);
                fogOverrides.Add(fog);

                // Save each fog's current color & attenuation
                originalFogColors.Add(fog.albedo.value);
                originalAttenuation.Add(fog.meanFreePath.value);

                fog.albedo.overrideState = true;
                fog.meanFreePath.overrideState = true;
            }
        }
    }
}