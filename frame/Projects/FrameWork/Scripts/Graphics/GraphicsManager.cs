using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using FrameWork.App;
using FrameWork.CinemaSystem;
using FrameWork.Graphics.FastShadowProjector;
using CoreFrameWork;

namespace FrameWork.Graphics
{
    /// <summary>
    /// 图形管理器，提供图形性能选项
    /// </summary>
    public class GraphicsManager : SingletonManager<GraphicsManager>
    {
        public enum eCameraOpenState
        {
            None,
            KeepBlack,
            KeepWhite,
            AutoFadeIn,
        }

        public enum eCameraType
        {
            Default,
            Character,
            Sky,
            PreEffect,
            Effect,
            PostEffect
        }


        protected class CameraParma
        {
            public float NearClip;
            public float FarClip;
            public float FogDestance;
            public float FogDestiy;

            public float VolumeFogOffset;
            public float VolumeFogDestiy;

            public bool AllowHDR;

            public CameraParma() : this(false)
            {

            }

            public CameraParma(bool allowHDR)
            {
                this.AllowHDR = allowHDR;
            }
        }

        public bool EnablePostEffect = true;
        public Camera MainCamera;
        public CameraAnimation _mainCameraAnimation;
        public IGraphicDraw DrawCard;

        public LightInfo SunLight;

        private AtmosphereManager AtmosphereMgr;

        public GlobalProjectorManager ShadowMgr;
        public bool StopRender = false;

        private Color cameraBackColor;

        private CameraParma TempCameraParma;
        private CameraParma ActiveCameraParma;
        private CameraParma ConfigCameraParma;


        //安装渲染效果卡
        public CallBack SetupDrawCrad;

        //抗锯齿 1 2 4 8
        public int AntiAliasing;

        /*  public string FPS
          {

              get { return Fps.currFPS.ToString(); }

          }*/

        public Material RenderMaterial
        {

            get { return DrawCard.GetMaterial(); }

        }

        public Camera GetCameraByType(eCameraType camType)
        {

            switch (camType)
            {
                case eCameraType.Default:
                    return MainCamera;
                case eCameraType.Character:
                    return MainCamera;

                case eCameraType.PreEffect:
                    return PreEffectCamera;
                case eCameraType.Effect:
                    return EffectCamera;
                case eCameraType.PostEffect:
                    return PostEffectCamera;
            }

            return MainCamera;
        }





        public GlobalProjectorManager.ShadowResolutions _ShadowResolution = GlobalProjectorManager.ShadowResolutions.Normal_512;
        /// <summary>
        /// 阴影质量
        /// </summary>
        public GlobalProjectorManager.ShadowResolutions ShadowResolution
        {

            get { return _ShadowResolution; }
            set
            {


                if (_ShadowResolution != value)
                {
                    _ShadowResolution = value;
                    ShadowMgr.GlobalShadowResolution = (int)value;
                }

            }

        }



        private GlobalProjectorManager.ShadowType _Shadow = GlobalProjectorManager.ShadowType.None;
        /// <summary>
        /// 是否开阴影
        /// </summary>
        public GlobalProjectorManager.ShadowType ShadowType
        {

            get { return _Shadow; }
            set
            {


                if (_Shadow != value)
                {
                    _Shadow = value;
                    ShadowMgr.GlobalShadowType = value;
                }

            }

        }

        [SerializeField]
        [HideInInspector]
        private bool _showLandscape = true;

        /// <summary>
        /// 是否一直重绘远景
        /// </summary>
        public bool AlwayReDrawFarTexture = false;

        /// <summary>
        /// 绘制精度
        /// </summary>

        public enum RtSize
        {
            /// <summary>
            /// 原始大小 100%
            /// </summary>
            Full = 100,
            /// <summary>
            /// 60%
            /// </summary>
            PointSix = 60,
            /// <summary>
            /// 70%
            /// </summary>
            PointSeven = 70,
            /// <summary>
            /// 对半 50%
            /// </summary>
            Half = 50,
            /// <summary>
            /// 四分之一 25%
            /// </summary>
            Quarter = 25,
        }



        /// <summary>
        /// 摄像机远裁面
        /// </summary>
        public float CameraFarClip
        {

            get { return ActiveCameraParma.FarClip; }
            set
            {
                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.FarClip = value;
                ModifyCameraClip();
            }

        }




