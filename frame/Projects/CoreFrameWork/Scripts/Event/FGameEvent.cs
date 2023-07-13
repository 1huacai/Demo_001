//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrameWork.Event
{

    /// <summary>
    /// event execute
    /// </summary>
    public struct GameEventInfo
    {
        public EventCallBack eventHandler;
        public object[] args;
    }

    public class FGameEvent : MonoBehaviour, IGameEvent
    {

        /// <summary>
        /// The same event id max number.
        /// </summary>
        public static ushort SameSyncEventMax = 10;

        public static object m_locker = new object();
        /// <summary>
        /// event execute list
        /// </summary>
        private List<GameEventInfo> m_AsyncEventList;

        /// <summary>
        /// The event list.
        /// </summary>
        protected Dictionary<Enum, List<EventCallBack>> m_EventDic;

        /// <summary>
        /// just use once of event
        /// </summary>
        protected Dictionary<Enum, List<EventCallBack>> m_OnceEventDic;

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsDispose
        {
            get;
            set;
        }


        /// <summary>
        /// on execute event error
        /// </summary>
        protected virtual void EventHandlerError(Exception error)
        {
            throw error;
        }


        /// <summary>
        /// Adds the event.
        /// </summary>
        public virtual void AddEvent(Enum type, EventCallBack handler, bool useOnce = false, bool isFirst = false)
        {
            lock (m_locker)
            {
                if (useOnce)
                {
                    addEvent(type, handler, m_OnceEventDic, isFirst);
                }
                else
                {
                    addEvent(type, handler, m_EventDic, isFirst);
                }
            }
        }

        private void addEvent(Enum type, EventCallBack handler, Dictionary<Enum, List<EventCallBack>> dic, bool isFirst = false)
        {
            lock (m_locker)
            {
                List<EventCallBack> _handlerList = null;
                if (dic.TryGetValue(type, out _handlerList))
                {
                    if (!_handlerList.Contains(handler))
                    {
                        if (isFirst)
                        {
                            _handlerList.Add(handler);
                        }
                        else
                        {

                            _handlerList.Insert(0, handler);
                        }
                    }
                }
                else
                {
                    _handlerList = new List<EventCallBack>();
                    dic.Add(type, _handlerList);

                    if (isFirst)
                    {
                        _handlerList.Add(handler);
                    }
                    else
                    {

                        _handlerList.Insert(0, handler);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the event.
        /// </summary>
        public virtual void RemoveEvent(Enum type, EventCallBack handler)
        {
            lock (m_locker)
            {
                removeEvent(type, handler, m_EventDic);
                removeEvent(type, handler, m_OnceEventDic);
            }
        }
        private void removeEvent(Enum type, EventCallBack handler, Dictionary<Enum, List<EventCallBack>> dic)
        {
            lock (m_locker)
            {
                List<EventCallBack> _handlerList = null;
                if (dic.TryGetValue(type, out _handlerList))
                {
                    if (_handlerList.Contains(handler))
                    {
                        _handlerList.Remove(handler);
                    }
                }
            }
        }



        /// <summary>
        /// Removes All this type of event.
        /// </summary>
        public virtual void RemoveEvent(Enum type)
        {
            lock (m_locker)
            {


                m_EventDic.Remove(type);



                m_OnceEventDic.Remove(type);

            }
        }


        /// <summary>
        /// remove all event
        /// </summary>
        public virtual void ClearEvent()
        {
            m_EventDic.Clear();
            m_OnceEventDic.Clear();
        }


        /// <summary>
        /// Dispatch the specified type, target and args. sync type. 
        /// </summary>
        public virtual void DispatchEvent(Enum type, params object[] args)
        {
            lock (m_locker)
            {
                List<EventCallBack> _handlerList = null;
                if (m_EventDic.TryGetValue(type, out _handlerList))
                {
                    int _count = _handlerList.Count;
                    for (int i = _count - 1; i >= 0; i--)
                    {
                        if (i >= _handlerList.Count)
                        {
                            i = _handlerList.Count - 1;
                            if (i < 0)
                            {
                                continue;
                            }
                        }
                        EventCallBack handler = _handlerList[i];
                        if (null != handler)
                        {
                            handler(args);
                        }
                        else
                        {
                            _handlerList.RemoveAt(i);
                        }
                    }
                }

                List<EventCallBack> _onceHandlerList = null;

                if (m_OnceEventDic.TryGetValue(type, out _onceHandlerList))
                {
                    int _count = _onceHandlerList.Count;
                    for (int i = _count - 1; i >= 0; i--)
                    {
                        EventCallBack handler = _onceHandlerList[i];
                        if (null != handler)
                        {
                            handler(args);
                        }
                    }
                    _onceHandlerList.Clear();

                }
            }
        }

        /// <summary>
        /// Dispatch the specified type, target and args. async type, in idle frame execute function
        /// 抛出异步事件， 将事件加入异步事件列表，然后执行
        /// </summary>
        public virtual void DispatchAsyncEvent(Enum type, params object[] args)
        {
            lock (m_locker)
            {
                List<EventCallBack> _handlerList = null;
                if (m_EventDic.TryGetValue(type, out _handlerList))
                {


                    for (int i = _handlerList.Count - 1; i >= 0; i--)
                    {
                        m_AsyncEventList.Add(new GameEventInfo()
                        {
                            args = args,
                            eventHandler = _handlerList[i]
                        });
                    }

                    //for (int i = 0; i < _handlerList.Count; i++)
                    //{
                    //    m_AsyncEventList.Add(new GameEventInfo()
                    //    {
                    //        args = args,
                    //        eventHandler = _handlerList[i]
                    //    });
                    //}
                }


                List<EventCallBack> _onceHandlerList = null;
                if (m_OnceEventDic.TryGetValue(type, out _onceHandlerList))
                {


                    for (int i = _onceHandlerList.Count - 1; i >= 0; i--)
                    {
                        EventCallBack handler = _onceHandlerList[i];
                        m_AsyncEventList.Add(new GameEventInfo()
                        {
                            args = args,
                            eventHandler = handler
                        });

                    }

                    //for (int i = 0; i < _onceHandlerList.Count; i++)
                    //{
                    //    EventCallBack handler = _onceHandlerList[i];
                    //    m_AsyncEventList.Add(new GameEventInfo()
                    //    {
                    //        args = args,
                    //        eventHandler = handler
                    //    });

                    //}

                    _onceHandlerList.Clear();
                }
            }

        }


        /// <summary>
        /// 是否有 type 类型的事件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool HasEvent(Enum type)
        {
            return m_EventDic.ContainsKey(type);
        }


        /// <summary>
        /// 异步消息循环
        /// </summary>
        public virtual void UpdateEvent()
        {
            for (short i = 500; m_AsyncEventList.Count > 0 && i > 0; i--)
            {
                int _count = m_AsyncEventList.Count;
                GameEventInfo taskEvent = m_AsyncEventList[_count - 1];
                m_AsyncEventList.Remove(taskEvent);
                try
                {
                    taskEvent.eventHandler(taskEvent.args);
                }
                catch (Exception error)
                {
                    try
                    {
                        EventHandlerError(error);
                    }
                    catch (Exception error2)
                    {
                    }
                }
            }
        }


        public virtual void Awake()
        {
            IsDispose = false;
            m_AsyncEventList = new List<GameEventInfo>();
            m_EventDic = new Dictionary<Enum, List<EventCallBack>>();
            m_OnceEventDic = new Dictionary<Enum, List<EventCallBack>>();

        }
        public virtual void Start()
        {


        }


        /// <summary>
        /// 销毁，回收处理
        /// </summary>
        public virtual void Dispose()
        {
            IsDispose = true;
            m_OnceEventDic.Clear();
            m_EventDic.Clear();
            m_AsyncEventList.Clear();
            m_AsyncEventList.Clear();
            m_EventDic.Clear();
            m_OnceEventDic.Clear();
        }



    }

}








