// ********************************************************************
//  作者:  OG.FH
//  创建时间: 2019-05-16 18:21
//  归属项目: XCore
//  文件名:   IEventHandler.cs
//  
//  目的:  
//          事件响应接口（用于 CSharpCallLua）
//         
//  更新记录(PS: 1、2019-4-29 调整优化了xxxx):
// 
//    
// *********************************************************************
namespace XCore.Event
{
    public interface IEventHandler
    {
        void EventHandle(int id, params object[] arg);
    }
}