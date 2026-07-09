using Ilumisoft.RadarSystem;
using Ilumisoft.RadarSystem.UI;
using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    [Header("Tag")]
    [SerializeField] private string _trackedTag = "Enemy";

    [Header("Blip Speed")]
    [SerializeField] private float _blipIntervalClose = 0.2f;  // fast when close
    [SerializeField] private float _blipIntervalFar = 2f;      // slow when far
    [SerializeField] private float _closeDistance = 5f;
    [SerializeField] private float _farDistance = 30f;

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _blipSound;

    private Radar _radar;
    private float _timer;
    private bool _blipVisible;

    private void Awake()
    {
        _radar = GetComponent<Radar>();
    }

    private void Update()
    {
        if (_radar.Player == null)
        {
            Debug.Log("No player assigned on Radar");
            return;
        }

        float closestDistance = GetClosestTaggedObjectDistance();
        Debug.Log($"Closest tagged object distance: {closestDistance}");

        if (closestDistance < 0)
        {
            Debug.Log("No tagged objects found");
            SetTaggedIconsVisible(false);
            return;
        }

        float t = Mathf.InverseLerp(_closeDistance, _farDistance, closestDistance);
        float interval = Mathf.Lerp(_blipIntervalClose, _blipIntervalFar, t);

        _timer += Time.deltaTime;

        if (_timer >= interval)
        {
            _timer = 0f;
            _blipVisible = !_blipVisible;
            SetTaggedIconsVisible(_blipVisible);
            Debug.Log($"Blip toggled, visible: {_blipVisible}");

            if (_blipVisible)
                PlayBlipSound(closestDistance); 
        }
    } 

    private float GetClosestTaggedObjectDistance()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(_trackedTag);

        if (taggedObjects.Length == 0) return -1;

        float closest = float.MaxValue;

        foreach (var obj in taggedObjects)
        {
            float dist = Vector3.Distance(obj.transform.position, _radar.Player.transform.position);
            if (dist < closest)
                closest = dist;
        }

        return closest;
    }

    private void SetTaggedIconsVisible(bool visible)
    {
        // Find all locatables with the matching tag and toggle their icon
        var locatables = FindObjectsByType<LocatableComponent>(FindObjectsSortMode.None);

        foreach (var locatable in locatables)
        {
            if (locatable.CompareTag(_trackedTag))
            {
                var canvasGroup = locatable.GetComponentInChildren<CanvasGroup>();
                if (canvasGroup != null)
                    canvasGroup.alpha = visible ? 1f : 0f;
            }
        } 
    }

    private void PlayBlipSound(float distance)
    {
        if (distance > _farDistance) return;
        if (_audioSource != null && _blipSound != null)
            _audioSource.PlayOneShot(_blipSound);
    } 
} 