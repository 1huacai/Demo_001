using System;
using UnityEngine;

namespace CoreFrameWork
{
    /// <summary>
    /// 单利管理器基类
    /// </summary>
    /// <typeparam name="T">管理器类型</typeparam>
    public class SingletonManager<T> : MonoBehaviour,IDisposable
         where T : Component
    {
        /// <summary>
        /// 获得当前类的实例
        /// </summary>
        public static T CreateInstance()
        {
            GameObject obj = new GameObject();
            obj.name = typeof(T).Name;
            DontDestroyOnLoad(obj);
            return (T)obj.AddComponent(typeof(T));
        }

        public virtual void Dispose()
        {
        }
    }
}


