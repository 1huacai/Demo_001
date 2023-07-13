using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using FrameWork.App;
using CoreFrameWork;

namespace FrameWork.GUI.AorUI.events
{
    /// <summary>
    /// AorUI事件监听器(所有需要事件响应的UI对象都应该挂载此组件)
    /// </summary>
    public class AorUIEventListenerForMapArmy : MonoSwitch,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        [Serializable]
        public class AorUIEventListenerEvent : UnityEvent<GameObject, PointerEventData> { }

        /*
        public static AorUIEventListener getParentEventListener(Transform target) {
            AorUIEventListener[] g = target.GetComponentsInParent<AorUIEventListener>();
            int i, length = g.Length;
            for (i = 0; i < length; i++) {
                if (!g[i].gameObject.Equals(target.gameObject)) {
                    //Debug.UiLog("************* " + g[i].gameObject.name);
                    AorUIEventListener o = g[i];
                    g = null;
                    return o;
                }
            }
            return null;
        }
        */
        /// <summary>
        /// 获取该AorUIEventListener对象的上一级AorUIEventListener对象(静态)
        /// </summary>
        /// <param name="target">Transform</param>
        /// <returns>AorUIEventListener</returns>
        public static AorUIEventListenerForMapArmy getParentEventListener(Transform target)
        {
            if (target.parent != null)
            {
                return target.parent.GetComponentInParent<AorUIEventListenerForMapArmy>();
            }
            return null;
        }
        /// <summary>
        /// 是否监听 Click 事件
        /// 
        /// </summary>
        public bool Click = true;
        /// <summary>
        /// 是否监听 LongPress 事件 (长按)
        /// </summary>
        public bool LongPress = false;
        /// <summary>
        /// 是否监听 Down 事件 (按下)
        /// </summary>
        public bool Down = false;
        /// <summary>
        /// 是否监听 Up 事件 (抬起)
        /// </summary>
        public bool Up = false;
        /// <summary>
        /// 是否监听 Over 事件 (移入,通常此事件用于鼠标)
        /// </summary>
        public bool Over = false;
        /// <summary>
        /// 是否监听 Out 事件 (移出,通常此事件用于鼠标)
        /// </summary>
        public bool Out = false;
        /// <summary>
        /// 是否监听 Drag 事件 (拖动)
        /// </summary>
        public bool Drag = false;
        /// /// <summary>
        /// 是否监听 Swing 事件 (滑动/快速甩动)
        /// </summary>
        public bool Swing = false;

        /// <summary>
        /// Click事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventClick = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventClick
        {
            get
            {
                return m_onEventClick;
            }
            set
            {
                m_onEventClick = value;
            }
        }

        /// <summary>
        /// LongPress事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventLongPress = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventLongPress
        {
            get
            {
                return m_onEventLongPress;
            }
            set
            {
                m_onEventLongPress = value;
            }
        }

        /// <summary>
        /// Down事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventDown = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventDown
        {
            get { return m_onEventDown; }
            set { m_onEventDown = value; }
        }

        /// <summary>
        /// Up事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventUp = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventUp
        {
            get
            {
                return m_onEventUp;
            }
            set
            {
                m_onEventUp = value;
            }
        }

        /// <summary>
        /// Over事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventOver = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventOver
        {
            get
            {
                return m_onEventOver;
            }
            set
            {
                m_onEventOver = value;
            }
        }
        /// <summary>
        /// Out事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventOut = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventOut
        {
            get
            {
                return m_onEventOut;
            }
            set
            {
                m_onEventOut = value;
            }
        }

        /// <summary>
        /// Drag事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventDrag = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventDrag
        {
            get
            {
                return m_onEventDrag;
            }
            set
            {
                m_onEventDrag = value;
            }
        }
        [SerializeField]
        public AorUIEventListenerEvent onEventBeginDrag
        {
            get; set;
        }
        [SerializeField]
        public AorUIEventListenerEvent onEventEndDrag
        {
            get; set;
        }
        /// <summary>
        /// Swing事件 委托
        /// AorUI事件监听器可针对某个监听对象挂载事件委托方法来处理特殊的事件响应.
        /// 挂载委托后,AorUI事件监听器仍会通知全局事件逻辑IAorUILogicInterface,请妥善处理两者的逻辑关系.
        /// </summary>
        [SerializeField]
        private AorUIEventListenerEvent m_onEventSwing = new AorUIEventListenerEvent();
        public AorUIEventListenerEvent onEventSwing
        {
            get
            {
                return m_onEventSwing;
            }
            set
            {
                m_onEventSwing = value;
            }
        }


        private AorUIManager _UIManager;
        public AorUIManager UIManager
        {
            get
            {
                return _UIManager;
            }
        }

