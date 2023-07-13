//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreFrameWork.Com
{
    public static class Runtime
    {
        public static List<Assembly> AssemblyList = new List<Assembly>();
        private static object mLoader = new object();


        public static T Instance<T>(  bool ignoreCase = false)
        {
            Type type = GetType(typeof(T).Name, ignoreCase);
            if (type == null)
                return default(T);
            return (T)Activator.CreateInstance(type);
        }

        public static T Instance<T>(string name, bool ignoreCase = false)
        {
            Type type = GetType(name, ignoreCase);
            if (type == null)
                return default(T);
            return (T)Activator.CreateInstance(type);
        }


        public static Type GetType(string name, bool ignoreCase = false)
        {
            for (int i = 0; i < AssemblyList.Count; i++)
            {
                Type type = AssemblyList[i].GetType(name, false, ignoreCase);
                if (type != null)
                    return type;
            }
            return null;
        }


        public static void Add(Assembly asm)
        {
            lock (mLoader)
            {
                AssemblyList.Add(asm);
            }
        }

        public static void Remove(Assembly asm)
        {
            lock (mLoader)
            {
                AssemblyList.Remove(asm);
            }
        }



    }
}