        /// <summary>
        /// 摄像机近裁面
        /// </summary>
        public float CameraNearClip
        {

            get { return ActiveCameraParma.NearClip; }
            set
            {


                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.NearClip = value;
                ModifyCameraClip();
            }

        }



        /// <summary>
        /// 大气雾距离
        /// </summary>

        public float FogDestance
        {
            get { return ActiveCameraParma.FogDestance; }
            set
            {

                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.FogDestance = value;
                ModifyCameraClip();

                //AtmosphereMgr.FogDestance = value;
            }
        }

        /// <summary>
        /// 体积雾偏移
        /// </summary>

        public float VolumeFogOffset
        {
            get { return ActiveCameraParma.VolumeFogOffset; }
            set
            {

                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.VolumeFogOffset = value;
                ModifyCameraClip();

                //AtmosphereMgr.FogDestance = value;
            }
        }

        /// <summary>
        /// 体积雾密度
        /// </summary>

        public float VolumeFogDestiy
        {
            get { return ActiveCameraParma.VolumeFogDestiy; }
            set
            {

                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.VolumeFogDestiy = value;
                ModifyCameraClip();

                //AtmosphereMgr.FogDestance = value;
            }
        }


        public void SetShadowCamera(Camera cam = null)
        {
            if (cam == null)
                ShadowMgr.SetMainCamera(MainCamera);
            else
                ShadowMgr.SetMainCamera(cam);
        }


        public ModelShadowProjector AddShadow(GameObject obj)
        {
            if (!obj)
                return null;

            if (ShadowType != GlobalProjectorManager.ShadowType.None)
            {
                ModelShadowProjector _shadow = obj.GetComponent<ModelShadowProjector>();
                if( null == _shadow)
                {
                    _shadow = obj.AddComponent<ModelShadowProjector>();
                }
                return _shadow;
            }

            return null;

        }

        public void SetCameraBackColor(Color col)
        {
            cameraBackColor = col;
            if (_CurrentSubCamera != null)
            {
                _CurrentSubCamera.GetComponent<Camera>().backgroundColor = cameraBackColor;
            }
        }

        /// <summary>
        /// 调整裁剪面
        /// </summary>
        private void ModifyCameraClip()
        {
            if (CurrentSubCamera != null)
            {

                if (CurrentSubCamera.OverrideGraphicSetting)
                {
                    TempCameraParma = new CameraParma();

                    if (CurrentSubCamera.OverrideNearClip != -1)
                        TempCameraParma.NearClip = CurrentSubCamera.OverrideNearClip;
                    else
                        TempCameraParma.NearClip = ActiveCameraParma.NearClip;


                    if (CurrentSubCamera.OverrideFarClip != -1)
                        TempCameraParma.FarClip = CurrentSubCamera.OverrideFarClip;
                    else
                        TempCameraParma.FarClip = ActiveCameraParma.FarClip;


                    if (CurrentSubCamera.OverrideFogDestance != -1)
                        TempCameraParma.FogDestance = CurrentSubCamera.OverrideFogDestance;
                    else
                        TempCameraParma.FogDestance = ActiveCameraParma.FogDestance;

                    if (CurrentSubCamera.OverrideFogDestiy != -1)
                        TempCameraParma.FogDestiy = CurrentSubCamera.OverrideFogDestiy;
                    else
                        TempCameraParma.FogDestiy = ActiveCameraParma.FogDestiy;

                    if (CurrentSubCamera.OverrideVolumeFogOffset != -1)
                        TempCameraParma.VolumeFogOffset = CurrentSubCamera.OverrideVolumeFogOffset;
                    else
                        TempCameraParma.VolumeFogOffset = ActiveCameraParma.VolumeFogOffset;

                    if (CurrentSubCamera.OverrideVolumeFogDestiy != -1)
                        TempCameraParma.VolumeFogDestiy = CurrentSubCamera.OverrideVolumeFogDestiy;
                    else
                        TempCameraParma.VolumeFogDestiy = ActiveCameraParma.VolumeFogDestiy;


                    ActiveCameraParma = TempCameraParma;
                }
                else
                {
                    ActiveCameraParma = ConfigCameraParma;
                }


                if (PreEffectCamera == null)
                {

                    CreateEffectCamera(ref PreEffectCamera, "preEffect", 10, eCameraType.PreEffect);
                    //CreateEffectCamera(ref EffectCamera, "effect", 15, eCameraType.Effect, CameraClearFlags.Depth);
                    CreateEffectCamera(ref EffectCamera, "effect", 15, eCameraType.Effect);
                    CreateEffectCamera(ref PostEffectCamera, "postEffect", 20, eCameraType.PostEffect);

                    SetAllowHDR(PreEffectCamera, false);
                    SetAllowHDR(EffectCamera, false);
                    SetAllowHDR(PostEffectCamera, false);

                }

                Camera cam = CurrentSubCamera.GetComponent<Camera>();
                cam.nearClipPlane = ActiveCameraParma.NearClip;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = cameraBackColor;
                cam.nearClipPlane = ActiveCameraParma.NearClip;
                cam.farClipPlane = ActiveCameraParma.FarClip;


                AtmosphereMgr.FogDestance = ActiveCameraParma.FogDestance;
                AtmosphereMgr.FogDestiy = ActiveCameraParma.FogDestiy;
                AtmosphereMgr.VolumeFogOffset = ActiveCameraParma.VolumeFogOffset;
                AtmosphereMgr.VolumeFogDestiy = ActiveCameraParma.VolumeFogDestiy;

                setClipPlane(PreEffectCamera);
                setClipPlane(EffectCamera);
                setClipPlane(PostEffectCamera);

            }
        }