        private AorUIEventListenerForMapArmy _parentEventListener;
        /// <summary>
        /// 上一级AorUIEventListener对象的实例引用
        /// </summary>
        public AorUIEventListenerForMapArmy parentEventListener
        {
            get
            {
                if (_parentEventListener == null)
                    _parentEventListener = getParentEventListener(this.transform);
                return _parentEventListener;
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();

            if (m_onEventClick == null)
            {
                m_onEventClick = new AorUIEventListenerEvent();
            }

            if (m_onEventDown == null)
            {
                m_onEventDown = new AorUIEventListenerEvent();
            }

            if (m_onEventDrag == null)
            {
                m_onEventDrag = new AorUIEventListenerEvent();
            }
            if (onEventBeginDrag == null)
                onEventBeginDrag = new AorUIEventListenerEvent();
            if (onEventEndDrag == null)
                onEventEndDrag = new AorUIEventListenerEvent();

            if (m_onEventLongPress == null)
            {
                m_onEventLongPress = new AorUIEventListenerEvent();
            }

            if (m_onEventOut == null)
            {
                m_onEventOut = new AorUIEventListenerEvent();
            }

            if (m_onEventOver == null)
            {
                m_onEventOver = new AorUIEventListenerEvent();
            }

            if (m_onEventSwing == null)
            {
                m_onEventSwing = new AorUIEventListenerEvent();
            }

            if (m_onEventUp == null)
            {
                m_onEventUp = new AorUIEventListenerEvent();
            }
        }

        // Use this for initialization
        protected override void Initialization()
        {
            base.Initialization();

            GameObject mmg = GameObject.Find(AorUIManager.PrefabName);
            if (mmg == null)
            {
                Log.Error("Can not find AorUISystem Main in this scene. init fail .");
                return;
            }
            _UIManager = mmg.GetComponent<AorUIManager>();



        }

        void OnDestroy()
        {
            if (m_onEventSwing != null)
            {
                m_onEventSwing.RemoveAllListeners();
                m_onEventSwing = null;
            }

            if (m_onEventDrag != null)
            {
                m_onEventDrag.RemoveAllListeners();
                m_onEventDrag = null;
            }
            if (onEventBeginDrag != null)
            {
                onEventBeginDrag.RemoveAllListeners();
                onEventBeginDrag = null;
            }
            if (onEventEndDrag != null)
            {
                onEventEndDrag.RemoveAllListeners();
                onEventEndDrag = null;
            }
            if (m_onEventOver != null)
            {
                m_onEventOver.RemoveAllListeners();
                m_onEventOver = null;
            }

            if (m_onEventOut != null)
            {
                m_onEventOut.RemoveAllListeners();
                m_onEventOut = null;
            }

            if (m_onEventClick != null)
            {
                m_onEventClick.RemoveAllListeners();
                m_onEventClick = null;
            }

            if (m_onEventLongPress != null)
            {
                m_onEventLongPress.RemoveAllListeners();
                m_onEventLongPress = null;
            }

            if (m_onEventDown != null)
            {
                m_onEventDown.RemoveAllListeners();
                m_onEventDown = null;
            }

            if (m_onEventUp != null)
            {
                m_onEventUp.RemoveAllListeners();
                m_onEventUp = null;
            }

            _UIManager = null;
        }

        // Update is called once per frame
        //void Update()
        //{
        //    if (_isDown)
        //    {
        //        _downValue += (int)(Time.deltaTime * 1000);
        //        //Drag
        //        if ((Vector2.Distance(_currentEventData.position, _currentEventData.pressPosition) > AorUIEventConfig.PosThreshold) || _isDraged)
        //        {
        //            if (onEventDrag != null)
        //            {
        //                onEventDrag.Invoke(gameObject, _currentEventData);
        //            }
        //        }
        //    }
        //}
        /*
        void FixedUpdate() {
            
        }*/
        private bool _isDraged = false;
        private bool _isLongPressed = false;
        private bool _isDown = false;
        private int _downValue;
        private PointerEventData _currentEventData;
        /// <summary>
        /// 原生EventSystem.Down事件处理接口实现 (若非强行事件注入,程序员通常不需要调用此方法)
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        public void OnPointerDown(PointerEventData eventData)
        {

            _currentEventData = eventData;
            _downValue = 0;
            _isDraged = false;
            //_isProDrag = false;
            _isLongPressed = false;

            _isDown = true;
            if (Down)
            {
                if (onEventDown != null)
                {
                    onEventDown.Invoke(gameObject, eventData);
                }
                if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                {
                    //Down
                    _UIManager.AorUIInterfaceLogic.Receive_Down(this.gameObject, eventData);
                }
            }
            else
            {
                passEventDown(parentEventListener, eventData);
            }
        }
        /// <summary>
        /// 原生EventSystem.Up事件处理接口实现 (若非强行事件注入,程序员通常不需要调用此方法)
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            _isDown = false;
            //Up
            if (Up)
            {
                if (onEventUp != null)
                {
                    onEventUp.Invoke(gameObject, eventData);
                }
                if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                {
                    _UIManager.AorUIInterfaceLogic.Receive_Up(this.gameObject, eventData);
                }
            }
            else
            {
                passEventUp(parentEventListener, eventData);
            }

            //Swing
            if (Vector2.Distance(eventData.position, eventData.pressPosition) > AorUIEventConfig.SwingThreshold && _downValue < AorUIEventConfig.SwingTimeThreshold)
            {
                if (Swing)
                {
                    if (onEventSwing != null)
                    {
                        onEventSwing.Invoke(gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Swing(this.gameObject, eventData);
                    }
                }
                else
                {
                    passEventSwing(parentEventListener, eventData);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Click
            if (_downValue < AorUIEventConfig.ClickDelay && Vector2.Distance(eventData.position, eventData.pressPosition) < AorUIEventConfig.PosThreshold)
            {
                if (Click)
                {
                    if (onEventClick != null)
                    {
                        onEventClick.Invoke(gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Click(this.gameObject, eventData);
                    }
                }
                else
                {
                    passEventClick(parentEventListener, eventData);
                }
            }
        }
        /// <summary>
        /// 原生EventSystem.Enter事件处理接口实现 (若非强行事件注入,程序员通常不需要调用此方法)
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Over)
            {
                if (onEventOver != null)
                {
                    onEventOver.Invoke(this.gameObject, eventData);
                }
                if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                {
                    _UIManager.AorUIInterfaceLogic.Receive_Over(this.gameObject, eventData);
                }
            }
            else
            {
                passEventOver(parentEventListener, eventData);
            }
        }
        /// <summary>
        /// 原生EventSystem.Exit事件处理接口实现 (若非强行事件注入,程序员通常不需要调用此方法)
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (Out)
            {
                if (onEventOut != null)
                {
                    onEventOut.Invoke(this.gameObject, eventData);
                }
                if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                {
                    _UIManager.AorUIInterfaceLogic.Receive_Out(this.gameObject, eventData);
                }
            }
            else
            {
                passEventOut(parentEventListener, eventData);
            }
        }
        /*
        private bool _isProDrag = false;
        public void OnDrag(PointerEventData eventData) {
            _isProDrag = true;
        }*/

        ///----------------------------------------------------- passEvent 事件传递 方法组;
        public void passEventClick(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Click)
                {
                    if (p.onEventClick != null)
                    {
                        p.onEventClick.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Click(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventClick(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventLongPress(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.LongPress)
                {
                    if (p.onEventLongPress != null)
                    {
                        p.onEventLongPress.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_LongPress(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventLongPress(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventDown(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Down)
                {
                    if (p.onEventDown != null)
                    {
                        p.onEventDown.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Down(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventDown(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventUp(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Up)
                {
                    if (p.onEventUp != null)
                    {
                        p.onEventUp.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Up(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventUp(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventOver(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Over)
                {
                    if (p.onEventOver != null)
                    {
                        p.onEventOver.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Over(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventOver(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventOut(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Out)
                {
                    if (p.onEventOut != null)
                    {
                        p.onEventOut.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Out(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventOut(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventDrag(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Drag)
                {
                    if (p.onEventDrag != null)
                    {
                        p.onEventDrag.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Drag(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventDrag(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventBeginDrag(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Drag)
                {
                    if (p.onEventBeginDrag != null)
                    {
                        p.onEventBeginDrag.Invoke(p.gameObject, eventData);
                    }
                    //if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    //{
                    //    _UIManager.AorUIInterfaceLogic.Receive_Drag(p.gameObject, eventData);
                    //}
                }
                else
                {
                    passEventBeginDrag(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventEndDrag(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Drag)
                {
                    if (p.onEventEndDrag != null)
                    {
                        p.onEventEndDrag.Invoke(p.gameObject, eventData);
                    }
                    //if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    //{
                    //    _UIManager.AorUIInterfaceLogic.Receive_Drag(p.gameObject, eventData);
                    //}
                }
                else
                {
                    passEventEndDrag(p.parentEventListener, eventData);
                }
            }
        }
        public void passEventSwing(AorUIEventListenerForMapArmy p, PointerEventData eventData)
        {
            if (p != null)
            {
                if (p.Swing)
                {
                    if (p.onEventSwing != null)
                    {
                        p.onEventSwing.Invoke(p.gameObject, eventData);
                    }
                    if (_UIManager != null && _UIManager.AorUIInterfaceLogic != null)
                    {
                        _UIManager.AorUIInterfaceLogic.Receive_Swing(p.gameObject, eventData);
                    }
                }
                else
                {
                    passEventSwing(p.parentEventListener, eventData);
                }
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (onEventDrag != null)
            {
                onEventDrag.Invoke(gameObject, eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onEventBeginDrag != null)
            {
                onEventBeginDrag.Invoke(gameObject, eventData);
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEventEndDrag != null)
            {
                onEventEndDrag.Invoke(gameObject, eventData);
            }
        }
    }
}