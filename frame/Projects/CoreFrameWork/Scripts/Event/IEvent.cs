using System;

namespace CoreFrameWork.Event
{
    public interface IEvent : IDisposable
    {
        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <param name="isOnce">是否使用一次</param>
        /// <param name="isFirst">是否插入到事件列表头位置</param>
        void AddEvent(Enum type, EventCallBack callBack, bool isOnce = false, bool isFirst = false);
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        void RemoveEvent(Enum type, EventCallBack callBack);
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        void RemoveEvent(Enum type);
        /// <summary>
        /// 移除事件
        /// </summary>
        void RemoveEvent();
        /// <summary>
        /// 分派事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="args">参数</param>
        void DispatchEvent(Enum type, params object[] args);
        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns></returns>
        bool ContainedEvent(Enum type);
        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <returns></returns>
        bool ContainedEvent(Enum type, EventCallBack callBack);
        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns></returns>
        bool ContainedOnceEvent(Enum type);
        /// <summary>
        /// 是否包含事件
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="callBack">事件处理回调</param>
        /// <returns></returns>
        bool ContainedOnceEvent(Enum type, EventCallBack callBack);
    }
}
