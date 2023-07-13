// ********************************************************************
//  作者:  FH
//  创建时间: 2019-05-13 18:50
//  归属项目: XCore
//  文件名:   EventManager.cs
//  
//  目的:  
// 
//         
//  更新记录(PS: 1、2019-4-29 调整优化了xxxx):
// 
//    
// *********************************************************************
namespace XCore.Event
{
    public static class GlobalEvent
    {
        private static IEvent EVENT;
        private static int EVENT_ID_START = 10000;
        public static int NewEventId
        {
            get { return EVENT_ID_START++; }
        }

        static GlobalEvent()
        {
            SetEvent(new BaseEvent());
        }

        public static void SetEvent(IEvent iEvent)
        {
            EVENT = iEvent;
        }

        public static void Destroy()
        {
            EVENT.Destroy();
        }
        #region Interface
        public static void AddEvent(int id, EventHandle handle)
        {
            EVENT.AddEvent(id, handle);
        }
        public static void AddEventSingle(int id, EventHandleSingle handle)
        {
            EVENT.AddEventSingle(id, handle);
        }
        public static void RemoveEvent(int id)
        {
            EVENT.RemoveEvent(id);
        }
        public static void RemoveEvent(int id, EventHandle handle)
        {
            EVENT.RemoveEvent(id, handle);
        }
        public static void RemoveEventSingle(int id, EventHandleSingle handle)
        {
            EVENT.RemoveEventSingle(id, handle);
        }
        public static void DispatchEventSingle(int id, object arg = null)
        {
            EVENT.DispatchEventSingle(id, arg);
        }
        public static void DispatchEvent(int id, params object[] arg)
        {
            EVENT.DispatchEvent(id, arg);
        }
        public static void DispatchEventSingleAsyn(int id, object arg = null)
        {
            EVENT.DispatchEventSingleAsyn(id, arg);
        }
        public static void DispatchEventAsyn(int id, params object[] arg)
        {
            EVENT.DispatchEventAsyn(id, arg);
        }
        #endregion Interface



        //====================================================================================
    }


    public delegate void EventHandle(params object[] arg);

    public delegate void EventHandleSingle(object arg = null);


    public class EventParam
    {
        public EventParam(params object[] arg)
        {
            Param = arg;
        }
        public object[] Param { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetParaValue<T>(int index)
        {
            if (index < Param.Length)
            {
                return (T)Param[index];
            }
            return default(T);
        }
    }
}