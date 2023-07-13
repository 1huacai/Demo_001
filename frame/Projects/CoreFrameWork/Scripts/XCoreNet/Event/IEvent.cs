// ********************************************************************
//  作者:  FH
//  创建时间: 2019-05-13 18:50
//  归属项目: XCore
//  文件名:   IEvent.cs
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
    public interface IEvent
    {
        void Destroy();
        void AddEvent(int id, EventHandle handle);
        void AddEventSingle(int id, EventHandleSingle handle);

        void RemoveEvent(int id);
        void RemoveEvent(int id, EventHandle handle);
        void RemoveEventSingle(int id, EventHandleSingle handle);

        void DispatchEvent(int id, params object[] arg);
        void DispatchEventSingle(int id, object arg = null);

        void DispatchEventAsyn(int id, params object[] arg);
        void DispatchEventSingleAsyn(int id, object arg = null);

        void Update();

        //IEventHandler EventHandler { get; set; }
        //void BindHandleInterface(IEventHandler iHandler);
    }
}