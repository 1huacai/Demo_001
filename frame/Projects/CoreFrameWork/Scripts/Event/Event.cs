using CoreFrameWork.Collections;
using System;
using System.Collections.Generic;

namespace CoreFrameWork.Event
{
    public class Event : IEvent
    {
        public bool isEditor;
        public Dictionary<Enum, int> MonitorDic = null;

        /// <summary>
        /// 事件字典
        /// </summary>
        private Dictionary<Enum, MYList<EventCallBack>> _eventDic = new Dictionary<Enum, MYList<EventCallBack>>();
        /// <summary>
        /// 使用一次事件字典
        /// </summary>
        private Dictionary<Enum, MYList<EventCallBack>> _onceEventDic = new Dictionary<Enum, MYList<EventCallBack>>();

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <param name="isOnce">是否使用一次</param>
        /// <param name="isFirst">是否插入到事件列表头位置</param>
        public void AddEvent(Enum type, EventCallBack callBack, bool isOnce = false, bool isFirst = false)
        {
            Dictionary<Enum, MYList<EventCallBack>> currentDic = isOnce ? _onceEventDic : _eventDic;

            MYList<EventCallBack> list = null;
            if (currentDic.ContainsKey(type))
            {
                list = currentDic[type];
            }
            else
            {
                list = new MYList<EventCallBack>();
                currentDic[type] = list;
            }
            list.Remove(callBack);
            if (isFirst)
            {
                list.Insert(0, callBack);
            }
            else
            {
                list.Add(callBack);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        public void RemoveEvent(Enum type, EventCallBack callBack)
        {
            MYList<EventCallBack> list = null;
            if (_eventDic.ContainsKey(type))
            {
                list = _eventDic[type];
                list.Remove(callBack);
            }
            if (_onceEventDic.ContainsKey(type))
            {
                list = _onceEventDic[type];
                list.Remove(callBack);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        public void RemoveEvent(Enum type)
        {
            _eventDic.Remove(type);
            _onceEventDic.Remove(type);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveEvent()
        {
            _eventDic.Clear();
            _onceEventDic.Clear();
        }

        /// <summary>
        /// 分派事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="args">参数</param>
        public void DispatchEvent(Enum type, params object[] args)
        {

            if (isEditor)
            {
                if (null == MonitorDic)
                {
                    MonitorDic = new Dictionary<Enum, int>();
                }


                if (MonitorDic.ContainsKey(type))
                {
                    MonitorDic[type]++;
                }
                else
                {
                    MonitorDic.Add(type, 1);
                }

            }

            MYList<EventCallBack> eventList = null;
            if (_eventDic.TryGetValue(type, out eventList))
            {
                for (int i = 0; i < eventList.Size; i++)
                {
                    eventList[i](args);
                }
 

            }
            if (_onceEventDic.TryGetValue(type, out eventList))
            {
                for (int i = 0; i < eventList.Size; i++)
                {
                    eventList[i](args);
                }
 
                eventList.Clear();
            }
 
        }


        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns></returns>
        public bool ContainedEvent(Enum type)
        {
            return _eventDic.ContainsKey(type);
        }

        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <returns></returns>
        public bool ContainedEvent(Enum type, EventCallBack callBack)
        {
            bool b = _eventDic.ContainsKey(type);
            if (b)
            {
                b = _eventDic[type].Contains(callBack);
            }
            return b;
        }

        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns></returns>
        public bool ContainedOnceEvent(Enum type)
        {
            return _onceEventDic.ContainsKey(type);
        }

        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <returns></returns>
        public bool ContainedOnceEvent(Enum type, EventCallBack callBack)
        {
            bool b = _onceEventDic.ContainsKey(type);
            if (b)
            {
                b = _onceEventDic[type].Contains(callBack);
            }
            return b;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            _eventDic.Clear();
            _onceEventDic.Clear();
        }
    }
}
