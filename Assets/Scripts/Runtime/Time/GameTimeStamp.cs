using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameTimeStamp
{
    public int day;
    public int hour;
    public int minute;
    public int second;

    // Constructor
    public GameTimeStamp(int day, int hour, int minute, int second)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        this.second = second;
    }

    // Copy constructor
    public GameTimeStamp(GameTimeStamp timeStamp)
    {
        this.day = timeStamp.day;
        this.hour = timeStamp.hour;
        this.minute = timeStamp.minute;
        this.second = timeStamp.second;
    }

    // Update the clock (1 tick = 1 second)
    public void UpdateClock()
    {
        second++;

        if (second >= 60)
        {
            second = 0;
            minute++;
        }

        if (minute >= 60)
        {
            minute = 0;
            hour++;
        }

        // 20-hour days in your world
        if (hour >= 20)
        {
            hour = 0;
            day++;
        }
    }

    // Convert whole timestamp to seconds
    public static int TimeStampInSeconds(GameTimeStamp ts)
    {
        // 1 day = 20 hours
        int seconds = 0;
        seconds += ts.day * 20 * 60 * 60;
        seconds += ts.hour * 60 * 60;
        seconds += ts.minute * 60;
        seconds += ts.second;
        return seconds;
    }

    // Optional: total hours (useful for sun rotation)
    public static float TimeStampInHours(GameTimeStamp ts)
    {
        return ts.day * 20f + ts.hour + (ts.minute / 60f) + (ts.second / 3600f);
    }
}