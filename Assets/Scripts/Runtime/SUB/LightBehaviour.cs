using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightBehaviour : MonoBehaviour, IInteractable
{
    [Header("Lights Settings")]
    public List<GameObject> lightSources = new List<GameObject>();

    [Header("Night Settings")]
    [SerializeField] private FogManager fogManager;
    [SerializeField] [Range(0f, 24f)] private float lightOffStartHourOffset = 0f;

    private bool lightOn = true;
    private bool nightEventTriggered = false;

    private List<GameObject> currentlyDisabledLights = new List<GameObject>();

    private float nightDuration;
    private float lightsRestoreHour;

    public string InteractText => throw new System.NotImplementedException();
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor)
    {
        lightOn = !lightOn;

        foreach (var obj in lightSources)
            obj.SetActive(lightOn);
    }

    public void EndInteraction() { }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        GameTimeStamp time = TimeManager.Instance.GetGameTimeStamp();
        float hours = time.hour + time.minute / 60f;

        bool isNight = hours >= fogManager.nightStartHour || hours <= fogManager.dayStartHour;

        if (isNight)
        {
            if (!nightEventTriggered)
            {
                nightEventTriggered = true;

                nightDuration = (fogManager.dayStartHour - fogManager.nightStartHour + 24) % 24;
                float lightsOffStartHour = (fogManager.nightStartHour + lightOffStartHourOffset) % 24;
                lightsRestoreHour = (lightsOffStartHour + nightDuration * 0.5f) % 24;

                if (IsTimeInRange(hours, lightsOffStartHour, lightsRestoreHour))
                    ApplyRandomNightLights();
            }

            if (currentlyDisabledLights.Count > 0 &&
                IsTimeInRange(hours, lightsRestoreHour, fogManager.dayStartHour))
            {
                RestoreLights();
            }
        }
        else
        {
            if (nightEventTriggered)
            {
                RestoreLights();
                nightEventTriggered = false;
            }
        }
    }

    private void ApplyRandomNightLights()
    {
        int amountToDisable = Mathf.CeilToInt(lightSources.Count * 0.3f);
        amountToDisable = Mathf.Min(amountToDisable, lightSources.Count);

        List<GameObject> tempList = new List<GameObject>(lightSources);

        for (int i = 0; i < amountToDisable; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            GameObject selected = tempList[randomIndex];

            StartCoroutine(FlickerAndDisable(selected));

            currentlyDisabledLights.Add(selected);
            tempList.RemoveAt(randomIndex);
        }
    }

    private IEnumerator FlickerAndDisable(GameObject lightObj)
    {
        int flickerCount = Random.Range(3, 7);

        for (int i = 0; i < flickerCount; i++)
        {
            lightObj.SetActive(false);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            lightObj.SetActive(true);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        lightObj.SetActive(false); 
    }

    private void RestoreLights()
    {
        foreach (var obj in currentlyDisabledLights)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        currentlyDisabledLights.Clear();
    }

    private bool IsTimeInRange(float currentHour, float startHour, float endHour)
    {
        if (startHour < endHour)
            return currentHour >= startHour && currentHour < endHour;
        else
            return currentHour >= startHour || currentHour < endHour;
    }
}