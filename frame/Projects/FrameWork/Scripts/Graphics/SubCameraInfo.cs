using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using FrameWork.App;
using FrameWork.Graphics;
using FrameWork.Manager;

namespace FrameWork.CinemaSystem
{
    /// <summary>
    /// 摄像机系统基类
    /// </summary>
    public class SubCameraInfo : MonoSwitch
    {

        /// <summary>
        /// 切换到该摄像机时是否由黑到亮过渡
        /// </summary>
        public GraphicsManager.eCameraOpenState OpenState = GraphicsManager.eCameraOpenState.None;
        [SerializeField]
        private int _Level;

        /// <summary>
        /// 优先级,同时存在的摄像机取 高优先级的
        /// </summary>
        public int Level
        {

            get { return _Level; }
            set
            {
                _Level = Mathf.Max(0, value);
                if (ApplicationCore.IsInited)
                    SingletonManager.GetManager<GraphicsManager>().UpdateCurrentSubCamera();

            }


        }

        /// <summary>
        /// 覆盖图形参数
        /// </summary>
        public bool OverrideGraphicSetting;
        public float OverrideNearClip = -1;
        public float OverrideMiddleClip = -1;
        public float OverrideFarClip = -1;
        public float OverrideFogDestance = -1;
        public float OverrideFogDestiy = -1;
        public float OverrideVolumeFogOffset = -1;
        public float OverrideVolumeFogDestiy = -1;

        public Camera MainCamera
        {
            get
            {
                if(null == m_mainCamera)
                {
                    m_mainCamera = this.GetComponent<Camera>();
                }

                return m_mainCamera;
            }
        }

        private Camera m_mainCamera;

        public override void OnAwake()
        {
            base.OnAwake();
            MainCamera.enabled = false;

            waitYKApplicationWhenDo(this, () =>
            {
                SingletonManager.GetManager<GraphicsManager>().RegisterSubCamera(this);
            });

        }

        protected override void OnEnable()
        {
            base.OnEnable();

            waitYKApplicationWhenDo(this, () =>
            {
                SingletonManager.GetManager<GraphicsManager>().UpdateCurrentSubCamera();
            });

        }

        protected override void OnDisable()
        {

            base.OnDisable();
            if (Application.isPlaying && ApplicationCore.IsInited)
            {
                GraphicsManager gm = SingletonManager.GetManager<GraphicsManager>();
                if (gm != null)
                    gm.UpdateCurrentSubCamera();
            }
        }



        protected override void OnDestroy()
        {

            if (ApplicationCore.IsInited)
                SingletonManager.GetManager<GraphicsManager>().RemoveSubCamera(this);

        }
    }
}


