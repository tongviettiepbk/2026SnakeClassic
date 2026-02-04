using Newtonsoft.Json;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public enum TimeProvider
{
    BackEndServer = 0,
    Microsoft = 1,
}

public class TimeManager : MonoBehaviour
{

    private float timeEndFetchData;
    private DateTime fetchedDateTime;

    public DateTime GetTimeNow()
    {
        return DateTime.UtcNow;
    }

    public string GetWeekRangeString(DateTime date)
    {
        int delta = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
        DateTime firstDay = date.AddDays(-delta);
        DateTime lastDay = firstDay.AddDays(6);

        string weekRange = string.Format("{0:00}{1:00}{2:00}{3:00}{4}", firstDay.Day, firstDay.Month, lastDay.Day, lastDay.Month, lastDay.Year);
        return weekRange;
    }

    public string GetCurrentWeekRangeString()
    {
        return GetWeekRangeString(GetTimeNow());
    }

    public string GetPreviousWeekRangeString()
    {
        DateTime date = GetTimeNow().AddDays(-7);
        int delta = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
        DateTime firstDay = date.AddDays(-delta);
        DateTime lastDay = firstDay.AddDays(6);

        string weekRange = string.Format("{0:00}{1:00}{2:00}{3:00}{4}", firstDay.Day, firstDay.Month, lastDay.Day, lastDay.Month, lastDay.Year);
        return weekRange;
    }

    public TimeSpan GetDayTimeLeft()
    {
        DateTime now = GetTimeNow();
        DateTime nextDay = now.AddDays(1f).Date;
        return TimeSpan.FromTicks(nextDay.Ticks - now.Ticks);
    }

    public TimeSpan GetWeekTimeLeft()
    {
        DateTime now = GetTimeNow();

        int delta = now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)now.DayOfWeek - 1;
        DateTime lastDay = now.AddDays(6 - delta);
        lastDay = new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59);

        return TimeSpan.FromTicks(lastDay.Ticks - now.Ticks);
    }

    public TimeSpan GetMonthTimeLeft()
    {
        DateTime now = GetTimeNow();

        int totalDays = DateTime.DaysInMonth(now.Year, now.Month);
        int remainingDays = totalDays - now.Day;

        DateTime newDayNextMonth = now.AddDays(remainingDays + 1).Date;

        return TimeSpan.FromTicks(newDayNextMonth.Ticks - now.Ticks);
    }
}
