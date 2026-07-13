using Ilumisoft.RadarSystem;
using System.Collections; 
using Ilumisoft.RadarSystem.UI;
using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    [Header("Tag")]
    [SerializeField] private string _trackedTag = "Enemy";

    [Header("Radar Pulse")]
    [SerializeField] private float _revealDuration = 2f;
    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private ParticleSystem radarPulse;
    [SerializeField] private float _sonarCooldown = 5f;

    private float _nextSonarTime; 

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _blipSound;

    private Radar _radar;
    private bool _sonarActive = false; 

    private float _revealTimer;

    private void Awake()
    {
        _radar = GetComponent<Radar>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_radar.Player == null)
            return;

        if (Input.GetMouseButtonDown(0) && Time.time >= _nextSonarTime)
        {
            _nextSonarTime = Time.time + _sonarCooldown;

            _sonarActive = true;
            _revealTimer = _revealDuration;

            _radar.ShowEnemyBlips = true;
            _audioSource.PlayOneShot(_blipSound);
            radarPulse.Play();
        } 

        if (_sonarActive)
        {
            _revealTimer -= Time.deltaTime;

            if (_revealTimer <= 0)
            {
                _sonarActive = false;
                _radar.ShowEnemyBlips = false; 
            }
        }
    } 

    private IEnumerator FadeIcons()
    {
        float timer = 0f;

        var locatables = FindObjectsByType<LocatableComponent>(
            FindObjectsSortMode.None
        );

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / _fadeDuration);

            foreach (var locatable in locatables)
            {
                if (locatable.CompareTag(_trackedTag))
                {
                    var canvasGroup = locatable.GetComponentInChildren<CanvasGroup>();

                    if (canvasGroup != null)
                        canvasGroup.alpha = alpha;
                }
            }

            yield return null;
        }

        SetTaggedIconsVisible(false);
    } 


    private float GetClosestTaggedObjectDistance()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(_trackedTag);

        if (taggedObjects.Length == 0)
            return -1;

        float closest = float.MaxValue;

        foreach (var obj in taggedObjects)
        {
            float dist = Vector3.Distance(
                obj.transform.position,
                _radar.Player.transform.position
            );

            if (dist < closest)
                closest = dist;
        }

        return closest;
    }

    private void SetTaggedIconsVisible(bool visible)
    {
        var locatables = FindObjectsByType<LocatableComponent>(
            FindObjectsSortMode.None
        );

        foreach (var locatable in locatables)
        {
            if (locatable.CompareTag(_trackedTag))
            {
                var icon = locatable.GetComponentInChildren<LocatableIconComponent>();

                if (icon != null)
                {
                    icon.SetVisible(visible);
                }
            }
        }
    } 

    private void PlayBlipSound(float distance)
    {
        if (_audioSource != null && _blipSound != null)
        {
            _audioSource.PlayOneShot(_blipSound);
        }
    }
} 