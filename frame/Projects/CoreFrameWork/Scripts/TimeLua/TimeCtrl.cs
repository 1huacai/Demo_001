using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CoreFrameWork.TimeLua
{
    public class TimeCtrl
    {
        static private TimeCtrl s_instance;

        public static TimeCtrl Instance
        {
            get { 
                if(s_instance == null)
                {
                    s_instance = new TimeCtrl();
                }
                return s_instance; 
            }
        }

        //static private bool isDestroy = false;

        #region 变量
        public delegate void CompleteCsCallLuaDelegate(double id);
        public delegate void EveryCsCallLuaDelegate(double id,double time);

        /// <summary>
        /// 为了减少lua 创建委托的开销，增加的全局委托方式
        /// </summary>
        public static CompleteCsCallLuaDelegate CompleteCsCallLua;

        /// <summary>
        /// 为了减少lua 创建委托的开销，增加的全局委托方式
        /// </summary>
        public static EveryCsCallLuaDelegate EveryCsCallLua;

        private Dictionary<double, double> completeHandles;
        private Dictionary<double, CompleteCsCallLuaDelegate> completeCallHandles;
        private Dictionary<double,double> completeTimes;
        private Dictionary<double, double> everySecondHandles;
        private Dictionary<double, double> everyMillisecondHandles;
        private Dictionary<double, double> everyMinuteHandles;
        private Dictionary<double, double> everyHandleTimesMax;
        private Dictionary<double, double> everyHandleTimes;

        private Dictionary<double,TimeCtrlData> addList;
        /// <summary>
        /// 服务器时间和系统时间差
        /// </summary>
        private double timeDifference = 0;
        private double lastTime = 0;
        private double lastMillisecond = 0;
        private double lastMinute = 0;
        private double timeId;
        private bool isInit = false;
        private bool isSetServerTime = false;
        private double frameTime = 0;
        private double localFrameTime = 0;
        private List<double> delList;

        #endregion

        public TimeCtrl()
        {

        }

        static public void Tick()
        {
            if(s_instance == null)
            {
                return;
            }
            s_instance.OnUpdate();
        }

        #region 生命周期
        void OnUpdate()
        {
            CheckAdd();
            CheckDel();
            CheckTime();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取当前系统时间 毫秒
        /// </summary>
        /// <returns></returns>
        public double GetSysTime()
        {
            return TimeHelper.GetSystemTime();
        }

        public double GetUnityTime()
        {
            return Time.realtimeSinceStartup * 1000;
        }

        /// <summary>
        /// 获取当前服务器时间(秒)
        /// </summary>
        /// <returns></returns>
        public double GetNowTime()
        {
            CheckInit();
            return (GetSysTime() + timeDifference) / 1000;
        }

        /// <summary>
        /// 获取当前服务器时间(毫秒)
        /// </summary>
        /// <returns></returns>
        public double GetNowTimeMs()
        {
            CheckInit();
            return GetSysTime() + timeDifference;
        }

        /// <summary>
        /// 设置服务器时间
        /// </summary>
        /// <param name="t"></param>
        public void SetServerTime(double t)
        {
            WriteServerTime(t);
        }

        /// <summary>
        /// 获取当前帧的服务器时间
        /// </summary>
        /// <returns></returns>
        public double GetFrameTime()
        {
            CheckInit();
            return frameTime;
        }

        /// <summary>
        /// 设置deadline时间点（秒）事件
        /// </summary>
        /// <param name="deadLine"></param>
        public double SetDeadLine(double deadLine)
        {
            return AddDeadLine(deadLine * 1000);
        }

        /// <summary>
        /// 设置deadline时间点（秒）事件，传入是字符串
        /// </summary>
        /// <param name="deadLineStr"></param>
        /// <returns></returns>
        public double SetDeadLineStr(string deadLineStr)
        {
            return AddDeadLine(double.Parse(deadLineStr) * 1000);
        }

        /// <summary>
        /// 设置deadline时间点（毫秒）事件
        /// </summary>
        /// <param name="deadLine"></param>
        public double SetDeadLineMs(double deadLine)
        {
            return AddDeadLine(deadLine);
        }

        /// <summary>
        /// 设置deadline时间点（毫秒）事件，传入的时间是字符串
        /// </summary>
        /// <param name="deadLineStr"></param>
        /// <returns></returns>
        public double SetDeadLineMsStr(string deadLineStr)
        {
            return AddDeadLine(double.Parse(deadLineStr));
        }

        /// <summary>
        /// 设置倒计时，单位秒
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public double SetCountDown(double sec)
        {
            return AddCountDown(sec);
        }

        /// <summary>
        /// 设置倒计时，单位秒
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public double SetCountDown(double sec,CompleteCsCallLuaDelegate completeCall)
        {
            double id = AddCountDown(sec);
            if(completeCall != null)
            {
                if (completeCallHandles.ContainsKey(id) == false)
                {
                    completeCallHandles.Add(id, completeCall);
                }
            }
           
            return id;
        }

        /// <summary>
        /// 设置倒计时，单位毫秒
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public double SetCountDownMs(double sec)
        {
            double id = AddCountDownMs(sec);
            return id;
        }

        /// <summary>
        /// 设置倒计时，单位毫秒
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public double SetCountDownMs(double sec, CompleteCsCallLuaDelegate completeCall)
        {
            double id = AddCountDownMs(sec);
            if (completeCall != null)
            {
                if (completeCallHandles.ContainsKey(id) == false)
                {
                    completeCallHandles.Add(id, completeCall);
                }
            }
            return id;
        }

        /// <summary>
        /// 设置每秒执行
        /// </summary>
        /// <returns></returns>
        public double SetEverySecond()
        {
            return SetEverySecondByTime(1);
        }

        /// <summary>
        /// 设置每多少秒执行
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double SetEverySecondByTime(double t)
        {
            return AddSecond(t);
        }

        /// <summary>
        /// 设置每毫秒执行（最小执行频率受帧率影响）
        /// </summary>
        /// <param name="everyHandle"></param>
        /// <returns></returns>
        public double SetEveryMs()
        {
            return SetEveryMsByTime(1);
        }

        /// <summary>
        /// 设置每多少毫秒执行(最小执行频率受帧率影响)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double SetEveryMsByTime(double t)
        {
            return AddMs(t);
        }

        /// <summary>
        /// 设置每分钟执行
        /// </summary>
        /// <returns></returns>
        public double SetEveryMinute()
        {
            return SetEveryMinuteByTime(1);
        }


        /// <summary>
        /// 设置每多少分钟执行
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double SetEveryMinuteByTime(double t)
        {
            return AddMinute(t);
        }


        /// <summary>
        /// 获取 (当前时间+指定时间)
        /// </summary>
        /// <param 增加时间="timeStamp"></param>
        /// <returns>返回所需时间</returns>
        public string GetAddTime(string timeStamp)
        {
            System.DateTime dtStart = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            System.TimeSpan toNow = new System.TimeSpan(lTime);
            System.DateTime dtResult = dtStart.Add(toNow);
            string timerStr = dtResult.ToShortDateString().ToString();
            return timerStr;
        }

        public void RemoveTime(double id)
        {
            if(delList == null)
            {
                delList = new List<double>();
            }
            delList.Add(id);
        }

        public void Clear()
        {
            CompleteCsCallLua = null;
            EveryCsCallLua = null;
            Init();
        }

        #endregion

        private void CheckInit()
        {
            if(isInit==false)
            {
                Init();
            }
        }

        private void Init()
        {
            completeHandles = new Dictionary<double, double>();
            completeTimes = new Dictionary<double, double>();
            everySecondHandles = new Dictionary<double, double>();
            everyMillisecondHandles = new Dictionary<double, double>();
            everyMinuteHandles = new Dictionary<double, double>();
            everyHandleTimesMax = new Dictionary<double, double>();
            everyHandleTimes = new Dictionary<double, double>();
            completeCallHandles = new Dictionary<double, CompleteCsCallLuaDelegate>();
            delList = new List<double>();
            timeDifference = 0;
            lastTime = 0;
            lastMillisecond = 0;
            lastMinute = 0;
            timeId = 0;
            isInit = true;
            isSetServerTime = false;
            frameTime = GetNowTimeMs();

        }

        private void CheckTime()
        {
            frameTime = GetNowTimeMs();
            localFrameTime = GetUnityTime();
            CheckComplete(frameTime);
            CheckSecond(frameTime, localFrameTime);
            CheckMs(frameTime, localFrameTime);
            CheckMinute(frameTime, localFrameTime);
            
        }

        private void CheckAdd()
        {
            if(addList == null||addList.Count==0)
            {
                return;
            }

            foreach(KeyValuePair<double,TimeCtrlData>item in addList)
            {
                AddOneTimeEvent(item.Value);
            }
            addList = new Dictionary<double, TimeCtrlData>();
        }

        private void CheckDel()
        {
            if(delList==null||delList.Count==0)
            {
                return;
            }

            for(int i = 0;i<delList.Count;i++)
            {
                Remove(delList[i]);
            }
            delList = new List<double>();
        }

        #region 实现方法

        private void AddOneTimeEvent(TimeCtrlData data)
        {
            int eventType = data.type;
            if(eventType == TimeEventType.deadLine)
            {
                AddDeadLineFun(data);
            }
            else if(eventType == TimeEventType.countDown)
            {
                AddCountDownFun(data);
            }
            else if(eventType == TimeEventType.countDownMs)
            {
                AddCountDownMsFun(data);
            }
            else if(eventType==TimeEventType.sec)
            {
                AddSecondFun(data);
            }
            else if(eventType == TimeEventType.ms)
            {
                AddMsFun(data);
            }
            else if(eventType == TimeEventType.minute)
            {
                AddMinuteFun(data);
            }
        }

        private bool CheckInDelList(double id)
        {
            if(delList==null||delList.Count == 0)
            {
                return false;
            }
            if(delList.IndexOf(id)>=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<double> checkCompleteRemoveList=new List<double>();
        private double checkCompleteCt = -1;
        private void CheckComplete(double t)
        {
            if(completeHandles == null ||completeHandles.Count == 0)
            {
                return;
            }
            //List<double> removeList = new List<double>();
            checkCompleteRemoveList.Clear();
            checkCompleteCt = -1;
            foreach(KeyValuePair<double,double>item in completeHandles)
            {
                if (CheckInDelList(item.Key) == true)
                {
                    continue;
                }
                if (completeTimes.ContainsKey(item.Key))
                {
                    checkCompleteCt = completeTimes[item.Key];
                    if (checkCompleteCt < t)
                    {

                        try
                        {
                            CompleteCsCallLua(item.Key);
                        }
                        catch (Exception e)
                        {
                            Log.Error("计时器完成的时候有报错：" +e);
                        }
                        if (completeCallHandles.ContainsKey(item.Key))
                        {
                            completeCallHandles[item.Key](item.Key);
                            completeCallHandles.Remove(item.Key);
                        }
                        checkCompleteRemoveList.Add(item.Key);
                    }
                }
            }

            if (checkCompleteRemoveList.Count > 0)
            {
                for (int i = 0; i < checkCompleteRemoveList.Count; i++)
                {
                    Remove(checkCompleteRemoveList[i]);
                }
            }


        }

        private void CheckEvery(double tid,double leftTime,double addtime)
        {
            double tInd = 0;
            if(everyHandleTimes.ContainsKey(tid))
            {
                tInd = everyHandleTimes[tid];
            }
            double tMax = 1;
            if(everyHandleTimesMax.ContainsKey(tid))
            {
                tMax = everyHandleTimesMax[tid];
            }
            tInd = tInd + addtime;
            if(tInd>=tMax)
            {
                if(everyHandleTimes.ContainsKey(tid))
                {
                    everyHandleTimes.Remove(tid);
                }
                try
                {
                    EveryCsCallLua(tid, (double)Mathf.FloorToInt((float)leftTime));
                }
                catch (Exception e)
                {
                    Log.Error("计时器every的时候有报错：" +e);
                    Remove(tid);//既然every计时器报错 那整个计时器就没有必要了 整个功能肯定是异常的
                    return;
                }
            }
            else
            {
                if(everyHandleTimes.ContainsKey(tid))
                {
                    everyHandleTimes[tid] = tInd;
                }
                else
                {
                    everyHandleTimes.Add(tid, tInd);
                }
            }

        }

        private void CheckSecond(double t,double lt)
        {
            if(everySecondHandles==null||everySecondHandles.Count==0)
            {
                return;
            }
            double passTime = lt - lastTime;
            if(passTime>=1000)
            {
                double leftTime = -1;
                foreach(KeyValuePair<double,double>item in everySecondHandles)
                {
                    if (CheckInDelList(item.Key) == true)
                    {
                        continue;
                    }
                    leftTime = 0;
                    if(completeTimes.ContainsKey(item.Key))
                    {
                        leftTime = (completeTimes[item.Key] - t) / 1000;
                    }
                    try
                    {
                        CheckEvery(item.Key, leftTime, passTime / 1000);
                    }
                    catch(System.Exception ex)
                    {
                       Log.Error(ex.StackTrace);
                    }

                }
                lastTime = lt;
            }
        }

        private void CheckMs(double t,double lt)
        {
            if(everyMillisecondHandles==null||everyMillisecondHandles.Count == 0)
            {
                return;
            }
            double passTime = lt - lastMillisecond;
            if(passTime>0)
            {
                double leftTime = 0;
                foreach(KeyValuePair<double,double>item in everyMillisecondHandles)
                {
                    if (CheckInDelList(item.Key) == true)
                    {
                        continue;
                    }
                    leftTime = 0;
                    if(completeTimes.ContainsKey(item.Key))
                    {
                        leftTime = completeTimes[item.Key]-t;
                    }
                    try
                    {
                        CheckEvery(item.Key, leftTime, passTime);
                    }
                    catch(System.Exception ex)
                    {
                     Log.Error(ex);
                    }
                    
                }
                lastMillisecond = lt;
            }
        }

        private void CheckMinute(double t,double lt)
        {
            if(everyMinuteHandles==null||everyMinuteHandles.Count ==0)
            {
                return;
            }
            double passTime = lt - lastMinute;
            if(passTime>=60000)
            {
                double leftTime = 0;
                foreach(KeyValuePair<double,double>item in everyMinuteHandles)
                {
                    if (CheckInDelList(item.Key) == true)
                    {
                        continue;
                    }
                    leftTime = 0;
                    if(completeTimes.ContainsKey(item.Key))
                    {
                        leftTime = (completeTimes[item.Key] - t) / 60000;
                    }
                    try
                    {
                        CheckEvery(item.Key, leftTime, passTime / 60000);
                    }
                    catch(System.Exception ex)
                    {
                        Log.Error(ex.StackTrace);
                    }
                    
                }
                lastMinute = lt;
            }
        }


        private void WriteServerTime(double t)
        {
            CheckInit();
            if (isSetServerTime!=true)
            {
                lastTime = 0;
                lastMinute = 0;
                lastMillisecond = 0;
                isSetServerTime = true;
            }
            
            double now = GetSysTime();
            timeDifference = t - now;
            frameTime = GetNowTimeMs();
            double lastSystemTime = GetSysTime();
        }

        private double GetTimeId()
        {
            timeId++;
            return timeId;
        }



        private void Add2AddList(TimeCtrlData data)
        {
            if(addList==null)
            {
                addList = new Dictionary<double, TimeCtrlData>();
            }
            addList.Add(data.id, data);
        }

        private double AddDeadLine(double deadLine)
        {
            CheckInit();
            if(deadLine<0)
            {
                return -1;
            }
            double tid = GetTimeId();
            TimeCtrlData data = new TimeCtrlData();
            data.id = tid;
            data.type = TimeEventType.deadLine;
            data.deadLine = deadLine;
            Add2AddList(data);
            return tid;
        }

        private double AddCountDown(double sec)
        {
            CheckInit();
            double tid = GetTimeId();
            double now = frameTime;
            double deadLine = now + sec * 1000;
            TimeCtrlData data = new TimeCtrlData();
            data.id = tid;
            data.type = TimeEventType.countDown;
            data.deadLine = deadLine;
            Add2AddList(data);
            return tid;
        }

        private double AddCountDownMs(double sec)
        {
            CheckInit();

            double tid = GetTimeId();
            double now = frameTime;
            double deadLine = now + sec;
            TimeCtrlData data = new TimeCtrlData();
            data.id = tid;
            data.type = TimeEventType.countDownMs;
            data.deadLine = deadLine;
            Add2AddList(data);
            return tid;
        }

        private double AddSecond(double t)
        {
            CheckInit();
            double tid = GetTimeId();
            TimeCtrlData data = new TimeCtrlData();
            data.type = TimeEventType.sec;
            data.id = tid;
            data.t = t;
            Add2AddList(data);
            return tid;
        }

        private double AddMs(double t)
        {
            CheckInit();
            double tid = GetTimeId();
            TimeCtrlData data = new TimeCtrlData();
            data.type = TimeEventType.ms;
            data.id = tid;
            data.t = t;
            Add2AddList(data);
            return tid;
        }

        private double AddMinute(double t)
        {
            CheckInit();
            double tid = GetTimeId();
            TimeCtrlData data = new TimeCtrlData();
            data.type = TimeEventType.minute;
            data.id = tid;
            data.t = t;
            Add2AddList(data);
            return tid;
        }

        private void AddDeadLineFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(completeHandles.ContainsKey(tid))
            {
                completeHandles[tid] = tid;
            }
            else
            {
                completeHandles.Add(tid, tid);
            }

            if(completeTimes.ContainsKey(tid))
            {
                completeTimes[tid] = data.deadLine;
            }
            else
            {
                completeTimes.Add(tid, data.deadLine);
            }

            if (everySecondHandles.ContainsKey(tid))
            {
                everySecondHandles[tid] = tid;
            }
            else
            {
                everySecondHandles.Add(tid, tid);
            }
        }

        private void AddCountDownFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(completeHandles.ContainsKey(tid))
            {
                completeHandles[tid] = tid;
            }
            else
            {
                completeHandles.Add(tid, tid);
            }

            if(completeTimes.ContainsKey(tid))
            {
                completeTimes[tid] = data.deadLine;
            }
            else
            {
                completeTimes.Add(tid, data.deadLine);
            }

            if (everySecondHandles.ContainsKey(tid))
            {
                everySecondHandles[tid] = tid;
            }
            else
            {
                everySecondHandles.Add(tid, tid);
            }
        }

        private void AddCountDownMsFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(completeHandles.ContainsKey(tid))
            {
                completeHandles[tid] = tid;
            }
            else
            {
                completeHandles.Add(tid, tid);
            }

            if(completeTimes.ContainsKey(tid))
            {
                completeTimes[tid] = data.deadLine;
            }
            else
            {
                completeTimes.Add(tid, data.deadLine);
            }

            if (everyMillisecondHandles.ContainsKey(tid))
            {
                everyMillisecondHandles[tid] = tid;
            }
            else
            {
                everyMillisecondHandles.Add(tid, tid);
            }
        }

        private void AddSecondFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(everySecondHandles.ContainsKey(tid))
            {
                everySecondHandles[tid] = tid;
            }
            else
            {
                everySecondHandles.Add(tid, tid);
            }

            if(everyHandleTimes.ContainsKey(tid))
            {
                everyHandleTimes[tid] = 0;
            }
            else
            {
                everyHandleTimes.Add(tid, 0);
            }

            if(everyHandleTimesMax.ContainsKey(tid))
            {
                everyHandleTimesMax[tid] = data.t;
            }
            else
            {
                everyHandleTimesMax.Add(tid, data.t);
            }
        }

        private void AddMsFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(everyMillisecondHandles.ContainsKey(tid))
            {
                everyMillisecondHandles[tid] = tid;
            }
            else
            {
                everyMillisecondHandles.Add(tid, tid);
            }
            if(everyHandleTimes.ContainsKey(tid))
            {
                everyHandleTimes[tid] = 0;
            }
            else
            {
                everyHandleTimes.Add(tid, 0);
            }

            if(everyHandleTimesMax.ContainsKey(tid))
            {
                everyHandleTimesMax[tid] = data.t;
            }
            else
            {
                everyHandleTimesMax.Add(tid, data.t);
            }
        }

        private void AddMinuteFun(TimeCtrlData data)
        {
            double tid = data.id;
            if(everyMinuteHandles.ContainsKey(tid))
            {
                everyMinuteHandles[tid] = tid;
            }
            else
            {
                everyMinuteHandles.Add(tid, tid);
            }

            if(everyHandleTimes.ContainsKey(tid))
            {
                everyHandleTimes[tid] = 0;
            }
            else
            {
                everyHandleTimes.Add(tid, 0);
            }

            if(everyHandleTimesMax.ContainsKey(tid))
            {
                everyHandleTimesMax[tid] = data.t;
            }
            else
            {
                everyHandleTimesMax.Add(tid, data.t);
            }
        }

        private void RemoveFromAddList(double id)
        {
            if(addList!=null&&addList.ContainsKey(id))
            {
                addList.Remove(id);
            }
        }

        private void Remove(double id)
        {
            if(completeHandles.ContainsKey(id))
            {
                completeHandles.Remove(id);
            }

            if (completeCallHandles.ContainsKey(id))
            {
                completeCallHandles.Remove(id);
            }

            if (completeTimes.ContainsKey(id))
            {
                completeTimes.Remove(id);
            }

            if(everySecondHandles.ContainsKey(id))
            {
                everySecondHandles.Remove(id);
            }

            if(everyMillisecondHandles.ContainsKey(id))
            {
                everyMillisecondHandles.Remove(id);
            }

            if(everyMinuteHandles.ContainsKey(id))
            {
                everyMinuteHandles.Remove(id);
            }

            if(everyHandleTimes.ContainsKey(id))
            {
                everyHandleTimes.Remove(id);
            }

            if(everyHandleTimesMax.ContainsKey(id))
            {
                everyHandleTimesMax.Remove(id);
            }

            RemoveFromAddList(id);
            
        }

        private void OnDestroy()
        {
            //isDestroy = true;
        }

        static public void DisposeClass()
        {
            if(s_instance!=null)
            {
                //_instance.RemoveUpdate();
                s_instance.Clear();
                s_instance = null;
            }
        }

        #endregion 
    }

    public class TimeCtrlData
    {
        public int type;
        public double deadLine;
        public double id;
        public double t = 1;

    }

    public class TimeEventType
    {
        public const int deadLine = 0;
        public const int countDown = 1;
        public const int countDownMs = 2;
        public const int sec = 3;
        public const int ms = 4;
        public const int minute = 5;
        
    }
}
