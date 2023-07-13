using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类说明：事件类
/// 作者：刘耀鑫
/// </summary>
/// 

public delegate void SDKEventCallBack(params object[] args);

public class SDKEvent : MonoBehaviour
{
    /// <summary>
    /// 事件字典
    /// </summary>
    private Dictionary<Enum, List<SDKEventCallBack>> _eventDic = new Dictionary<Enum, List<SDKEventCallBack>>();
    /// <summary>
    /// 使用一次事件字典
    /// </summary>
    private Dictionary<Enum, List<SDKEventCallBack>> _onceEventDic = new Dictionary<Enum, List<SDKEventCallBack>>();

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="callBack">事件处理回调</param>
    /// <param name="isOnce">是否使用一次</param>
    /// <param name="isFirst">是否插入到事件列表头位置</param>
    public void AddEvent(Enum type, SDKEventCallBack callBack, bool isOnce = false, bool isFirst = false)
    {
        Dictionary<Enum, List<SDKEventCallBack>> currentDic = isOnce ? _onceEventDic : _eventDic;

        List<SDKEventCallBack> list = null;
        if (currentDic.ContainsKey(type))
        {
            list = currentDic[type];
        }
        else
        {
            list = new List<SDKEventCallBack>();
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
    public void RemoveEvent(Enum type, SDKEventCallBack callBack)
    {
        List<SDKEventCallBack> list = null;
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
        List<SDKEventCallBack> eventList = null;
        if (_eventDic.TryGetValue(type, out eventList))
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                eventList[i](args);
            }
        }
        if (_onceEventDic.TryGetValue(type, out eventList))
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                eventList[i](args);
            }
            eventList.Clear();
        }
        //            if (args.Length > 0)
        //            {
        //                Log.Warning("Role: " + args[0] +" Event: " + type);   
        //            }
        //            else
        //            {
        //                Log.Warning("Event: " + type);   
        //            }
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
    public bool ContainedEvent(Enum type, SDKEventCallBack callBack)
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
    public bool ContainedOnceEvent(Enum type, SDKEventCallBack callBack)
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
