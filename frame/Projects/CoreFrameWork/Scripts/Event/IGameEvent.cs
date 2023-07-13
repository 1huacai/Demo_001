//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;

namespace CoreFrameWork.Event
{
    public interface IGameEvent
    {
        void AddEvent(Enum type, EventCallBack handler, bool isUseOnce = false, bool isFirst = false);
        void RemoveEvent(Enum type, EventCallBack handler);
        void RemoveEvent(Enum type);
        void ClearEvent();
        void DispatchEvent(Enum type, params object[] args);
        void DispatchAsyncEvent(Enum type, params object[] args);
        bool HasEvent(Enum type);
        void Dispose();
        bool IsDispose
        {
            get;
        }
    }
}