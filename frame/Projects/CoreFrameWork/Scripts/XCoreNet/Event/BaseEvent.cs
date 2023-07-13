// ********************************************************************
//  作者:  FH
//  创建时间: 2019-05-13 19:03
//  归属项目: XCore
//  文件名:   BaseEvent.cs
//  
//  目的:  
// 
//         
//  更新记录(PS: 1、2019-4-29 调整优化了xxxx):
// 
//    
// *********************************************************************

using CoreFrameWork;
using System;
using System.Collections.Generic;

namespace XCore.Event
{
    public class BaseEvent : IEvent
    {
        protected static readonly List<WeakReference> LIST = new List<WeakReference>();
        protected Dictionary<int, List<EventHandle>> listener = new Dictionary<int, List<EventHandle>>();
        protected Dictionary<int, List<EventHandleSingle>> listenerSingle = new Dictionary<int, List<EventHandleSingle>>();

        protected List<int> asynEventWaitList = new List<int>();
        protected Dictionary<int, object[]> asyncEventParam = new Dictionary<int, object[]>();

        protected List<int> asynEventWaitListSingle = new List<int>();
        protected Dictionary<int, object> asyncEventParamSingle = new Dictionary<int, object>();

        public static void UpdateAll()
        {
            if (LIST.Count > 0)
            {
                for (int i = LIST.Count - 1; i >= 0; i--)
                {
                    if (LIST[i].IsAlive)
                    {
                        (LIST[i].Target as BaseEvent).Update();
                    }
                    else
                    {
                        LIST.RemoveAt(i);
                    }
                }
            }
        }

        public static void DestroyAll()
        {
            LIST.Clear();
        }

        public BaseEvent()
        {
            LIST.Add(new WeakReference(this));
        }

        //TODO:关于是否添加 Once 事件？
        public virtual void AddEvent(int id, EventHandle handle)
        {
            List<EventHandle> list;
            if (!listener.TryGetValue(id, out list))
            {
                list = new List<EventHandle>();
                listener.Add(id, list);
            }
            if (!list.Contains(handle))
            {
                list.Add(handle);
            }
        }
        public virtual void AddEventSingle(int id, EventHandleSingle handle)
        {
            List<EventHandleSingle> list;
            if (!listenerSingle.TryGetValue(id, out list))
            {
                list = new List<EventHandleSingle>();
                listenerSingle.Add(id, list);
            }
            if (!list.Contains(handle))
            {
                list.Add(handle);
            }
        }
        //----------------------------------------------------
        public virtual void RemoveEvent(int id)
        {
            listener.Remove(id);
            listenerSingle.Remove(id);
        }
        public virtual void RemoveEvent(int id, EventHandle handle)
        {
            List<EventHandle> list;
            if (listener.TryGetValue(id, out list))
            {
                list.Remove(handle);
            }
        }
        public virtual void RemoveEventSingle(int id, EventHandleSingle handle)
        {
            List<EventHandleSingle> list;
            if (listenerSingle.TryGetValue(id, out list))
            {
                list.Remove(handle);
            }
        }
        //----------------------------------------------------
        public void DispatchEvent(int id, params object[] arg)
        {
            OnDispatchEvent(id, arg);
        }
        public void DispatchEventSingle(int id, object arg = null)
        {
            OnDispatchEvent(id, arg);
        }
        //----------------------------------------------------
        public void DispatchEventAsyn(int id, params object[] arg)
        {
            if (!asyncEventParam.ContainsKey(id))
            {
                asynEventWaitList.Add(id);
            }
            asyncEventParam[id] = arg;
        }
        public void DispatchEventSingleAsyn(int id, object arg = null)
        {
            if (!asyncEventParamSingle.ContainsKey(id))
            {
                asynEventWaitListSingle.Add(id);
            }
            asyncEventParamSingle[id] = arg;
        }
        //----------------------------------------------------
        public virtual void Update()
        {
            int id;
            //----------------------
            for (int i = 0; i < asynEventWaitList.Count; i++)
            {
                id = asynEventWaitList[i];
                if(asyncEventParam.ContainsKey(id))
                {
                    OnDispatchEvent(id, asyncEventParam[id]);
                }
                
            }
            //----------------------
            for (int i = 0; i < asynEventWaitListSingle.Count; i++)
            {
                id = asynEventWaitListSingle[i];
                if(asyncEventParamSingle.ContainsKey(id))
                {
                    OnDispatchEvent(id, asyncEventParamSingle[id]);
                }
               
            }
            asynEventWaitList.Clear();
            asynEventWaitListSingle.Clear();
        }

        //public IEventHandler EventHandler { get; set; }

        //public void BindHandleInterface(IEventHandler iHandler)
        //{
        //    EventHandler = iHandler;
        //}

        protected virtual void OnDispatchEvent(int id, params object[] arg)
        {
            if (listener.ContainsKey(id))
            {
                List<EventHandle> handleList = listener[id];
                if (handleList.Count > 0)
                {
                    for (int i = handleList.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            handleList[i].Invoke(arg);
                        }
                        catch (Exception e)
                        {
                            Log.Error("派发事件错误"+ e);
                        }

                    }
                }
            }
        }

        protected virtual void OnDispatchEvent(int id, object arg = null)
        {
            if (listenerSingle.ContainsKey(id))
            {
                List<EventHandleSingle> handleList = listenerSingle[id];
                if (handleList.Count > 0)
                {
                    for (int i = handleList.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            handleList[i].Invoke(arg);
                        }
                        catch (Exception e)
                        {
                            Log.Error("派发事件错误"+ e);
                        }
                    }
                }
            }
        }

        public virtual void Destroy()
        {
            listener.Clear();
            listenerSingle.Clear();
            asynEventWaitList.Clear();
            asyncEventParam.Clear();
            asynEventWaitListSingle.Clear();
            asyncEventParamSingle.Clear();
        }
    }
}