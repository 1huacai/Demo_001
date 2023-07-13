using System;
using System.Diagnostics;

// Usage: 统计代码执行时间
//  using (new StopTime("find"))
//  {
//      /******/
//  }

#if DEBUG
public class StopTime : IDisposable
{
    private string _tag;
    private Stopwatch _time;
    public StopTime(string tag)
    {
        _tag = tag;
        _time = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _time.Stop();
        float totalTime = _time.ElapsedMilliseconds;
        var desc =  string.Format("测试名称：{0}   总耗时：{1}", _tag, totalTime);
        UnityEngine.Debug.Log(desc);
    }
}
#endif


