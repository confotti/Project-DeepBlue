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
    public float nightStartHour = 20f;
    public float duskStartHour = 16f;
    public float fogLerpSpeed = 0.5f;

    [Header("Fog Attenuation")]
    public float dayAttenuation = 300f;
    public float nightAttenuation = 150f; 

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

            float targetMeanFreePath = Mathf.Lerp(
                nightAttenuation,
                dayAttenuation,
                fogFactor
            );

            fog.meanFreePath.value = Mathf.Lerp(
                fog.meanFreePath.value,
                targetMeanFreePath,
                Time.deltaTime * fogLerpSpeed
            );
        }
    }

    private float GetFogBlendFactor(float hours)
    {
        if (hours >= duskStartHour && hours <= nightStartHour)
            return Mathf.InverseLerp(nightStartHour, duskStartHour, hours);

        if (hours >= nightStartHour || hours < dayStartHour)
            return 0f;

        return 1f;
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
            if (vol.profile != null && vol.profile.TryGet<Fog>(out Fog fog))
            {
                fogVolume.Add(vol);
                fogOverrides.Add(fog);

                // Save each fog's current color & attenuation
                originalFogColors.Add(fog.albedo.value);

                fog.albedo.overrideState = true;
                fog.meanFreePath.overrideState = true;
            }
        }
    }
}