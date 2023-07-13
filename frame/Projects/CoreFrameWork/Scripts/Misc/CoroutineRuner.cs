using System;
using UnityEngine;

namespace CoreFrameWork.Misc
{
    public class CoroutineRuner : MonoBehaviour
    {

        private bool _isStarted;

        void Start() {
            if (initDoFunc != null)
            {
                initDoFunc();
            }
        }

        public Action initDoFunc;
        
        void OnDestroy() {
            initDoFunc = null;
        }

    }
}
