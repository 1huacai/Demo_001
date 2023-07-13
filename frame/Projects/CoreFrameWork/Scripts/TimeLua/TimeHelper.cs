using UnityEngine;
using System.Collections;
using System;
namespace CoreFrameWork.TimeLua
{
    public class TimeHelper
    {


        private static DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        private static DateTime now;


        static public double GetSystemTime()
        {
            now = System.DateTime.Now;
            double ts = (now - startTime).TotalMilliseconds;
            return ts;
        }
    }
}
