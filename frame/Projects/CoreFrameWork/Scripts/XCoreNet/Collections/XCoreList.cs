using System.Collections.Generic;

namespace XCore.Collections
{
    /// <summary>
    /// 类说明：列表集合
    /// </summary>
    public class XCoreList<T>
    {
        /// <summary>
        /// 数组
        /// </summary>
        private T[] mBuffer;
        /// <summary>
        /// 大小
        /// </summary>
        private int mSize;

        /// <summary>
        /// 下标
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                return mBuffer[i];
            }
            set
            {
                mBuffer[i] = value;
            }
        }

        /// <summary>
        /// 大小
        /// </summary>
        public int Size
        {
            get
            {
                return mSize;
            }
            set
            {
                mSize = value;
            }
        }
        /// <summary>
        /// 比较委托
        /// </summary>
        /// <param name="left">左边</param>
        /// <param name="right">右边</param>
        /// <returns>大于0替换</returns>
        public delegate int CompareFunc(T left, T right);

        /// <summary>s
        /// 获取计数器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (mBuffer != null)
            {
                for (int i = 0; i < mSize; ++i)
                {
                    yield return mBuffer[i];
                }
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            mSize = 0;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            mSize = 0;
            mBuffer = null;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (mBuffer == null || mSize == mBuffer.Length)
            {
                AllocateMore();
            }
            mBuffer[mSize++] = item;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            if (mBuffer == null || mSize == mBuffer.Length)
            {
                AllocateMore();
            }
            if (index > -1 && index < mSize)
            {
                for (int i = mSize; i > index; --i)
                {
                    mBuffer[i] = mBuffer[i - 1];
                }
                mBuffer[index] = item;
                ++mSize;
            }
            else Add(item);
        }

        /// <summary>
        /// 包含
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            if (mBuffer == null)
            {
                return false;
            }
            for (int i = 0; i < mSize; ++i)
            {
                if (mBuffer[i] != null && mBuffer[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 查找下标
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            if (mBuffer == null)
            {
                return -1;
            }
            for (int i = 0; i < mSize; ++i)
            {
                if (mBuffer[i] != null && mBuffer[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            if (mBuffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;

                for (int i = 0; i < mSize; ++i)
                {
                    if (comp.Equals(mBuffer[i], item))
                    {
                        --mSize;
                        mBuffer[i] = default(T);
                        for (int b = i; b < mSize; ++b) mBuffer[b] = mBuffer[b + 1];
                        mBuffer[mSize] = default(T);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 按位移除
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            if (mBuffer != null && index > -1 && index < mSize)
            {
                --mSize;
                mBuffer[index] = default(T);
                for (int b = index; b < mSize; ++b)
                {
                    mBuffer[b] = mBuffer[b + 1];
                }
                mBuffer[mSize] = default(T);
            }
        }

        /// <summary>
        /// 弹出第一个
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (mBuffer != null && mSize != 0)
            {
                T val = mBuffer[--mSize];
                mBuffer[mSize] = default(T);
                return val;
            }
            return default(T);
        }

        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            Trim();
            return mBuffer;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(CompareFunc comparer)
        {
            int start = 0;
            int max = mSize - 1;
            bool changed = true;

            while (changed)
            {
                changed = false;

                for (int i = start; i < max; ++i)
                {
                    if (comparer(mBuffer[i], mBuffer[i + 1]) > 0)
                    {
                        T temp = mBuffer[i];
                        mBuffer[i] = mBuffer[i + 1];
                        mBuffer[i + 1] = temp;
                        changed = true;
                    }
                    else if (!changed)
                    {
                        start = i == 0 ? 0 : i - 1;
                    }
                }
            }
        }

        /// <summary>
        /// 分配新的内存
        /// </summary>
        private void AllocateMore()
        {
            T[] newList = mBuffer != null ? new T[System.Math.Max(mBuffer.Length << 1, 32)] : new T[32];
            if (mBuffer != null && mSize > 0) mBuffer.CopyTo(newList, 0);
            mBuffer = newList;
        }

        /// <summary>
        /// 去除空数据
        /// </summary>
        private void Trim()
        {
            if (mSize > 0)
            {
                if (mSize < mBuffer.Length)
                {
                    T[] newList = new T[mSize];
                    for (int i = 0; i < mSize; ++i) newList[i] = mBuffer[i];
                    mBuffer = newList;
                }
            }
            else mBuffer = null;
        }
    }
}
