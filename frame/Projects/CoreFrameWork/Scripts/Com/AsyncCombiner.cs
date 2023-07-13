using System;
using System.Collections.Generic;

namespace CoreFrameWork.Com
{
    public class AsyncCombiner
    {
        public class AsyncHandle
        {
            private readonly AsyncCombiner _combiner;
            private readonly int _handle;

            protected internal AsyncHandle(AsyncCombiner combiner, int handle)
            {
                _combiner = combiner;
                _handle = handle;
            }

            public AsyncCombiner Combiner
            {
                get { return _combiner; }
            }

            public void Finish()
            {
                _combiner._asyncHandles[_handle] = true;
                _combiner.RefreshAsyncHandles();
            }
        }

        private List<CallBackHandle> _completionCallBacks = new List<CallBackHandle>();
        private List<bool> _asyncHandles = new List<bool>();


        public void AddCompletionCall(CallBackHandle call)
        {
            _completionCallBacks.Add(call);
        }

        public AsyncHandle CreateAsyncHandle()
        {
            _asyncHandles.Add(false);
            return new AsyncHandle(this, _asyncHandles.Count - 1);
        }

        public void RefreshAsyncHandles()
        {
            bool finish = true;
            for (int i = 0; i < _asyncHandles.Count; i++)
            {
                if (!_asyncHandles[i])
                {
                    finish = false;
                    break;
                }
            }

            if (finish)
            {
                for (int i = 0; i < _completionCallBacks.Count; i++)
                {
                    _completionCallBacks[i]();

                }

                _completionCallBacks.Clear();
            }
        }
    }
}