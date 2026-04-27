using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    [Header("Fog Settings")]
    public Color nightFogColor = new Color(0.5f, 0.05f, 0.1f);
    public float dayStartHour = 6f;
    public float nightStartHour = 22f;
    public float duskStartHour = 20f;
    public float dawnStartHour = 4f;
    public float fogLerpSpeed = 0.02f;

    [Header("Fog Attenuation")]
    public float dayAttenuation = 300f;
    public float nightAttenuation = 150f;

    [Header("Night Fog")]
    public bool useBlackFogAtNight = true;
    [Range(0f, 1f)]
    public float nightFogIntensity = 1f; // 1 = full black, 0 = no change

    public List<Volume> fogVolume = new List<Volume>();

    private readonly List<Fog> fogOverrides = new List<Fog>();
    private readonly List<Color> originalFogColors = new List<Color>();

    private void Start()
    {
        RefreshFogVolumes();
    }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        GameTimeStamp time = TimeManager.Instance.GetGameTimeStamp();
        float hours = time.Hour + time.Minute / 60f;

        // Compute fogFactor (0 = night, 1 = day)
        float fogFactor = GetFogBlendFactor(hours);

        // Apply to all fog volumes
        for (int i = 0; i < fogOverrides.Count; i++)
        {
            if (fogOverrides[i] == null) continue;

            Fog fog = fogOverrides[i];

            // Lerp from nightFogColor (at night) to original color (at day)
            Color nightColor = useBlackFogAtNight
                ? Color.black
                : nightFogColor;

            // Blend toward black (or chosen night color)
            Color blendedNight = Color.Lerp(originalFogColors[i], nightColor, nightFogIntensity);

            // Then blend with time of day
            Color targetFogColor = Color.Lerp(blendedNight, originalFogColors[i], fogFactor);
            fog.albedo.value = targetFogColor; 

            float targetMeanFreePath = Mathf.Lerp(
                nightAttenuation,
                dayAttenuation,
                fogFactor
            );

            fog.meanFreePath.value = targetMeanFreePath; 
        }
    }

    private float GetFogBlendFactor(float hours)
    {
        // Dawn (night → day)
        if (hours >= dawnStartHour && hours < dayStartHour)
        {
            float t = Mathf.InverseLerp(dawnStartHour, dayStartHour, hours);
            return Mathf.SmoothStep(0f, 1f, t);
        }

        // Day
        if (hours >= dayStartHour && hours < duskStartHour)
        {
            return 1f;
        }

        // Dusk (day → night)
        if (hours >= duskStartHour && hours < nightStartHour)
        {
            float t = Mathf.InverseLerp(duskStartHour, nightStartHour, hours);
            return Mathf.SmoothStep(1f, 0f, t);
        }

        // Night
        return 0f;
    }

    [ContextMenu("Refresh Fog Volumes")]
    public void RefreshFogVolumes()
    {
        fogVolume.Clear();
        fogOverrides.Clear();
        originalFogColors.Clear();

        Volume[] allVolumes = FindObjectsOfType<Volume>();
        foreach (var vol in allVolumes)
        {
            if (vol.profile != null)
            {
                vol.profile = Instantiate(vol.profile); // create runtime instance

                if (vol.profile.TryGet<Fog>(out Fog fog))
                {
                    fogVolume.Add(vol);
                    fogOverrides.Add(fog);

                    originalFogColors.Add(fog.albedo.value);

                    fog.albedo.overrideState = true;
                    fog.meanFreePath.overrideState = true;
                }
            } 
        }
    }
} 