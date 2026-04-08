using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameTimeStamp
{
    public int Day;
    public int Hour;
    public int Minute;
    public int Second;

    public GameTimeStamp(int day, int hour, int minute, int second)
    {
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
    }

    public GameTimeStamp(GameTimeStamp timeStamp)
    {
        Day = timeStamp.Day;
        Hour = timeStamp.Hour;
        Minute = timeStamp.Minute;
        Second = timeStamp.Second;
    }

    public void AddOneSecond()
    {
        Second++;

        if (Second == 60)
        {
            Second = 0;
            Minute++;

            if (Minute == 60)
            {
                Minute = 0;
                Hour++;

                if (Hour == 24)
                {
                    Hour = 0;
                    Day++;
                }
            }
        }
    }
}