        //可自定义到任何一个渲染位置的黑屏

        private GameObject _darkOcclusion;
        private float _darkDuration;
        private float _darkLum;
        private float _darkFade;
        private Material _darkMaterial;

        private void darkOcclusionUpdate()
        {
            if (_darkOcclusion != null)
            {
                if (_darkDuration > 0)
                {
                    if (!_darkOcclusion.activeInHierarchy)
                    {
                        _darkOcclusion.SetActive(true);
                    }
                    float lum = _darkMaterial.GetFloat("_Lum");
                    lum = MYMath.Lerp(lum, _darkLum, Time.deltaTime / _darkFade);
                    _darkMaterial.SetFloat("_Lum", lum);
                    _darkDuration -= Time.deltaTime;
                }
                else if (_darkOcclusion.activeInHierarchy)
                {
                    _darkLum = 1;
                    float lum = _darkMaterial.GetFloat("_Lum");
                    lum = MYMath.Lerp(lum, _darkLum, Time.deltaTime / _darkFade);
                    if (lum > 0.95f)
                    {
                        _darkOcclusion.SetActive(false);
                    }
                    else
                    {
                        _darkMaterial.SetFloat("_Lum", lum);
                    }
                }
            }

        }

        public void DarkOcclusion(float lum, float time, string layerName = "effect", float fade = 0.2f)
        {
            if (_darkOcclusion == null)
            {
                _darkOcclusion = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _darkOcclusion.transform.SetParent(MainCamera.transform, false);
                _darkOcclusion.transform.localPosition = new Vector3(0, 0, 10);
                _darkOcclusion.GetComponent<Collider>().enabled = false;
                _darkOcclusion.transform.localScale = new Vector3(999, 999, 0);
                _darkMaterial = new Material(ApplicationCore.Instance.GetConstShader("Custom/Other/DarkOcclusion"));
                _darkMaterial.SetFloat("_Lum", 1);
                _darkOcclusion.GetComponent<MeshRenderer>().material = _darkMaterial;
                _darkOcclusion.layer = LayerMask.NameToLayer(layerName);
            }
            _darkLum = lum;
            _darkDuration = Math.Max(time, _darkDuration);
            _darkFade = fade;
        }




        private void setClipPlane(Camera camera)
        {
            if (camera != null)
            {
                //特效摄像机必须和主摄像机裁剪一样,否则深度出错
                Camera cam = CurrentSubCamera.GetComponent<Camera>();
                camera.nearClipPlane = cam.nearClipPlane;
                camera.farClipPlane = cam.farClipPlane;
            }
        }

        /// <summary>
        /// 大气雾浓度
        /// </summary>
        public float FogDestiy
        {
            get { return ActiveCameraParma.FogDestiy; }
            set
            {

                if (!Application.isPlaying)
                    return;

                ActiveCameraParma.FogDestiy = value;
                ModifyCameraClip();

            }
        }

