using UnityEngine;
using UnityEngine.Splines;

public class SubmarineController : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    private bool isRunning = false;

    [SerializeField] private float velocity = 2f;
    [SerializeField] private float brake = 2f;
    [SerializeField] private float maxSpeed = 5.5f;
    private float currentSpeed = 0f;

    [Header("Player Follow")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform sub;

    void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.Pause();  // startas avstängd
    }

    void Update()
    {
        if (isRunning)
        {
            currentSpeed = Mathf.Lerp(
                currentSpeed, maxSpeed, velocity * Time.deltaTime);

            UpdatePathSpeed(currentSpeed);
        }
        else
        {
            if (currentSpeed <= 0.01f)
            {
                currentSpeed = 0;
                splineAnimate.Pause();
                return;
            }

            currentSpeed = Mathf.Lerp(
                currentSpeed, 0f, brake * Time.deltaTime); 
                
            UpdatePathSpeed(currentSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = sub;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isRunning = false;
            Debug.Log("Player Has Exit SUB");
        }

        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = null;
        }
    }

    public void StartSub()
    {
        if (!isRunning)
        {
            isRunning = true;
            splineAnimate.Play();
        }
    }

    public void StopSub()
    {
        isRunning = false;
    }

    public void ToggleSub()
    {
        if (isRunning)
            StopSub();
        else
            StartSub();
    }

    private void UpdatePathSpeed(float newSpeed)
    {
        float prevProgress;
        prevProgress = !float.IsNaN(splineAnimate.NormalizedTime) ? splineAnimate.NormalizedTime : 0;
        splineAnimate.MaxSpeed = currentSpeed;
        splineAnimate.NormalizedTime = prevProgress;
    }
}