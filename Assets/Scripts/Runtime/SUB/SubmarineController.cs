using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Splines;

public class SubmarineController : MonoBehaviour
{
    [SerializeField] private BiomePort _biomePort;
    private SplineAnimate splineAnimate;

    [Header("Player Follow")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform sub;

    private BiomeSubSplineHolder _nextBiomeSpline;
    private BiomeSubSplineHolder _currentBiomeSpline;

    private bool _nextBiomeReady = false;

    void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.Pause();  // startas avstängd
    }

    void OnEnable()
    {
        BiomeSubSplineHolder.OnSpawned += SetNextBiomeSpline;
        _biomePort.OnFinishLoadingBiome += OnFinishedLoading;
    }

    void OnDisable()
    {
        BiomeSubSplineHolder.OnSpawned -= SetNextBiomeSpline;
        _biomePort.OnFinishLoadingBiome += OnFinishedLoading;
    }

    private void SetNextBiomeSpline(BiomeSubSplineHolder spline)
    {
        if (_currentBiomeSpline == null)
        {
            _currentBiomeSpline = spline;
            transform.position = spline.transform.position;
            return;
        }

        _nextBiomeSpline = spline;
    }

    void Update()
    {
        if (splineAnimate.IsPlaying)
        {
            if (_nextBiomeReady && splineAnimate.NormalizedTime > 0.7)
            {
                _nextBiomeReady = false;
                splineAnimate.Container = _nextBiomeSpline.entrySpline;

                (_currentBiomeSpline, _nextBiomeSpline) = (_nextBiomeSpline, null);

                splineAnimate.NormalizedTime = 0;
                splineAnimate.Easing = SplineAnimate.EasingMode.EaseOut;
                _biomePort.OnReadyToUnload?.Invoke();
            }

            if (splineAnimate.NormalizedTime >= 1)
            {
                splineAnimate.Pause();
            }

        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.gameObject.transform.parent = sub;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player Has Exit SUB");
            other.gameObject.transform.parent = null;
        }
    }


    public void StartSub()
    {
        splineAnimate.Easing = SplineAnimate.EasingMode.EaseIn;
        splineAnimate.Container = _currentBiomeSpline.exitSpline;
        splineAnimate.NormalizedTime = 0;

        splineAnimate.Play();
        
        _biomePort.CommandStartLoadingNextBiome?.Invoke();

    }

    public void OnFinishedLoading()
    {
        _nextBiomeReady = true;
    }

/*
    private void UpdatePathSpeed(float newSpeed)
    {
        float prevProgress;
        prevProgress = !float.IsNaN(splineAnimate.NormalizedTime) ? splineAnimate.NormalizedTime : 0;
        splineAnimate.MaxSpeed = currentSpeed;
        splineAnimate.NormalizedTime = prevProgress;
    }
    */
}