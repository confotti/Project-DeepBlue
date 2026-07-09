using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.Timeline;
using System.Collections.Generic;

public class LightEnableTimeLine : MonoBehaviour, IInteractable
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private TimelineAsset newTimeline;

    [Header("Light Settings")]
    public List<GameObject> lightSources = new List<GameObject>();
    public List<GameObject> warningLights = new List<GameObject>();
    [SerializeField] private SubmarineEngine engine;

    private bool lightOn = false;
    private bool hasInteracted = false;

    public static event System.Action<bool> OnWarningLightsChanged;
    public string InteractText => "Toggle Lights";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public event System.Action OnFirstInteract;

    public void Interact(PlayerInteract interactor)
    {
        // Prevent turning lights on if fuel = 0
        if (!lightOn && engine != null && !engine.HasFuel())
            return;

        lightOn = !lightOn;
        SetLights(lightOn);

        // Play timeline only on first interaction
        if (!hasInteracted)
        {
            hasInteracted = true;
            OnFirstInteract?.Invoke();

            if (timeline != null)
            {
                timeline.playableAsset = newTimeline;
                timeline.Play();
            }
        }
    }

    public void EndInteraction() { }

    private void SetLights(bool normalLightsOn)
    {
        foreach (var obj in lightSources)
            if (obj != null)
                obj.SetActive(normalLightsOn);

        foreach (var obj in warningLights)
            if (obj != null)
                obj.SetActive(!normalLightsOn);

        OnWarningLightsChanged?.Invoke(!normalLightsOn);
    }

    public void ForceWarningLights()
    {
        lightOn = false;
        SetLights(false);
    }
} 