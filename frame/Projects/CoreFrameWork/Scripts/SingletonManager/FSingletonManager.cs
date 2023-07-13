//------------------------------------------------------------------------
// |                                                                   |
// | Autor:Adam                                                           |
// |                                       |
// |                                                                   |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public class FSingletonManager
    {
        private static FSingletonManager _inst = null;
        public static FSingletonManager Inst
        {
            get
            {
                if (null == _inst)
                {
                    _inst = new FSingletonManager();
                }
                return _inst;
            }
        }

        private Dictionary<string, object> m_dic = new Dictionary<string, object>();
        private Dictionary<string, Singleton<System.Object>> _dic;



        public void AddSingle(object obj)
        {
            if (!m_dic.ContainsKey(obj.GetType().FullName))
            {
                m_dic.Add(obj.GetType().FullName, obj);
            }
        }


        /// <summary>
        /// 删除单例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveSingle(object obj)
        {
            string _key = obj.GetType().FullName;
           
   
            
 
                m_dic.Remove(_key);
            

        }

 
        /// <summary>
        /// 释放所有单例
        /// </summary>
        public void Dispose()
        {
            _inst = null;
            GC.Collect();
        }
 

    }

}