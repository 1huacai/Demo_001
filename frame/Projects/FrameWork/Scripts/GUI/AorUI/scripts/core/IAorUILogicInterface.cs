using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FrameWork.GUI.AorUI.Core {
    /// <summary>
    /// 全局UI事件逻辑接口
    /// AorUIManager可以挂在一个此接口的实现类,用于处理全局的UI交互事件.
    /// </summary>
    public interface IAorUILogicInterface {
        /// <summary>
        /// 全局Click事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Click(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局LongPress事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_LongPress(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Down事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Down(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Up事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Up(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Over事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Over(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Out事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Out(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Drag事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Drag(GameObject go, PointerEventData eventData);
        /// <summary>
        /// 全局Swing事件响应接口
        /// </summary>
        /// <param name="go">GameObject 触发对象</param>
        /// <param name="eventData">PointerEventData EventSystem事件对象</param>
        void Receive_Swing(GameObject go, PointerEventData eventData);
    }
}
