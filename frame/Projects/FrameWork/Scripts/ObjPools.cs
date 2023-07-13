//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameWork
{
    public class ObjPools<T>
    {
        public Dictionary<T, PoolsItem<T>> ItemsDic;
        public int Max = -1;
        public bool ProfundityDispose = false;


        public ObjPools(int max = -1)
        {
            ItemsDic = new Dictionary<T, PoolsItem<T>>();
            this.Max = max;
        }


        private PoolsItem<T> AddToPool<T1>() where T1 : T
        {
            return AddToPool(typeof(T1));
        }

        public PoolsItem<T> AddToPool(Type t)
        {
            PoolsItem<T> item = new PoolsItem<T>(t);
            ItemsDic.Add(item.Obj, item);
            return item;
        }


        /// <summary>
        /// 
        /// </summary>
        public T Create()
        {
            return Create<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        public T1 Create<T1>() where T1 : T
        {
            return (T1)Create(typeof(T1));
        }

        public T Create(Type t)
        {
            foreach (KeyValuePair<T, PoolsItem<T>> item in ItemsDic)
            {
                if (!item.Value.IsUse && item.Value.ObjType == t)
                {
                    item.Value.IsUse = true;
                    return item.Key;
                }
            }
            if (Max == -1 || ItemsDic.Count < Max)
                return AddToPool(t).Obj;
            throw new Exception("Can't Create New Memory, Obj Max Count : " + Max);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose(T obj)
        {
            if (obj == null || !ItemsDic.ContainsKey(obj))
                return;
            ItemsDic[obj].IsUse = false;
            // 深度清理对象
            if (ProfundityDispose)
            {
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Default | BindingFlags.NonPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i].SetValue(obj, default(T));
                }
            }
        }

        public void Dispose()
        {
            ItemsDic.Clear();
            ItemsDic = null;
        }

        public class PoolsItem<T1>
        {
            public T1 Obj;
            public bool IsUse;
            public Type ObjType;

            public PoolsItem(Type t)
            {
                IsUse = true;
                ObjType = t;
                Obj = (T1)Activator.CreateInstance(t);
            }
        }


    }

}