using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Splines;

public class SubmarineController : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    private bool isRunning = false;

    [SerializeField] private float velocity = 2f;
    [SerializeField] private float brake = 2f;
    [SerializeField] private float maxSpeed = 20.5f;
    private float currentSpeed = 0f;

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
        BiomeManager.OnFinishLoadingBiome += OnFinishedLoading;
    }

    void OnDisable()
    {
        BiomeSubSplineHolder.OnSpawned -= SetNextBiomeSpline;
        BiomeManager.OnFinishLoadingBiome += OnFinishedLoading;
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
        if (isRunning)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, velocity * Time.deltaTime);

            UpdatePathSpeed(currentSpeed);

            if (_nextBiomeReady && splineAnimate.NormalizedTime > 0.7)
            {
                _nextBiomeReady = false;
                splineAnimate.Container = _nextBiomeSpline.entrySpline;

                (_currentBiomeSpline, _nextBiomeSpline) = (_nextBiomeSpline, null);

                splineAnimate.NormalizedTime = 0;
                BiomeManager.OnReadyToUnload?.Invoke();
            }

            if (splineAnimate.NormalizedTime >= 1)
            {
                isRunning = false;
                splineAnimate.Pause();
            } 

        }
        else
        {
            if (currentSpeed <= 0.01f)
            {
                currentSpeed = 0;
                splineAnimate.Pause();
                return;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, 0f, brake * Time.deltaTime);

            UpdatePathSpeed(currentSpeed);
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
            isRunning = false;
            Debug.Log("Player Has Exit SUB");
            other.gameObject.transform.parent = null;
        }
    }


    public void StartSub()
    {
        splineAnimate.Container = _currentBiomeSpline.exitSpline;
        splineAnimate.NormalizedTime = 0;


        isRunning = true;
        splineAnimate.Play();
        
        BiomeManager.CommandStartLoadingNextBiome?.Invoke();

    }

    public void StopSub()
    {
        isRunning = false;
    }

    public void OnFinishedLoading()
    {
        _nextBiomeReady = true;
    }

    private void UpdatePathSpeed(float newSpeed)
    {
        float prevProgress;
        prevProgress = !float.IsNaN(splineAnimate.NormalizedTime) ? splineAnimate.NormalizedTime : 0;
        splineAnimate.MaxSpeed = currentSpeed;
        splineAnimate.NormalizedTime = prevProgress;
    }
}