using System.Collections;
using System.Collections.Generic;

namespace XCore.Collections
{
    public class SynchronizedQueue<T>
    {
        private readonly Queue<T> mQueue;
        private readonly object mRoot;

        public int Count
        {
            get
            {
                lock (mRoot)
                {
                    return mQueue.Count;
                }
            }
        }

        public SynchronizedQueue(int size)
        {
            mQueue = new Queue<T>(size);
            mRoot = new object();
        }

        public SynchronizedQueue()
        {
            mQueue = new Queue<T>();
            mRoot = new object();
        }

        public void Clear()
        {
            lock (mRoot)
            {
                mQueue.Clear();
            }
        }

        public bool Contains(T obj)
        {
            lock (mRoot)
            {
                return mQueue.Contains(obj);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (mRoot)
            {
                mQueue.CopyTo(array, arrayIndex);
            }
        }

        public void Enqueue(T value)
        {
            lock (mRoot)
            {
                mQueue.Enqueue(value);
            }
        }

        public T Dequeue()
        {
            lock (mRoot)
            {
                return mQueue.Dequeue();
            }
        }

        public IEnumerator GetEnumerator()
        {
            lock (mRoot)
            {
                return mQueue.GetEnumerator();
            }
        }

        public T Peek()
        {
            lock (mRoot)
            {
                return mQueue.Peek();
            }
        }

        public T[] ToArray()
        {
            lock (mRoot)
            {
                return mQueue.ToArray();
            }
        }

        public void TrimExcess()
        {
            lock (mRoot)
            {
                mQueue.TrimExcess();
            }
        }
    }
}