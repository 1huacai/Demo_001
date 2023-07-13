using System;

namespace ResourceFrameWork
{
    /// <summary>
    /// 资源信息 主加载资源才有，加载依赖资源不用
    /// </summary>
    public class InstanceKeyInfo
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string Path;
       /// <summary>
       /// 资源类型
       /// </summary>
        public Type Type;
        /// <summary>
        /// 资源名字
        /// </summary>
        public string Name;
        public InstanceKeyInfo()
        {
        }
        public InstanceKeyInfo(string path, string name, Type type)
        {
            this.Path = path;
            this.Type = type;
            this.Name = name;
        }
        public void Init(string path, string name, Type type)
        {
            this.Path = path;
            this.Type = type;
            this.Name = name;
        }

    }

    public class CallBackInfo
    {
        public CallBack<object> callBack;
        public CallBackInfo( CallBack<object> callBack)
        {
            this.callBack = callBack;
        }
    }
}
