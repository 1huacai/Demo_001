using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoukiaGame;
using FrameWork.App;
using FrameWork.GUI.AorUI.Animtion;

namespace FrameWork.GUI.AorUI.Core
{
    /// <summary>
    /// 特别注意 :
    /// 
    ///     关于UprefabUnitBase类的CanvasGroup.alpha的问题:
    ///         
    ///         此组件会接管此节点上的CanvasGroup, 所以如果需要实时动态控制CanvasGroup的alpha属性.
    ///         建议在此节点下多设置一个节点并创建一个CanvasGroup用于实现实时动态控制CanvasGroup.alpha属性,否则可能造成alpha数值设置冲突.
    /// 
    /// </summary>
    public abstract class UprefabUnitBase : MonoSwitch
    {

        protected bool _hasOriginCanvasGroup;
        /// <summary>
        /// 表示是否在创建时已经包含CanvasGroup组件了
        /// </summary>
        public bool hasOriginCanvasGroup
        {
            get
            {
                return _hasOriginCanvasGroup;
            }
        }

        protected float _orginCGAlpha;
        public float orginCGAlpha
        {
            get
            {
                return _orginCGAlpha;
            }
        }

        protected CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup
        {
            get
            {
                return _canvasGroup;
            }
        }

        [SerializeField]
        public string id;

        public override void OnAwake ()
        {
            base.OnAwake ();

            _canvasGroup = gameObject.GetComponent<CanvasGroup> ();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup> ();
                _canvasGroup.hideFlags = HideFlags.DontSave;
                _hasOriginCanvasGroup = false;
            }
            else
            {
                _hasOriginCanvasGroup = true;
            }
            _orginCGAlpha = _canvasGroup.alpha;
            _canvasGroup.alpha = 0;
        }

        protected override void Initialization ()
        {
            base.Initialization ();

            AorUIAnimator an = gameObject.GetComponent<AorUIAnimator> ();
            if (an == null)
            {
                _canvasGroup.alpha = _orginCGAlpha;
            }

        }


        protected virtual void OnDestroy ()
        {
            _canvasGroup = null;
        }

        public virtual void Destroy ()
        {

            //修复此bug
            //DOTWEEN :: An error inside a tween callback was silently taken care of > The object of type 'AorWindow' has been destroyed but you are still trying to access it.
            // Your script should either check if it is null or you should not destroy the object.
            if (null == this || gameObject == null)
                return;
            //----------------------

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate (gameObject);
            }
            else
            {
                Destroy (gameObject);
            }
        }

        /// <summary>
        /// 脚本被添加GameObject时执行
        /// </summary>
        protected virtual void Reset ()
        {
            if (Application.isEditor)
            {
                //UiLog.Debug("脚本添加事件 : " + this.name);
            }
        }

        /// <summary>
        /// 检查Inspector面板属性的值发生了变化
        /// </summary>
        protected virtual void OnValidate ()
        {
            if (Application.isEditor)
            {
                //UiLog.Debug("脚本对象数据发生改变事件 ***** ");
            }
        }
    }
}
