using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Internal Clock")]
    [SerializeField]
    GameTimeStamp timestamp;
    public float timeScale = 1.0f;

    [Header("Day and Night Cycle")]
    //transform the directional light
    public Transform sunTransform;
    Vector3 sunAngle; 

    private void Awake()
    {
        //if there is more than one instance, destroy the extra
        if (Instance != null && Instance != this)
        {
            Destroy(this); 
        }
        else
        {
            //set the static instance to this instance 
            Instance = this; 
        }
    }

    private void Start()
    {
        //initilise the time stamp 
        timestamp = new GameTimeStamp(1, 6, 0);
        StartCoroutine(TimeUpdate()); 
    }

    IEnumerator TimeUpdate()
    {
        while (true)
        {
            Tick(); 

            yield return new WaitForSeconds(1 / timeScale); 
        }
    }

    void Tick()
    {
        timestamp.UpdateClock();
        UpdateSunMovement(); 
    }

    void UpdateSunMovement()
    {
        //converts the current time to minutes 
        int timeInMinutes = GameTimeStamp.HoursToMinutes(timestamp.hour) + timestamp.minute;

        //during daytime
        //sun moves .2 degees a minute
        //during nighttime
        //sun moves .6 degrees a minute

        float sunAngle = 0;
        if (timeInMinutes <= 15 * 60)
        {
            sunAngle = .2f * timeInMinutes; 
        }
        else if(timeInMinutes > 15 * 60)
        {
            sunAngle = 180f + .6f * (timeInMinutes - (15 * 60)); 
        }

        //Apply angle to the dir light
        //sunTransform.eulerAngles = new Vector3(sunAngle, 0, 0); 
        this.sunAngle = new Vector3(sunAngle, 0, 0); 
    }

    private void Update()
    {
        sunTransform.rotation = Quaternion.Slerp(sunTransform.rotation, Quaternion.Euler(sunAngle), 1f * Time.deltaTime); 
    }

    //function to skip time

    public void SkipTime(GameTimeStamp timeToSkipTo)
    {
        //converts to minutes
        int timeToSkipInMinutes = GameTimeStamp.TimeStampInMinutes(timeToSkipTo);
        Debug.Log("Time Skip to:" + timeToSkipInMinutes);
        int timeNowInMinutes = GameTimeStamp.TimeStampInMinutes(timestamp);
        Debug.Log("Time Now: " + timeNowInMinutes);

        int differenceInMinutes = timeToSkipInMinutes - timeNowInMinutes; 
        Debug.Log("Skip " + differenceInMinutes + "minutes");

        //check if the timestamp to skip to has already been reached
        if (differenceInMinutes <= 0) return;

        for (int i = 0; i < differenceInMinutes; i++)
        {
            Tick(); 
        }
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        //return a cloned instance
        return new GameTimeStamp(timestamp); 
    }
}
