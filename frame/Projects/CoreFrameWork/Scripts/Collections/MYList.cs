using System;
using System.Collections.Generic;

namespace CoreFrameWork.Collections
{
    public class MYList<T>
    {
        /// <summary>
        /// 比较委托
        /// </summary>
        /// <param name="left">左边</param>
        /// <param name="right">右边</param>
        /// <returns>大于0替换</returns>
        public delegate int CompareFunc(T left, T right);

        /// <summary>
        /// 数组
        /// </summary>
        private T[] _buffer;
        /// <summary>
        /// 大小
        /// </summary>
        private int _size = 0;
        /// <summary>
        /// 下标
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                return _buffer[i];
            }
            set
            {
                _buffer[i] = value;
            }
        }

        /// <summary>
        /// 大小
        /// </summary>
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }
        /// <summary>s
        /// 获取计数器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_buffer != null)
            {
                for (int i = 0; i < _size; ++i)
                {
                    yield return _buffer[i];
                }
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            _size = 0;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            _size = 0;
            _buffer = null;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (_buffer == null || _size == _buffer.Length)
            {
                AllocateMore();
            }
            _buffer[_size++] = item;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            if (_buffer == null || _size == _buffer.Length)
            {
                AllocateMore();
            }
            if (index > -1 && index < _size)
            {
                for (int i = _size; i > index; --i)
                {
                    _buffer[i] = _buffer[i - 1];
                }
                _buffer[index] = item;
                ++_size;
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
            if (_buffer == null)
            {
                return false;
            }
            for (int i = 0; i < _size; ++i)
            {
                if (_buffer[i] != null && _buffer[i].Equals(item))
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
            if (_buffer == null)
            {
                return -1;
            }
            for (int i = 0; i < _size; ++i)
            {
                if (_buffer[i] != null && _buffer[i].Equals(item))
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
            if (_buffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;

                for (int i = 0; i < _size; ++i)
                {
                    if (comp.Equals(_buffer[i], item))
                    {
                        --_size;
                        _buffer[i] = default(T);
                        for (int b = i; b < _size; ++b) _buffer[b] = _buffer[b + 1];
                        _buffer[_size] = default(T);
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
            if (_buffer != null && index > -1 && index < _size)
            {
                --_size;
                _buffer[index] = default(T);
                for (int b = index; b < _size; ++b)
                {
                    _buffer[b] = _buffer[b + 1];
                }
                _buffer[_size] = default(T);
            }
        }

        /// <summary>
        /// 弹出第一个
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (_buffer != null && _size != 0)
            {
                T val = _buffer[--_size];
                _buffer[_size] = default(T);
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
            return _buffer;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(CompareFunc comparer)
        {
            int start = 0;
            int max = _size - 1;
            bool changed = true;

            while (changed)
            {
                changed = false;

                for (int i = start; i < max; ++i)
                {
                    if (comparer(_buffer[i], _buffer[i + 1]) > 0)
                    {
                        T temp = _buffer[i];
                        _buffer[i] = _buffer[i + 1];
                        _buffer[i + 1] = temp;
                        changed = true;
                    }
                    else if (!changed)
                    {
                        start = (i == 0) ? 0 : i - 1;
                    }
                }
            }
        }
        /// <summary>
        /// 分配新的内存
        /// </summary>
        private void AllocateMore()
        {
            T[] newList = (_buffer != null) ? new T[Math.Max(_buffer.Length << 1, 32)] : new T[32];
            if (_buffer != null && _size > 0) _buffer.CopyTo(newList, 0);
            _buffer = newList;
        }

        /// <summary>
        /// 去除空数据
        /// </summary>
        private void Trim()
        {
            if (_size > 0)
            {
                if (_size < _buffer.Length)
                {
                    T[] newList = new T[_size];
                    for (int i = 0; i < _size; ++i) newList[i] = _buffer[i];
                    _buffer = newList;
                }
            }
            else _buffer = null;
        }
    }
}
