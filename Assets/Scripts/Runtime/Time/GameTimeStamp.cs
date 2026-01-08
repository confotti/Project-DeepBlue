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

    public GameTimeStamp(int day, int hour, int minute, int second)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        this.second = second;
    }

    public GameTimeStamp(GameTimeStamp timeStamp)
    {
        this.day = timeStamp.day;
        this.hour = timeStamp.hour;
        this.minute = timeStamp.minute;
        this.second = timeStamp.second;
    }

    // Convert to seconds
    public static int TimeStampInSeconds(GameTimeStamp ts)
    {
        // 1 day = 20 hours
        int seconds = 0;
        seconds += ts.day * 24 * 60 * 60;
        seconds += ts.hour * 60 * 60;
        seconds += ts.minute * 60;
        seconds += ts.second;
        return seconds;
    }
}