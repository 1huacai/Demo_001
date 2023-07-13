using System;
namespace CoreFrameWork.TimeTool
{
    public class TimeKit
    {
#if Japan || SEA || TAIWAN||KOREA
        private static DateTime initTime = new DateTime (1970, 1, 1, 8, 0, 0);
#else
        private static readonly DateTime initTime = new DateTime(1970, 1, 1, 8, 0, 0);
#endif
#if Japan || SEA || TAIWAN || KOREA
        private static long OffsetSeconds = 0;
#endif

        // 服务器游戏开始时间
        public static int loginServerTime;

        /// <summary>
        /// 当前服务器的时间戳 (取整)
        /// </summary>
        public static long LastModifyServerTime { get; private set; }
        /// <summary>
        /// 当前服务器的时间戳的毫秒
        /// </summary>
        public static float LastModifyServerTimeMilliSecond { get; private set; }

        public static float LastModifyClientTime { get; private set; }



#if Japan || SEA || TAIWAN || KOREA
        /// <summary>
        /// 设置本地时区时间（海外版使用;）
        /// </summary>
        /// <param name="time"></param>
        public static void SetInitTime(DateTime time)
        {
            initTime = time;
        }

        /// <summary>
        /// 设置本地时区时间（海外版使用;）
        /// </summary>
        /// <param name="time">1970 1 1</param>
        /// <param name="_OffsetSeconds">偏移时间 秒</param>
        public static void SetInitTime(DateTime time,long _OffsetSeconds)
        {
            initTime = time;
            OffsetSeconds = _OffsetSeconds;
        }
#endif

        /// <summary>
        /// 获取当前时间的时间戳 (精确到秒)
        /// </summary>
        public static long GetNowUnixTime()
        {
            //float _offset = ConnectManager.manager().RealtimeSinceStartup - LastModifyClientTime;
            return LastModifyServerTime /*+ (long)_offset*/;
        }

        /// <summary>
        ///获取当前时间的时间戳  (精确到毫秒)
        /// </summary>
        /// <returns></returns>
        public static double GetNowUnixTimeToMilliSecond()
        {
            //float _offset = ConnectManager.manager().RealtimeSinceStartup - LastModifyClientTime;
            return (double)LastModifyServerTime + LastModifyServerTimeMilliSecond /*+ _offset*/;

        }

        /// <summary>
        /// 获取当前时间的时间戳(毫秒值)
        /// 与后台时间有最大1秒的误差.
        /// </summary>
        public static long GetNowUnixTimeMillis()
        {
            return GetMillisTime();

        }

        /// <summary>
        /// 获取当前时间datetime对象
        /// </summary>
        public static DateTime GetNowDateTime()
        {

            DateTime dtStart = initTime;
#if Japan || SEA|| TAIWAN||KOREA
            return dtStart.Add (new TimeSpan ((GetNowUnixTime ()+OffsetSeconds) * 10000000));
#else
            return dtStart.Add(new TimeSpan(GetNowUnixTime() * 10000000));
#endif
        }

        /// <summary>
        /// 得到当月的第一天
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(int Year, int Month)
        {
            return Convert.ToDateTime(Year.ToString() + "-" + Month.ToString() + "-1");
        }

        /// <summary>
        /// 得到当月的最后一天
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(int Year, int Month)
        {
            int Days = DateTime.DaysInMonth(Year, Month);
            return Convert.ToDateTime(Year.ToString() + "-" + Month.ToString() + "-" + Days.ToString());

        }

        /// <summary>
        /// 得到校正后时间，毫秒为单位
        /// </summary>
        /// <returns></returns>
        public static long GetMillisTime()
        {
            return (long)(GetNowUnixTimeToMilliSecond() * 1000);
        }
        /// <summary>
        /// 得到校正后时间，秒为单位
        /// </summary>
        /// <returns></returns>
        public static int GetSecondTime()
        {
            return (int)GetNowUnixTime();
        }

        //得到DateTime   从1970.1.1 8:00 开始 秒
        public static DateTime GetDateTime(int time = 0)
        {
            if (time == 0)
#if Japan || SEA || TAIWAN || KOREA
                time = GetSecondTime () + (int)OffsetSeconds;
            else
            {
                time = time + (int)OffsetSeconds;
            }
#else
                time = GetSecondTime();
#endif
            return initTime.AddSeconds(time);
        }

        //得到DateTime   从1970.1.1 8:00 开始 毫秒
        public static DateTime GetDateTimeMillis(long time)
        {
#if Japan || SEA|| TAIWAN||KOREA
            time = time + (long)(OffsetSeconds*1000.0f);
#endif
            return initTime.AddMilliseconds(time);
        }

        //得到DateTime   从1970.1.1 8:00 开始 毫秒
        public static DateTime GetDateTimeMin(int time)
        {
#if Japan || SEA|| TAIWAN||KOREA
            time = time + (int)OffsetSeconds;
#endif
            return initTime.AddMinutes(time / 60);
        }

        /// <summary>
        /// 得到当前时间的时间戳，未做时间修正，功能开发不能用
        /// </summary>
        /// <returns>长整型时间</returns>
        public static long CurrentTimeMillis()
        {
            return Convert.ToInt64(DateTime.Now.Subtract(initTime).TotalMilliseconds);
        }

        /// <summary>
        /// 修正时间 
        /// </summary>
        /// <param name="timestamp">当前服务器时间戳 （取整）</param>
        /// <param name="millisecond">当前服务器的毫秒</param>
        /// <param name="realtimeSinceStartup">客户端时间</param>
        public static void ModifyTime(long timestamp, int millisecond, float realtimeSinceStartup)
        {
            if (millisecond > 999)
            {
                millisecond = 999;
                Log.Error("millisecond 不能大于999.");
            }
            LastModifyServerTime = timestamp;//服务器响应时那一刻的时间戳
            LastModifyServerTimeMilliSecond = millisecond * 0.001f;
            LastModifyClientTime = realtimeSinceStartup;
        }
    }
}
    