        /// <summary>
        /// debug模式
        /// </summary>
        public bool DebugMode = false;


        List<SubCameraInfo> SubCameraList = new List<SubCameraInfo>();

        /// <summary>
        /// 当前摄像机
        /// </summary>
        private SubCameraInfo _CurrentSubCamera;




        private Vector3 LastSubCameraPos;
        private Vector3 LastSubCameraRot;
        public bool _isInited = false;

        public MainDraw FinalDraw;
        public RawImage UIimage;


        public void SetDrawPassCount(int index)
        {
            FinalDraw.DrawPassCount = index;
        }

        /// <summary>
        /// 设置RT背景的透明度
        /// </summary>
        public void SetRenderTargetAlpha(float alpha)
        {


            if (UIimage != null)
            {
                UIimage.material.SetFloat("_AlphaAdd", alpha);
            }
        }

        /// <summary>
        /// 是否已经启动
        /// </summary>
        public bool isInited
        {
            get { return _isInited; }
        }

        /// <summary>
        /// 当前摄像机
        /// </summary>
        public SubCameraInfo CurrentSubCamera
        {
            set
            {
                _CurrentSubCamera = value;
                if (value == null)
                    return;

                ApplicationCore.Instance.GlobalEvent.DispatchEvent(GameEvent.SubCameraInfoChange, MainCamera, _CurrentSubCamera.MainCamera);
                _mainCameraAnimation.OnSwitch(_CurrentSubCamera.OpenState);
                ModifyCameraClip();

            }
            get
            {

                return _CurrentSubCamera;
            }
        }

        internal Camera PreEffectCamera;
        internal Camera EffectCamera;
        internal Camera PostEffectCamera;
        internal Camera FinalCamera;


        private float CullingSize = 24;//剔除范围

        private void CreateEffectCamera(ref Camera effectCamera, string layer, int depth, eCameraType camType, CameraClearFlags flag = CameraClearFlags.Nothing)
        {
            GameObject tmp = new GameObject(layer + "Camera");
            effectCamera = tmp.AddComponent<Camera>();
            SetAllowHDR(effectCamera, false);
            // collider.enabled = false;
            effectCamera.transform.parent = transform;
            effectCamera.transform.position = _CurrentSubCamera.transform.position;
            effectCamera.transform.eulerAngles = _CurrentSubCamera.transform.eulerAngles;

            Camera cam = CurrentSubCamera.GetComponent<Camera>();
            SetAllowHDR(cam, false);
            effectCamera.clearFlags = flag;
            effectCamera.fieldOfView = cam.fieldOfView;
            effectCamera.nearClipPlane = cam.nearClipPlane;
            effectCamera.farClipPlane = cam.farClipPlane;
            effectCamera.depth = depth;
            effectCamera.cullingMask = 1 << LayerMask.NameToLayer(layer);
            PostEffectDraw ped = effectCamera.gameObject.AddComponent<PostEffectDraw>();
            ped.mgr = this;
            ped.enabled = false;
            ped.cameraType = camType;



        }


        private void UpdateEffectCamera(Camera effectCamera)
        {
            if (effectCamera == null)
                return;

            Camera cam = CurrentSubCamera.GetComponent<Camera>();
            SetAllowHDR(cam, false);
            effectCamera.enabled = !StopRender & CurrentSubCamera != null;
            effectCamera.fieldOfView = cam.fieldOfView;
            effectCamera.transform.position = _CurrentSubCamera.transform.position;
            effectCamera.transform.eulerAngles = _CurrentSubCamera.transform.eulerAngles;

        }



        [SerializeField]
        [HideInInspector]
        private int _Quality = -1;


        /// <summary>
        /// 品质等级=unity设置内的品质等级
        /// </summary>
        public int GraphicsQuality
        {
            get
            {
                if (_Quality == -1)
                {
                    _Quality = QualitySettings.GetQualityLevel();
                }
                return _Quality;
            }
            set
            {
                _Quality = value;
                QualitySettings.SetQualityLevel(value);
            }
        }

