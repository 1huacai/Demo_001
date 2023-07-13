using System;
using System.Collections.Generic;

namespace FrameWork
{
    /// <summary>
    /// GUI树节点,每一个节点对应一个物理UI页面或窗体
    /// </summary>
    public class GUITreeComponent
    {
        protected readonly string _name;
        private readonly List<GUITreeComponent> _children = new List<GUITreeComponent>();

        protected GUIManager Manager { get; private set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        public bool IsActive { get; private set; }

        /// <summary>
        /// 当前目标激活节点的父节点,注意set属性禁止由Manager以外修改
        /// </summary>
        public GUITreeComponent CurrentParent { get; set; }

        /// <summary>
        /// 创建一个逻辑UI节点
        /// </summary>
        /// <param name="game">Game实例</param>
        /// <param name="name">节点名称</param>
        public GUITreeComponent(GUIManager manager, string name)
        {
            Manager = manager;
            _name = name;
        }



        /// <summary>
        /// 将指定的节点附加至该节点下
        /// 同一节点可以被附加至多个父节点,同一父节点下节点不能重名
        /// </summary>
        /// <param name="childs">附加的子节点</param>
        public void AttachChild(params GUITreeComponent[] childs)
        {
            for (int i = 0; i < childs.Length; i++)
            {
                DetachChild(childs[i]);
                _children.Add(childs[i]);
            }
        }

        /// <summary>
        /// 移除指定的子节点
        /// </summary>
        /// <param name="child">子节点</param>
        public void DetachChild(GUITreeComponent child)
        {
            _children.RemoveAll(node => child == node);
        }
 

        /// <summary>
        /// 移除所有的子节点
        /// </summary>
        public void DetachChildren()
        {
            _children.Clear();
        }

        /// <summary>
        /// 启用/禁用节点,一般由Manager自动调用
        /// </summary>
        /// <param name="active">是否启用</param>
        public void SetActive(bool active)
        {
            if (active == IsActive)
            {
                return;
            }
            if (active)
            {
                
                OnNodeActive();
            //    Game.DispatchEvent(GameEvent.GameTreeNodeActive, Game, this);
            }
            else
            {
                OnNodeRelease();
              //  Game.DispatchEvent(GameEvent.GameTreeNodeRelease, Game, this);
            }
            this.IsActive = active;
        }

        /// <summary>
        /// 获取指定名称的子节点
        /// </summary>
        /// <param name="name">子节点名称</param>
        /// <returns>对应的子节点,如果没有名称匹配的节点,则返回null</returns>
        public GUITreeComponent GetChild(string name)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Name == name)
                {
                    return _children[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 返回所有子节点
        /// </summary>
        /// <returns>子节点列表</returns>
        public GUITreeComponent[] GetChildren()
        {
            return _children.ToArray();
        }

        /// <summary>
        /// 当节点被管理器卸载时调用
        /// </summary>
        public virtual void OnNodeRelease()
        {
            Console.WriteLine(this + " Release!");
        }

        /// <summary>
        /// 当节点被管理器激活时调用
        /// </summary>
        public virtual void OnNodeActive()
        {
            Console.WriteLine(this + " Active!");
        }

        /// <summary>
        /// 重写返回节点名称
        /// </summary>
        /// <returns>节点名称</returns>
        public override string ToString()
        {
            return _name;
        }
    }
}
