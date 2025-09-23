using UnityEngine;
using UnityEngine.Splines; 

public class SubmarineController : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    private bool isRunning = false;

    [SerializeField] private float velocity = 2f;
    [SerializeField] private float brake = 2f;
    [SerializeField] private float maxSpeed = 4.5f;
    private float currentSpeed = 0f;

    void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.Pause();  // starta avstängd 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartSub();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StopSub();
        }

        //Mathf.MoveTowards(a, b, c) is a Unity function that: Moves a value A toward a target value B By a maximum step size of C(per frame). 
        //VET INTE OM DESSA FUNGERAR ENS?? 
        if (isRunning)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed, splineAnimate.MaxSpeed, velocity * Time.deltaTime); 
            Debug.Log(currentSpeed); 
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed, 0f, brake * Time.deltaTime);
            Debug.Log(currentSpeed);
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
    //den stannar inte efter ett klick, måste klicka igen när currentSpeed <= 0.01f för att den ska stanna?? 
    public void StopSub()
    {
        isRunning = false;

        if (currentSpeed <= 0.01f)
        {
            splineAnimate.Pause();
        }
    }
}