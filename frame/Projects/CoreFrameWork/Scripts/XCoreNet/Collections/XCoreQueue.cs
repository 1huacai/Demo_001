using System;

namespace XCore.Collections
{
    public class XCoreQueue<T>
    {
        private IXCoreQueueNode<T> mHead;
        private IXCoreQueueNode<T> mEnd;
        private int mCount;

        public int Count
        {
            get { return mCount; }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            if (mHead != null)
            {
                mHead.ClearNext();
            }
            mHead = null;
            mEnd = null;
            mCount = 0;
        }

        /// <summary>
        /// 移除并返回开始处对象
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (mHead == null)
            {
                throw new IndexOutOfRangeException("队列为空");
            }
            T temp = mHead.Data();
            IXCoreQueueNode<T> next = mHead.Next();
            if (next != null)
            {
                mHead = next;
            }
            else
            {
                mHead = null;
                mEnd = null;
            }
            mCount--;
            return temp;
        }

        /// <summary>
        /// 将对象添加到结尾处
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            XCoreQueueNode<T> temp = new XCoreQueueNode<T>(item);
            if (mEnd != null)
            {
                mEnd.SetNext(temp);
                mEnd = temp;
            }
            else
            {
                mHead = temp;
                mEnd = temp;
            }
            mCount++;
        }
        /// <summary>
        /// 返回开始处对象但不移除
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (mHead == null)
            {
                throw new IndexOutOfRangeException("队列为空");
            }
            return mHead.Data();
        }

        /// <summary>
        /// 移除并返回开始处对象
        /// </summary>
        /// <returns></returns>
        public IXCoreQueueNode<T> DequeueNode()
        {
            if (mHead == null)
            {
                throw new IndexOutOfRangeException("队列为空");
            }
            IXCoreQueueNode<T> temp = mHead;
            IXCoreQueueNode<T> next = mHead.Next();
            if (next != null)
            {
                mHead = next;
            }
            else
            {
                mHead = null;
                mEnd = null;
            }
            mCount--;
            return temp;
        }

        /// <summary>
        /// 将对象添加到结尾处
        /// </summary>
        /// <param name="item"></param>
        public void EnqueueNode(IXCoreQueueNode<T> item)
        {
            if (item == null)
            {
                throw new IndexOutOfRangeException("XCoreQueueNode<T>为空");
            }
            item.SetNext(null);
            if (mEnd != null)
            {
                mEnd.SetNext(item);
                mEnd = item;
            }
            else
            {
                mHead = item;
                mEnd = item;
            }
            mCount++;
        }

    }

    public class XCoreQueueNode<T> : IXCoreQueueNode<T>
    {
        private readonly T mData;
        private IXCoreQueueNode<T> mNext;

        public XCoreQueueNode(T d)
        {
            mData = d;
            mNext = null;
        }

        public void ClearNext()
        {
            if (mNext != null)
            {
                mNext.ClearNext();
            }
            mNext = null;
        }

        public void SetNext(IXCoreQueueNode<T> t)
        {
            mNext = t;
        }

        public IXCoreQueueNode<T> Next()
        {
            return mNext;
        }

        public T Data()
        {
            return mData;
        }
    }

    public interface IXCoreQueueNode<T>
    {
        void ClearNext();
        IXCoreQueueNode<T> Next();
        void SetNext(IXCoreQueueNode<T> t);
        T Data();
    }
}
