using UnityEngine;
using UnityEngine.Splines;

public class SubmarineController : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    private bool isRunning = false;

    [SerializeField] private float velocity = 2f;
    [SerializeField] private float brake = 2f;
    private float currentSpeed = 0f;

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
                currentSpeed, splineAnimate.MaxSpeed, velocity * Time.deltaTime); 
        }
        else
        {
            currentSpeed = Mathf.Lerp( 
                currentSpeed, 0f, brake * Time.deltaTime);

            if (currentSpeed <= 0.01f)
                splineAnimate.Pause();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Has Exit SUB"); 
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
}