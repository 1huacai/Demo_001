using System;
using System.Collections.Generic;
using CoreFrameWork;
using FrameWork;

namespace YoukiaGame
{

    /// <summary>
    /// TODO:对Node的SetActive方法需要以队列的方式执行以保证在回调中再次调用Goto后可能导致的不确定后果
    /// </summary>
    public class GUIStackManager : GUIManager
    {

        private readonly Stack<GUITreeComponent> m_nodeCache = new Stack<GUITreeComponent>();
        private readonly Stack<string> m_navBackStack = new Stack<string>();
        private string m_currentPath;
        private List<KeyValuePair<GUITreeComponent, bool>> m_messageDic = new List<KeyValuePair<GUITreeComponent, bool>>();

        public string GetLastComponent()
        {
            return m_navBackStack.Peek();

        }

        /// <summary>
        /// Manager的当前UI路径
        /// </summary>
        public string CurrentPath
        {
            get { return m_currentPath; }
            private set
            {
                m_navBackStack.Push(m_currentPath);
                m_currentPath = value;
            }
        }

        /// <summary>
        /// Manager当前最上层激活节点
        /// </summary>
        public GUITreeComponent CurrentComponent { get; private set; }


        /// <summary>
        /// Manager的默认根节点
        /// </summary>
        public GUITreeComponent RootComponent { get; private set; }

        /// <summary>
        /// 创建一个UITree管理器
        /// </summary>
        public GUIStackManager(GUITreeComponent root) : base(root)
        {
            RootComponent = root;
            CurrentComponent = RootComponent;
            m_nodeCache.Push(RootComponent);
            m_currentPath = root.Name;
            //RootComponent.SetActive(true);
        }


        public void ResetRoot(GUITreeComponent root)
        {

            m_nodeCache.Clear();
            //    _navForwardStack.Clear();
            m_navBackStack.Clear();

            RootComponent = root;
            CurrentComponent = RootComponent;
            CurrentComponent.CurrentParent = null;
            m_nodeCache.Push(RootComponent);
            m_currentPath = root.Name;

        }

        /// <summary>
        /// 跳转至指定Path的UI节点
        /// </summary>
        /// <param name="treePath">指定的UI路径,节点间以'/'隔开,例如roo/node1/node2</param>
        /// <returns>返回对应路径的UI节点,如果没有对应的节点,则UITree不做任何改变并返回null</returns>
        public GUITreeComponent Goto(string treePath, List<GUITreeComponent> nodes, bool sendMsg)
        {
            return _goto(treePath, nodes, sendMsg);
        }


        /// <summary>
        /// 跳转至指定名称的子节点
        /// </summary>
        /// <returns>指定的子节点,如果对应名称的子节点不存在,则UITree不做任何改变并返回null</returns>
        public GUITreeComponent GotoChild(string name, bool sentMsg)
        {
            return Goto(CurrentPath + "/" + name, null, sentMsg);
        }


        public GUITreeComponent GetNode(string paths)
        {

            GUITreeComponent current = RootComponent;

            if (current.Name == paths)
                return current;

            List<GUITreeComponent> lists = new List<GUITreeComponent>();

            GetChildNodeLoop(ref lists, RootComponent);

            if (lists.Count > 0)
            {
                for (int i = 0; i < lists.Count; i++)
                {
                    if (lists[i].Name == paths)
                        return lists[i];
                }
            }

            return null;
        }

        private GUITreeComponent[] _getTreeNodes(string[] paths)
        {
            GUITreeComponent[] result = new GUITreeComponent[paths.Length];

            if (paths[0] != RootComponent.Name)
            {
                return null;
            }
            GUITreeComponent current = RootComponent;
            result[0] = current;
            for (int i = 1; i < paths.Length; i++)
            {
                result[i] = current.GetChild(paths[i]);
                if (result[i] == null)
                {
                    return null;
                }
                current = result[i];
            }

            return result;
        }
        void SetComponentActive(GUITreeComponent component, bool bo)
        {
            component.SetActive(bo);

            //移除相同
            for (int i = 0; i < m_messageDic.Count; i++)
            {
                if (m_messageDic[i].Key == component)
                {
                    m_messageDic.RemoveAt(i);
                }

            }

            m_messageDic.Add(new KeyValuePair<GUITreeComponent, bool>(component, bo));
        }
        public void GetChildNodeLoop(ref List<GUITreeComponent> Components, GUITreeComponent root)
        {

            GUITreeComponent[] childs = root.GetChildren();

            if (childs != null && childs.Length > 0)
            {
                Components.Add(childs[0]);

                GetChildNodeLoop(ref Components, childs[0]);
            }
        }


        //根据路径跳转
        private GUITreeComponent _goto(string treePath, List<GUITreeComponent> nodes, bool sendMsg)
        {

            m_messageDic.Clear();

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    m_messageDic.Add(new KeyValuePair<GUITreeComponent, bool>(nodes[i], false));
                }
            }

            string[] newPaths = treePath.Split('/');

            GUITreeComponent[] newComponents = _getTreeNodes(newPaths);

            if (newComponents == null)
            {
                Log.Error("没有找到节点路径:" + treePath);
                return null;
            }

            CurrentComponent = newComponents[newComponents.Length - 1];
            CurrentPath = treePath;


            for (int i = 0; i < newComponents.Length; i++)
            {
                m_nodeCache.Push(newComponents[i]);
                newComponents[i].CurrentParent = i == 0 ? null : newComponents[i - 1];
                SetComponentActive(newComponents[i], true);

            }

            if (!CurrentComponent.IsActive)
            {
                SetComponentActive(CurrentComponent, true);
            }

            if (sendMsg)
                DispatchEvent(FrameWork.GameEvent.GameTreeNodeChange, m_messageDic);

            return CurrentComponent;
        }

    }
}
