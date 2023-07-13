using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FrameWork;
using FrameWork.App;
using YoukiaGame;
using FrameWork.GUI.AorUI.Animtion;
using CoreFrameWork;

namespace FrameWork.GUI.AorUI.Core
{
    public class AorSwitchableUI : UprefabUnitBase
    {
        [Serializable]
        public class AorSwitchableUIEvent : UnityEvent<GameObject> { }

        /// <summary>
        /// 是否是静态页面
        /// 勾选此项,在切换页面时此组件不会被Destroy,只是setActive(false);
        /// </summary>
        public bool isStatic = false;
        /// <summary>
        /// 自动调用Open方法
        /// </summary>
        public bool AutoOpenOnEnable;
        public object[] StartArgs;
        private AorUIManager _UIManager;
        public CallBack CloseCallBack = null;
        public int StateIndex = 0;
        /// <summary>
        /// *** Path也作为Xid使用
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private string _path;
        public string Path {
            get { return _path; }
            set { _path = value; }
        }
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public bool isStarted {
            get {
                return _isStarted;
            }
        }
        protected bool _isStarted = false;

        /// <summary>
        /// 初始化完成时调用的委托,仅在Page初始化时会被调用,之后修改无用.
        /// </summary>
        public CallBack onEventInit;

        private bool _isOpend;
        public bool isOpend {
            get { return _isOpend; }
        }

        [SerializeField]
        private AorSwitchableUIEvent m_onOpen = new AorSwitchableUIEvent();
        public AorSwitchableUIEvent onOpen {
            get { return m_onOpen; }
            set { m_onOpen = value; }
        }

        [SerializeField]
        private AorSwitchableUIEvent m_onClose = new AorSwitchableUIEvent();
        public AorSwitchableUIEvent onClose {
            get { return m_onClose; }
            set { m_onClose = value; }
        }

        private void findUIManager()
        {
            if (_UIManager == null)
            {
                GameObject mmg = GameObject.Find(AorUIManager.PrefabName);
                if (mmg != null)
                {
                    _UIManager = mmg.GetComponent<AorUIManager>();
                }
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();

            if (m_onOpen == null)
            {
                m_onOpen = new AorSwitchableUIEvent();
            }

            if (m_onClose == null)
            {
                m_onClose = new AorSwitchableUIEvent();
            }

        }

        protected override void OnEnable()
        {
            if (_isStarted && !_isOpend && AutoOpenOnEnable)
            {
                Open(null);
            }
        }

        protected override void Initialization()
        {
            //base.Initialization(); ** AorSwitchableUI不会执行UprefabUnitBase.Initialization,响应实现移至Open方法内;

            findUIManager();

            if (_UIManager == null)
            {
                Log.Error("Can not find AorUISystem Main in this scene. init fail .");
                return;
            }

            //UI管理器未启动就等待
            if (_UIManager.isInit)
            {
                initFinish();
            }
            else
            {
                StartCoroutine(waitInit());
            }
        }

        /// <summary>
        /// 初始化完成后续行为
        /// </summary>
        protected virtual void initFinish()
        {
            _UIManager.addPageINPool(this);
            _isStarted = true;
            if (onEventInit != null)
            {
                onEventInit();
            }

            if (_UIManager != null)
            {
                _UIManager.onAorSwitchableUIInited.Invoke(this);
            }

            if (AutoOpenOnEnable)
            {
                Open(null);
            }
        }

        IEnumerator waitInit()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (_UIManager.isInit)
                {
                    initFinish();
                    break;
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _UIManager = null;

        }

        public virtual void Open(CallBack openedDoFunc)
        {

            if (_isStarted && !_isOpend)
            {
                _canvasGroup.alpha = _orginCGAlpha;
                if (openedDoFunc != null)
                {
                    openedDoFunc();
                }
                if (m_onOpen != null)
                {
                    m_onOpen.Invoke(gameObject);
                }
                if (_UIManager != null)
                {
                    _UIManager.onAorSwitchableUIOpen.Invoke(this);
                }
                _isOpend = true;
            }
        }


        public virtual void Close(CallBack closedDoFunc)
        {

            if (_isStarted && _isOpend)
            {
                _canvasGroup.alpha = 0;
                if (CloseCallBack != null)
                {
                    CloseCallBack();
                }
                _UIManager.CloseUI(this);
                if (closedDoFunc != null)
                {
                    closedDoFunc();
                }
                if (m_onClose != null)
                {
                    m_onClose.Invoke(gameObject);
                }
                if (_UIManager != null)
                {
                    _UIManager.onAorSwitchableUIClose.Invoke(this);
                }
                _isOpend = false;
            }
        }

    }
}
