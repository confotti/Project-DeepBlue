using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GameTimeStamp 
{
    public int day;
    public int hour;
    public int minute; 

    //class constructor - stes up the class
    public GameTimeStamp (int day, int hour, int minute)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute; 
    }

    //create a new timestamp from an existing one
    public GameTimeStamp(GameTimeStamp timeStamp)
    {
        this.day = timeStamp.day;
        this.hour = timeStamp.hour;
        this.minute = timeStamp.minute; 
    }

    //function to incrrese the time correctly
    public void UpdateClock()
    {
        minute++; 

        //60 minute in one hour
        if(minute >= 60)
        {
            //reset minutes
            minute = 0;
            hour++; 
        }

        //20 hours in 1 day
        if (hour >= 20)
        {
            //reset hours
            hour = 0;
            day++; 
        }
    }

    //converts hours to minutes 
    public static int HoursToMinutes(int hour)
    {
        //60 minutes = 1 hour
        return hour * 60; 
    }

    //returns the current timestamp in minutes
    public static int TimeStampInMinutes(GameTimeStamp timeStamp)
    {
        return (HoursToMinutes(timeStamp.day * 20)+ HoursToMinutes(timeStamp.hour) + timeStamp.minute); 
    }
}
