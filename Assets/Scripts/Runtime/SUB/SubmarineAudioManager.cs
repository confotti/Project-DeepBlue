using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineAudioManager : MonoBehaviour
{
    public static SubmarineAudioManager Instance { get; private set; }

    [Header("Night Sound Objects")]
    public List<GameObject> nightSoundObjects; // Each has an AudioSource

    [Header("Night Timing")]
    public int nightStartHour = 22;
    public int nightEndHour = 6;

    private List<GameObject> lastNightPlayed = new List<GameObject>();
    private List<GameObject> playedTonight = new List<GameObject>();
    private bool nightStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // Disable all AudioSources at start
        foreach (GameObject go in nightSoundObjects)
        {
            AudioSource src = go.GetComponent<AudioSource>();
            if (src) src.enabled = false;
        }
    }

    private Coroutine nightCoroutine;

    private void Update()
    {
        if (!TimeManager.Instance) return;

        bool isNight = IsNightTime();

        if (isNight && !nightStarted)
        {
            nightStarted = true;
            nightCoroutine = StartCoroutine(PlayRandomNightSounds());
        }
        else if (!isNight && nightStarted)
        {
            nightStarted = false;
            playedTonight.Clear();

            // Stop any ongoing night sounds
            if (nightCoroutine != null)
            {
                StopCoroutine(nightCoroutine);
                nightCoroutine = null;
            }
        }
    } 

    private bool IsNightTime()
    {
        int hour = TimeManager.Instance.GetGameTimeStamp().Hour;

        if (nightStartHour < nightEndHour)
        {
            // Night does NOT cross midnight (rare)
            return hour >= nightStartHour && hour < nightEndHour;
        }
        else
        {
            // Night crosses midnight (common: 22 to 6)
            return hour >= nightStartHour || hour < nightEndHour;
        }
    } 

    private IEnumerator PlayRandomNightSounds()
    {
        int numSounds = Random.Range(3, 6);

        // Pool excluding last night
        List<GameObject> availableSounds = new List<GameObject>(nightSoundObjects);
        availableSounds.RemoveAll(s => lastNightPlayed.Contains(s));

        // Shuffle pool
        for (int i = 0; i < availableSounds.Count; i++)
        {
            GameObject temp = availableSounds[i];
            int randomIndex = Random.Range(i, availableSounds.Count);
            availableSounds[i] = availableSounds[randomIndex];
            availableSounds[randomIndex] = temp;
        }

        for (int i = 0; i < numSounds && availableSounds.Count > 0; i++)
        {
            GameObject soundObj = availableSounds[0];
            availableSounds.RemoveAt(0);

            playedTonight.Add(soundObj);

            AudioSource source = soundObj.GetComponent<AudioSource>();
            if (source)
            {
                source.enabled = true; // Enable the AudioSource
                source.Play();
            }

            // Wait 30–90 in-game seconds
            float waitTime = Random.Range(30f, 90f) / TimeManager.Instance.daySpeedMultiplier;
            yield return new WaitForSeconds(waitTime);

            // Optionally disable it after playing
            if (source) source.enabled = false;
        }

        lastNightPlayed.Clear();
        lastNightPlayed.AddRange(playedTonight);
    } 
}
