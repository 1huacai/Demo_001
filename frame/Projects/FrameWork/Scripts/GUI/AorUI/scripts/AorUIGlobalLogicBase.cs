using UnityEngine;
using UnityEngine.EventSystems;
using FrameWork.App;
using FrameWork.GUI.AorUI.Core;
using CoreFrameWork;

namespace FrameWork.GUI.AorUI
{
    /// <summary>
    /// 全局UI事件逻辑处理基类
    /// </summary>
    public class AorUIGlobalLogicBase : MonoSwitch, IAorUILogicInterface {

        protected AorUIManager _UIManager;
        public AorUIManager UIManager {
            get { return _UIManager; }
        }

        protected override void Initialization() {
            base.Initialization();
            
            GameObject mmg = GameObject.Find(AorUIManager.PrefabName);
            if (mmg == null) {
                Log.Error("Can not find AorUISystem Main in this scene. init fail .");
                return;
            }
            _UIManager = mmg.GetComponent<AorUIManager>();
        }

        protected virtual void OnDestroy() {
            _UIManager = null;
        }

        //==========================================================================

        /// <summary>
        /// 点击事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Click(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Click -> " + go.name);
        }
        /// <summary>
        /// 按下事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Down(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Down -> " + go.name);
        }
        /// <summary>
        /// 放开事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Up(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Up -> " + go.name);
        }
        /// <summary>
        /// 拖拽事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Drag(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Drag -> " + go.name);
        }
        /// <summary>
        /// 长按事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_LongPress(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_LongPress -> " + go.name);
        }
        /// <summary>
        /// swing事件处理接口
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Swing(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Swing -> " + go.name);
        }
        /// <summary>
        /// over事件处理接口 (提示此事件通常触控设备不会产生)
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Over(GameObject go, PointerEventData eventData) {
            // Debug.UiLog("|| Receive_Over -> " + go.name);
        }
        /// <summary>
        /// out事件处理接口 (提示此事件通常触控设备不会产生)
        /// </summary>
        /// <param name="go">事件发布者</param>
        /// <param name="eventData">uGUI原生事件数据</param>
        public virtual void Receive_Out(GameObject go, PointerEventData eventData) {
            //Debug.UiLog("|| Receive_Out -> " + go.name);
        }

    }
}