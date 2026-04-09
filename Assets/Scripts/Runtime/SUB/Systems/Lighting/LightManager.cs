using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LightSwitch lightSwitch;
    [SerializeField] private TimeManager timeManager;

    [Header("Flicker Settings")]
    [SerializeField] private float flickerSeconds = 3f;
    [SerializeField] private float powerOutageHours = 2f;

    [SerializeField] private float minFlickerDelay = 0.05f;
    [SerializeField] private float maxFlickerDelay = 0.2f;

    [SerializeField] private int minLightsToFlicker = 1;
    [SerializeField] private int maxLightsToFlicker = 4;

    private bool warningLightsOn = true;
    private bool eventStarted = false;

    private List<GameObject> flickerLights = new List<GameObject>();

    private void OnEnable()
    {
        LightSwitch.OnWarningLightsChanged += HandleWarningLights;
    }

    private void OnDisable()
    {
        LightSwitch.OnWarningLightsChanged -= HandleWarningLights;
    }

    private void Update()
    {
        if (timeManager == null || eventStarted)
            return;

        var time = timeManager.GetGameTimeStamp();

        if (time.Hour >= 0 && time.Hour < 3)
        {
            TryStartEvent();
        }
    }

    private void HandleWarningLights(bool warningOn)
    {
        warningLightsOn = warningOn;
    }

    private void TryStartEvent()
    {
        if (warningLightsOn)
            return;

        if (Random.Range(0f, 1f) < 0.0005f)
        {
            eventStarted = true;
            StartCoroutine(PowerFailureRoutine());
        }
    }

    private IEnumerator PowerFailureRoutine()
    {
        ChooseRandomLights();

        // Flicker phase
        float elapsed = 0f;

        while (elapsed < flickerSeconds)
        {
            ToggleSelectedLights(false);
            yield return new WaitForSeconds(Random.Range(minFlickerDelay, maxFlickerDelay));

            ToggleSelectedLights(true);
            yield return new WaitForSeconds(Random.Range(minFlickerDelay, maxFlickerDelay));

            elapsed += Time.deltaTime;
        }

        // Turn lights OFF
        ToggleSelectedLights(false);

        // Stay off for 2 hours
        float outageTime = powerOutageHours * 3600f;
        yield return new WaitForSeconds(outageTime);

        // Turn lights back ON
        ToggleSelectedLights(true);

        eventStarted = false;
    }

    private void ChooseRandomLights()
    {
        flickerLights.Clear();

        var lights = lightSwitch.lightSources;

        int count = Random.Range(minLightsToFlicker, maxLightsToFlicker + 1);

        for (int i = 0; i < count; i++)
        {
            var randomLight = lights[Random.Range(0, lights.Count)];

            if (!flickerLights.Contains(randomLight))
            {
                flickerLights.Add(randomLight);
            }
        }
    }

    private void ToggleSelectedLights(bool state)
    {
        foreach (var light in flickerLights)
        {
            if (light != null)
                light.SetActive(state);
        }
    }
} 