        /// <summary>
        /// 注册摄像机
        /// </summary>
        /// <param name="cameraInfo">要注册的摄像机</param>
        public void RegisterSubCamera(SubCameraInfo cameraInfo)
        {
            if (!SubCameraList.Contains(cameraInfo))
            {

                SubCameraList.Add(cameraInfo);

                UpdateCurrentSubCamera();

            }

        }

        /// <summary>
        /// 移除摄像机
        /// </summary>
        /// <param name="cameraInfo"></param>
        public void RemoveSubCamera(SubCameraInfo cameraInfo)
        {
            if (this == null || cameraInfo == null || this.gameObject == null)
                return;

            if (SubCameraList.Contains(cameraInfo))
            {
                SubCameraList.Remove(cameraInfo);
                //  Log("SubCamera Destory!");
            }

            UpdateCurrentSubCamera();
        }

        private void LogInfo(string str)
        {
            if (DebugMode)
            {
                Log.Info(str);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="onFinish">成功后的回调</param>
        public void Init(CallBack onFinish)
        {
            DisplayConfig config = ConfigManager.Instance.GetConfigFromDic<DisplayConfig>(789123);


            //FastShadowProjector结构生成
            ShadowMgr = GlobalProjectorManager.CreateInstance();
            ShadowMgr.transform.parent = transform;



            //AtmosphereManager结构生成
            GameObject obj = new GameObject("AtmosphereManager");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            AtmosphereMgr = obj.AddComponent<AtmosphereManager>();

            //主摄像机
            obj = new GameObject("MainCamera");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            MainCamera = obj.AddComponent<Camera>();
            SetAllowHDR(MainCamera, false);
            _mainCameraAnimation = MainCamera.gameObject.AddComponent<CameraAnimation>();
            MainCamera.clearFlags = CameraClearFlags.Color;
            MainCamera.backgroundColor = Color.black;
            Camera cam = MainCamera.GetComponent<Camera>();
            SetAllowHDR(cam, false);
            cam.depth = 0;
            cam.cullingMask = 1 << LayerMask.NameToLayer("Default");


            ShadowMgr.SetMainCamera(MainCamera);

            //FinalCamera结构生成
            obj = new GameObject("FinalCamera");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            cam = obj.AddComponent<Camera>();
            SetAllowHDR(cam, false);
            cam.clearFlags = CameraClearFlags.Nothing;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            cam.depth = 30;
            FinalDraw = obj.AddComponent<MainDraw>();
            FinalCamera = cam;
            SetAllowHDR(FinalCamera, false);
            // Fps = obj.AddComponent<FPS>();

            //drawCard插入
            SetupDrawCrad();



            //序列化数据读取
            ConfigCameraParma = new CameraParma();
            ShadowType = config == null ? GlobalProjectorManager.ShadowType.None : config.ShadowType;
            ShadowResolution = config == null ? GlobalProjectorManager.ShadowResolutions.VeryLow_128 : config.ShadowResolutions;

            AntiAliasing = config == null ? 1 : config.AntiAliasing;


            ConfigCameraParma.FogDestance = config == null ? 60 : config.FogDistance;
            ConfigCameraParma.FogDestiy = config == null ? 20 : config.FogDestiy;

            ConfigCameraParma.FarClip = config == null ? 500 : config.FarCameraClip;
            ConfigCameraParma.NearClip = config == null ? 0.1f : config.NearCameraClip;

            //screenSize = new Vector2(Screen.width, Screen.height);
            GraphicsQuality = config == null ? 0 : config.GraphicQuality;
            AtmosphereMgr.Init();
            ActiveCameraParma = ConfigCameraParma;
            UpdateCurrentSubCamera();
            FinalDraw.gameObject.SetActive(true);

            FinalDraw.enabled = true;
            FinalDraw.Init();
            _isInited = true;

            if (onFinish != null)
                onFinish();

            GameObject lgo = new GameObject("SunLight");
            lgo.transform.SetParent(transform, false);
            lgo.transform.localEulerAngles = new Vector3(45, 45, 0);
            Light l = lgo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1;
            SunLight = lgo.AddComponent<LightInfo>();

            // gameObject.AddComponent<FPS>();
            //Application.targetFrameRate = 25;
        }


        public void SetAllowHDR(Camera camera, bool allowHDR)
        {
            if (camera)
            {
                camera.allowHDR = allowHDR;
            }
        }


        /// <summary>
        /// 设置屏幕遮罩颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetMaskColor(Color color)
        {

            if (UIimage != null)
            {
                UIimage.material.SetColor("_MaskColor", color);
            }
            else
            {
                RenderMaterial.SetColor("_MaskColor", color);
            }

        }
        /// <summary>
        /// 设置屏幕曝光度
        /// </summary>
        /// <param name="exp"></param>
        public void SetExposure(float exp)
        {
            if (UIimage != null)
            {
                UIimage.material.SetFloat("_Exposure", exp);
            }
            else
            {
                RenderMaterial.SetFloat("_Exposure", exp);
            }



        }

        public void UpdateCurrentSubCamera()
        {
            if (SubCameraList.Count == 0)
                return;

            SubCameraInfo select = null;
            for (int i = 0; i < SubCameraList.Count; i++)
            {
                if (SubCameraList[i] && SubCameraList[i].gameObject.activeInHierarchy)
                    select = SubCameraList[i];

            }

            if (select == null)
            {
                CurrentSubCamera = null;
                return;
            }



            for (int i = 0; i < SubCameraList.Count; i++)
            {
                if (!SubCameraList[i])
                    continue;

                if (SubCameraList[i].gameObject.activeInHierarchy && SubCameraList[i].Level >= select.Level)
                {
                    select = SubCameraList[i];
                }

            }

            if (select)
            {
                CurrentSubCamera = select;

            }

        }


        private void UpdateMainCamera()
        {

            MainCamera.enabled = !StopRender & CurrentSubCamera != null;
            MainCamera.transform.position = CurrentSubCamera.transform.position;
            MainCamera.transform.eulerAngles = CurrentSubCamera.transform.eulerAngles;

            Camera cam = CurrentSubCamera.GetComponent<Camera>();
            Camera maincam = MainCamera.GetComponent<Camera>();
            MainCamera.farClipPlane = cam.farClipPlane;
            MainCamera.nearClipPlane = cam.nearClipPlane;
            // MainCamera.backgroundColor = CurrentSubCamera.camera.backgroundColor;
            //  MainCamera.clearFlags = CurrentSubCamera.camera.clearFlags;
            maincam.cullingMask = 1 << LayerMask.NameToLayer("Default");
            MainCamera.fieldOfView = cam.fieldOfView;
            MainCamera.orthographic = cam.orthographic;
            MainCamera.orthographicSize = cam.orthographicSize;

            _mainCameraAnimation.AnimUpdate();

            cam.GetComponent<Camera>().enabled = false;
        }

        private int changeModeTime;


        // Update is called once per frame
        void LateUpdate()
        {



            darkOcclusionUpdate();

            if (CurrentSubCamera == null || !CurrentSubCamera.gameObject.activeInHierarchy)
            {
                CurrentSubCamera = null;
                UpdateCurrentSubCamera();

            }

            if (CurrentSubCamera == null)
            {
                if (MainCamera != null)
                {
                    MainCamera.backgroundColor = Color.black;
                    MainCamera.clearFlags = CameraClearFlags.SolidColor;
                    MainCamera.cullingMask = 0;
                }

                return;
            }
            else
            {

                UpdateMainCamera();
            }


            UpdateEffectCamera(PreEffectCamera);
            UpdateEffectCamera(EffectCamera);
            UpdateEffectCamera(PostEffectCamera);


        }


        /// <summary>
        /// 抖动当前摄像机
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="power">力度</param>
        public void CameraShake(float time, float power)
        {

            if (_CurrentSubCamera != null)
                _CurrentSubCamera.transform.DOShakePosition(time, new Vector3(power, power, 0), 100).SetEase(Ease.Linear);
        }



        /// <summary>
        /// 当前摄像机渐出(变暗)
        /// </summary>
        public void FadeOutCurrentCamera(float time = 0.5f)
        {
            if (_mainCameraAnimation != null)
                _mainCameraAnimation.SmoothFadeOut(time);
        }

        /// <summary>
        /// 当前摄像机渐入
        /// </summary>
        public void FadeInCurrentCamera()
        {
            if (_mainCameraAnimation != null)
                _mainCameraAnimation.SmoothFadeIn();
        }
    }

}


