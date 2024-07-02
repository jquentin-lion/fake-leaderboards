using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public static class TimeUtils
    {

        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string DynamicFormat(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return timeSpan.ToString(@"d\d\ h\h");
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return timeSpan.ToString(@"h\h\ m\m");
            }
            else
            {
                return timeSpan.ToString(@"m\m\ s\s");
            }
        }
        
        public static bool IsSameTime(DateTime a, DateTime b)
        {
            return Math.Abs((a - b).TotalSeconds) <= 2;
        }
        
        public static float GetTimeSpentRatio(DateTime start, DateTime end, DateTime time)
        {
            start = start.Round();
            end = end.Round();
            time = time.Round();
            double totalTime = (end - start).TotalSeconds;
            double timeSpent = (time - start).TotalSeconds;
            return Mathf.Clamp01((float)timeSpent / (float)totalTime);
        }
        
        public static float GetCurrentTimeSpentRatio(DateTime start, DateTime end)
        {
            return GetTimeSpentRatio(start, end, DateTime.Now);
        }
        
        public static DateTime FromUnixTime(long unixTime)
        {
            return EPOCH.AddSeconds(unixTime);
        }
        
        public static long ToUnixTime(this DateTime dateTime)
        {
            return Convert.ToInt64((dateTime - EPOCH).TotalSeconds);
        }
        
        public static DateTime Round(this DateTime dateTime)
        {
            return dateTime.AddMilliseconds(-dateTime.Millisecond);
        }
        
        public static async Task NextFrame()
        {
            int currentFrame = Time.renderedFrameCount;
            while (currentFrame >= Time.renderedFrameCount)
                await Task.Yield();
        }
        
    }
}
