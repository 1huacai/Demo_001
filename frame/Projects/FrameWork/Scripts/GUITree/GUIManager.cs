using CoreFrameWork.Event;

namespace FrameWork
{

    /// <summary>
    /// TODO:对Node的SetActive方法需要以队列的方式执行以保证在回调中再次调用Goto后可能导致的不确定后果
    /// </summary>
    public class GUIManager : Event
    {


        /// <summary>
        /// 创建一个UITree管理器
        /// </summary>
        public GUIManager(GUITreeComponent root)
        {

        }
    }
}
