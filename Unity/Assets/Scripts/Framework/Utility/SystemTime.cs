using System;
/// <summary>
/// 时间
/// </summary>
public static class SystemTime
{
    /// <summary>
    /// Utc时间
    /// </summary>
    public static readonly DateTime UtcTime = new DateTime(1970, 1, 1);

    /// <summary>
    /// 将linux时间戳转为DateTime
    /// </summary>
    /// <param name="time">时间戳</param>
    /// <returns>DateTime</returns>
    public static DateTime ToDateTime(int time)
    {
        var datetStart = UtcTime.ToLocalTime();
        var toNow = new TimeSpan(long.Parse(time + "0000000"));
        return datetStart.Add(toNow);
    }

    /// <summary>
    /// 转为Linux时间戳
    /// </summary>
    /// <param name="time">DateTime(本地时间)</param>
    /// <returns>linux时间戳</returns>
    public static int Timestamp(this DateTime time)
    {
        var startTime = UtcTime.ToLocalTime();
        return (int)(time - startTime).TotalSeconds;
    }
}

