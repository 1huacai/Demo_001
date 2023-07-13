using CoreFrameWork;
using FrameWork.Audio;
using ResourceLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrameWork.Manager
{
    public class SingletonManager : IDisposable
    {
        //private static SingletonManager s_Inst;
        //public static SingletonManager Inst { get { if (s_Inst == null) s_Inst = new SingletonManager();return s_Inst; } }

        private static Dictionary<Type, object> s_ManagersDic = new Dictionary<Type, object>();
        #region C#管理器
        public static T GetManager<T>() where T : Component
        {
            object _value = null;
            if (s_ManagersDic.TryGetValue(typeof(T), out _value))
            {
                return _value as T;
            }

            return null;

        }

        public static void AddManager(object obj)
        {
            Type _key = obj.GetType();
            if (s_ManagersDic.ContainsKey(_key))
            {
                s_ManagersDic.Remove(_key);
            }
            s_ManagersDic.Add(_key, obj);
        }
        #endregion
        public static void Clear()
        {
            foreach (var item in s_ManagersDic)
            {
                var _temp = item.Value as IDisposable;
                if (_temp != null)
                    _temp.Dispose();
            }
        }
        public void Dispose()
        {
        }
    }
}
