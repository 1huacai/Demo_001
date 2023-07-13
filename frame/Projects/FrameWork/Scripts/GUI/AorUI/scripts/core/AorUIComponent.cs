
using UnityEngine;
using UnityEngine.EventSystems;
using FrameWork.App;

namespace FrameWork.GUI.AorUI.Core
{
    /// <summary>
    /// AorUI组件基类
    /// AorUIComponent负责组织显示逻辑,提供统一的Dirty标记和刷新逻辑
    /// </summary>
    public class AorUIComponent : UIBehaviour, IMonoSwitch
    {

        /// <summary>
        /// 更新标记
        /// </summary>
        protected bool _isDirty;

        /// <summary>
        /// initialization的完毕标记.
        /// </summary>
        protected bool _isStarted;

        /// <summary>
        /// DrawUI方法的完成标记.
        /// 
        /// </summary>
        protected bool _isDrawUIDone;

        /// <summary>
        /// **** 绘制/更新UI显示的核心方法
        /// </summary>
        protected virtual void DrawUI()
        {

        }

        //------------------ IMonoSwitch 实现 ---------------
        #region IMonoSwitch 实现


        public virtual string ExportData()
        {
            return "";
        }
        public virtual void ImportData(string stringData)
        {

        }
        public void SetOtherParma(string target, string stringData)
        {
            if (target == GetType().ToString())
            {
                ImportData(stringData);
            }
        }

        public void RemoveCall(string className)
        {
            if (className == GetType().ToString())
            {
                OnRemoved();
            }
        }


        public virtual void OnAwake()
        {

        }



        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected virtual void Initialization()
        {
            _isStarted = true;
        }
        protected virtual void OnUpdate()
        {

        }
        protected virtual void OnRemoved()
        {

        }
        protected override void OnDestroy()
        {

        }

        protected virtual void OnEditorAwake()
        {

        }

        protected virtual void OnEditorStart()
        {

        }

        protected virtual void OnEditorUpdate()
        {


        }

        protected override void Awake()
        {
            base.Awake();
            MonoSwitch.PublicStaticProcess(this);
        }

        protected override void Start()
        {
            base.Start();
            Initialization();

        }

        private void Update()
        {
            if (_isDirty)
            {
                _isDrawUIDone = false; //标记开始绘制UI
                DrawUI();
                _isDrawUIDone = true;
                _isDirty = false;
            }
            if (!Application.isPlaying)
            {
                OnEditorUpdate();

            }

            OnUpdate();

//            else if (YKApplication.IsInited)
//            {
//                OnUpdate();
//            }
        }


        void IMonoSwitch.OnEditorAwake()
        {
            //  throw new NotImplementedException();
        }



        public void Finish()
        {
            MonoSwitch.Finish(this);
            //   throw new NotImplementedException();
        }

        #endregion
        //------------------ IMonoSwitch 实现 --------------- End

    }
}
