
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FrameWork.GUI.AorUI.Animtion;
using FrameWork.GUI.AorUI.Components;
using FrameWork.GUI.AorUI.Core;
using FrameWork.GUI.AorUI.layout;
using DG.Tweening;
using CoreFrameWork;
namespace FrameWork.GUI.AorUI
{
    /// <summary>
    /// 背景的对齐方式
    /// </summary>
    public enum BGAlignType
    {
        tLeft, top, tRight,
        left, center, right,
        bLeft, bottom, bRight
    }

    /// <summary>
    /// AorUI舞台缩放方式定义
    /// </summary>
    public enum ScaleModeType
    {
        /// <summary>
        /// Unity缩放方式
        /// </summary>
        normal,
        /// <summary>
        /// 不缩放,已设计尺寸显示舞台大小
        /// </summary>
        noScale,
        /// <summary>
        /// 基于宽度缩放,再匹配高度
        /// </summary>
        widthBase,
        /// <summary>
        /// 基于高度缩放,再匹配宽度
        /// </summary>
        heightBase,
        /// <summary>
        /// 等比缩放到屏幕内部.
        /// </summary>
        fitScreen,
        /// <summary>
        /// 等比缩放裁切超出部分
        /// </summary>
        envelopeScreen
    }

    /// <summary>
    /// AorUIManger 
    /// 核心管理器.
    /// </summary>
    public class AorUIManager : MonoBehaviour
    {

        public const string PrefabName = "__AorUISystem";
        [SerializeField]//此序列化只用于编辑器模式下查看数据使用
        private ScaleModeType _scaleMode = ScaleModeType.noScale;
        /// <summary>
        /// 舞台缩放方式
        /// </summary>
        public ScaleModeType ScaleMode
        {
            get { return _scaleMode; }
            set
            {
                _scaleMode = value;
                //if (_isStartd) {
                changeScaleMode();
                //}
            }
        }

        [SerializeField]//此序列化只用于编辑器模式下查看数据使用
        private Vector2 _stageSize = new Vector2(960f, 640f);
        /// <summary>
        /// 舞台的设计尺寸
        /// </summary>
        public Vector2 StageSize
        {
            get { return _stageSize; }
            set
            {
                _stageSize = value;
                //if (_isStartd) {
                changeScaleMode();
                //}
            }
        }

        /// <summary>
        /// UICamera引用
        /// </summary>
        public Camera UICamera { get; private set; }

        private Canvas _canvas;
        private Vector2 _canvasRectCache;

        private CanvasGroup _stageCanvasGroup;
        public CanvasGroup StageCanvasCroup
        {
            get { return _stageCanvasGroup; }
        }

        private RectTransform _Stage;
        /// <summary>
        /// 舞台RectTransform实例的应用
        /// </summary>
        public RectTransform Stage
        {
            get { return _Stage; }
        }


        private RectTransform _FloatUIBackLayer;
        /// <summary>
        /// UI背景层
        /// </summary>
        public RectTransform FloatUIBackLayer
        {
            get
            {
                return _FloatUIBackLayer;
            }
        }


        private RectTransform _UIBackLayer;
        /// <summary>
        /// UI背景层
        /// </summary>
        public RectTransform UIBackLayer
        {
            get
            {
                return _UIBackLayer;
            }
        }

        private RectTransform _FloatFrontUIBackLayer;
        /// <summary>
        /// UI背景层
        /// </summary>
        public RectTransform FloatFrontUIBackLayer
        {
            get
            {
                return _FloatFrontUIBackLayer;
            }
        }

        private RectTransform _FloatingLayerA;
        /// <summary>
        /// 底层浮动层
        /// </summary>
        public RectTransform FloatingLayerA
        {
            get
            {
                return _FloatingLayerA;
            }
        }

        private RectTransform _FloatingLayerB;
        /// <summary>
        /// 底层浮动层
        /// </summary>
        public RectTransform FloatingLayerB
        {
            get { return _FloatingLayerB; }
        }

        private RectTransform _FloatingLayerC;
        /// <summary>
        /// 底层浮动层
        /// </summary>
        public RectTransform FloatingLayerC
        {
            get
            {
                return _FloatingLayerC;
            }
        }

        private RectTransform _FloatingLayerT;
        /// <summary>
        /// 顶层浮动层
        /// </summary>
        public RectTransform FloatingLayerT
        {
            get { return _FloatingLayerT; }
        }

        private RectTransform _FloatingLayerX;
        /// <summary>
        /// 顶层浮动层
        /// </summary>
        public RectTransform FloatingLayerX
        {
            get { return _FloatingLayerX; }
        }

        //        private List<RectTransform> _LayerList;
        //        /// <summary>
        //        /// 用户层列表
        //        /// </summary>
        //        public List<RectTransform> LayerList
        //        {
        //            get { return _LayerList; }
        //        }


        private bool _isAwakeInit = false;
        public bool isAwakeInit
        {
            get { return _isAwakeInit; }
        }

        private bool _isInit = false;
        public bool isInit
        {
            get { return _isInit; }
        }

        private float _NotchScreenPixel = 120;
        /// <summary>
        /// 刘海像素
        /// </summary>
        public float NotchScreenPixel
        {
            get { return _NotchScreenPixel; }
            set
            {
                _NotchScreenPixel = value;
                updateStageSize();
            }
        }
        //------------------------------------------------------------------------ 接口组 

        /// <summary>
        /// 完成初始化的回调
        /// </summary>
        private Action<AorUIManager> _onManagerStarted;

        //------------------------------------------------------------------------ 接口组 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void SetManagerStartedAction(Action<AorUIManager> callback)
        {
            _onManagerStarted = callback;
        }

        //awake
        void Awake()
        {
            AwakeInit();
        }

        public void AwakeInit()
        {

            //_LayerList = new List<RectTransform>();

            _UICacheList = new Dictionary<string, AorSwitchableUI>();

            if (UICamera == null)
            {
                UICamera = transform.Find("AorUICamera#").GetComponent<Camera>();
            }

            initEvent();

            _isAwakeInit = true;
        }

        private void initEvent()
        {
            if (m_onAorSwitchableUIInited == null)
            {
                m_onAorSwitchableUIInited = new AorSwitchableUIEvent();
            }
            if (m_onAorSwitchableUIOpen == null)
            {
                m_onAorSwitchableUIOpen = new AorSwitchableUIEvent();
            }
            if (m_onAorSwitchableUIClose == null)
            {
                m_onAorSwitchableUIClose = new AorSwitchableUIEvent();
            }
        }

        private void destroyEvent()
        {
            if (m_onAorSwitchableUIInited != null)
            {
                m_onAorSwitchableUIInited.RemoveAllListeners();
                m_onAorSwitchableUIInited = null;
            }
            if (m_onAorSwitchableUIOpen != null)
            {
                m_onAorSwitchableUIOpen.RemoveAllListeners();
                m_onAorSwitchableUIOpen = null;
            }
            if (m_onAorSwitchableUIClose != null)
            {
                m_onAorSwitchableUIClose.RemoveAllListeners();
                m_onAorSwitchableUIClose = null;
            }
        }

        public void init()
        {
            Transform _floatBackCanvasT = transform.Find("AorFloatBackUICanvas#");

            Transform canvasT = transform.Find("AorUICanvas#");
            Transform _floatFrontCanvasT = transform.Find("AorFloatFrontUICanvas#");


            if (_canvas == null)
            {
                _canvas = canvasT.GetComponent<Canvas>();
            }

            _canvasRectCache = new Vector2(_canvas.pixelRect.width, _canvas.pixelRect.height);
            //_canvasRectCache = new Vector2(_stageSize.x, _stageSize.y);
            //挂载backgroud对象
            if (_background == null)
            {
                _background = canvasT.Find("AorUIBG#").GetComponent<RectTransform>();
            }
            //CanvasGroup bgCanvasGroup = _background.GetComponent<CanvasGroup>();
            //if (bgCanvasGroup == null)
            //{
            //    bgCanvasGroup = _background.gameObject.AddComponent<CanvasGroup>();
            //}
            //bgCanvasGroup.blocksRaycasts = false;

            if (isHideBG)
            {
                _background.gameObject.SetActive(false);
            }


            //RT
            if (_renderTexture == null)
            {
                _renderTexture = canvasT.Find("AorRT#").GetComponent<RectTransform>();
            }
            //CanvasGroup rtCanvasGroup = _renderTexture.GetComponent<CanvasGroup>();
            //if (rtCanvasGroup == null)
            //{
            //    rtCanvasGroup = _renderTexture.gameObject.AddComponent<CanvasGroup>();
            //}
            //rtCanvasGroup.blocksRaycasts = false;


            //挂载FloatUIBackLayer对象
            if (FloatUIBackLayer == null)
            {

                _FloatUIBackLayer = _floatBackCanvasT.Find("FloatUIBackLayer#").GetComponent<RectTransform>();


                //_FloatUIBackLayer.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatUIBackLayer.gameObject.activeInHierarchy)
                {
                    FloatUIBackLayer.gameObject.SetActive(true);
                }
            }

            //挂载UIBackLayer对象
            if (UIBackLayer == null)
            {
                _UIBackLayer = canvasT.Find("UIBackLayer#").GetComponent<RectTransform>();
                //_UIBackLayer.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !UIBackLayer.gameObject.activeInHierarchy)
                {
                    UIBackLayer.gameObject.SetActive(true);
                }
            }

            //挂载FloatFrontUIBackLayer对象
            if (FloatFrontUIBackLayer == null)
            {

                _FloatFrontUIBackLayer = _floatFrontCanvasT.Find("FloatFrontUIBackLayer#").GetComponent<RectTransform>();


                //_FloatFrontUIBackLayer.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatFrontUIBackLayer.gameObject.activeInHierarchy)
                {
                    FloatFrontUIBackLayer.gameObject.SetActive(true);
                }
            }



            //挂载FloatingLayer_A对象
            if (FloatingLayerA == null)
            {
                _FloatingLayerA = transform.Find("FloatingLayer_A#").GetComponent<RectTransform>();
                //_FloatingLayerA.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatingLayerA.gameObject.activeInHierarchy)
                {
                    FloatingLayerA.gameObject.SetActive(true);
                }
            }

            //挂载FloatingLayer_B对象
            if (FloatingLayerB == null)
            {
                _FloatingLayerB = transform.Find("FloatingLayer_B#").GetComponent<RectTransform>();
                //_FloatingLayerB.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatingLayerB.gameObject.activeInHierarchy)
                {
                    FloatingLayerB.gameObject.SetActive(true);
                }
            }

            //挂载FloatingLayer_C对象
            if (FloatingLayerC == null)
            {
                _FloatingLayerC = transform.Find("FloatingLayer_C#").GetComponent<RectTransform>();
                //_FloatingLayerC.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatingLayerC.gameObject.activeInHierarchy)
                {
                    FloatingLayerC.gameObject.SetActive(true);
                }
            }

            //挂载FloatingLayer_X对象
            if (FloatingLayerX == null)
            {
                Transform _tran = transform.Find("FloatingLayer_X#");
                if (null != _tran)
                {
                    _FloatingLayerX = _tran.GetComponent<RectTransform>();
                }
                if (Application.isPlaying && !FloatingLayerX.gameObject.activeInHierarchy)
                {
                    FloatingLayerX.gameObject.SetActive(true);
                }
            }

 
            ////挂载stage对象
            //if (Stage == null)
            //{
            //    _Stage = canvasT.Find("AorUIStage#").GetComponent<RectTransform>();
            //}
            //_stageCanvasGroup = Stage.GetComponent<CanvasGroup>();
            //if (_stageCanvasGroup == null)
            //{
            //    _stageCanvasGroup = Stage.gameObject.AddComponent<CanvasGroup>();
            //}

            ////强制stage不接受Raycast
            //_stageCanvasGroup.blocksRaycasts = false;



            //挂载FloatingLayer_T对象
            if (FloatingLayerT == null)
            {
                _FloatingLayerT = canvasT.Find("FloatingLayer_T#").GetComponent<RectTransform>();
                //_FloatingLayerT.gameObject.AddComponent<CanvasGroup>();
                if (Application.isPlaying && !FloatingLayerT.gameObject.activeInHierarchy)
                {
                    FloatingLayerT.gameObject.SetActive(true);
                }
            }

            if (null == Stage)
            {
                return;
            }

            //装载静态layer (已废弃)
            int i, length = Stage.childCount;
            for (i = 0; i < length; i++)
            {
                RectTransform rt = Stage.GetChild(i).GetComponent<RectTransform>();
                if (rt != null)
                {

                    CanvasGroup cg = rt.GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        cg = rt.gameObject.AddComponent<CanvasGroup>();
                    }

                    if (_canRaycast)
                    {
                        cg.ignoreParentGroups = true;
                    }
                    else
                    {
                        cg.ignoreParentGroups = false;
                    }
                }
            }

            //EventSystem
            GameObject esGO = GameObject.Find("EventSystem");
            if (esGO == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            //检查是否挂载IAorLogicInterface
            _AorUIInterfaceLogic = gameObject.GetInterface<IAorUILogicInterface>();
            if (_AorUIInterfaceLogic == null)
            {
                Log.Info("AorUIManager.Start : this manager can not find UI Interface Logic.");
            }

            //加载scaleMode
            changeScaleMode();
            changeScaleModeBG();

            if (!Application.isEditor)
            {
                DontDestroyOnLoad(this);
            }

            _isInit = true;
            Log.Info("AorUIManager.Started");

            StartCoroutine(runManagerStartedFunc());
        }

        //start
        void Start()
        {
        }

        void OnDestroy()
        {
            if (_UICacheList != null)
            {
                _UICacheList.Clear();
                _UICacheList = null;
            }

            //            if (LayerList != null)
            //            {
            //                LayerList.Clear();
            //                _LayerList = null;
            //            }

            destroyEvent();

            _AorUIInterfaceLogic = null;
            _Stage = null;

        }

        private int _startedDoWaitNum;
        private int _startedDoWaitMax = 1000;

        IEnumerator runManagerStartedFunc()
        {
            while (true)
            {
                yield return 0;
                if (_onManagerStarted != null)
                {
                    _onManagerStarted(this);
                    break;
                }
                _startedDoWaitNum++;
                if (_startedDoWaitNum > _startedDoWaitMax)
                {
                    Log.Info("AorUIManager._onManagerStarted no set !");
                    break;
                }
            }
        }
        //
        private bool _isDirty = false;

        private Vector2 _creentCanvasRect;
        // Update is called once per frame
        public void Update()
        {

            if (!_isAwakeInit || !_isInit) return;

            if (UICamera)
            {
                UICamera.transform.localPosition = new Vector3(
                    UICamera.transform.localPosition.x,
                    UICamera.transform.localPosition.y,
                    -(_canvas.planeDistance)
                                                                );
            }
            if (!_isDirty) return;
            _creentCanvasRect = new Vector2(_canvas.pixelRect.width, _canvas.pixelRect.height);
            if (_creentCanvasRect != _canvasRectCache)
            {
                // Debug.Log("||AorUIManager.CanvasRect changed :: [ " + _creentCanvasRect.x + " , " + _creentCanvasRect.y + " ].");
                _canvasRectCache = _creentCanvasRect;
                updateStageSize();
                updateBGSize();
            }
        }

        private void updateBGSize()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                if (!isAwakeInit || !isInit)
                {
                    return;
                }
            }

            switch (_bgScaleMode)
            {
                case ScaleModeType.widthBase:
                    set2WidthBaseModeBG();
                    break;
                case ScaleModeType.heightBase:
                    set2HeightBaseModeBG();
                    break;
                case ScaleModeType.fitScreen:
                    set2FitScreenModeBG();
                    break;
                case ScaleModeType.envelopeScreen:
                    set2EnvelopeScreenModeBG();
                    break;
                default:
                    break;
            }

        }

        private void updateStageSize()
        {

            if (Application.isEditor && !Application.isPlaying)
            {
                if (!isAwakeInit || !isInit)
                {
                    return;
                }
            }

            switch (_scaleMode)
            {
                case ScaleModeType.widthBase:
                    set2WidthBaseMode();
                    break;
                case ScaleModeType.heightBase:
                    set2HeightBaseMode();
                    break;
                case ScaleModeType.fitScreen:
                    set2FitScreenMode();
                    break;
                case ScaleModeType.envelopeScreen:
                    set2EnvelopeScreenMode();
                    break;
                default:
                    break;
            }

        }
        public Vector3 GetWidthBaseMode()
        {
            float b = _canvasRectCache.x / _stageSize.x;
            return new Vector3(b, b, b);
        }

        private void set2WidthBaseMode()
        {
            float b = _canvasRectCache.x / _stageSize.x;
            Stage.localScale = new Vector3(b, b, b);
            float c = _canvasRectCache.y - _stageSize.y * b;
            Stage.sizeDelta = new Vector2(Stage.sizeDelta.x, _stageSize.y + c / b);
        }

        public Vector3 GetHeightBaseMode()
        {
            float b = _canvasRectCache.y / _stageSize.y;
            return new Vector3(b, b, b);
        }
        //0.035
        private void set2HeightBaseMode()
        {
            float b = _canvasRectCache.y / _stageSize.y;
            Stage.localScale = new Vector3(b, b, b);
            float c = _canvasRectCache.x - (_stageSize.x + NotchScreenPixel) * b;
            Stage.sizeDelta = new Vector2(_stageSize.x + c / b, Stage.sizeDelta.y);
        }


        private void set2EnvelopeScreenMode()
        {
            //获取Screen宽高比
            float c_whb = _canvasRectCache.x / _canvasRectCache.y;
            float s_whb = _stageSize.x / _stageSize.y;
            if (c_whb < s_whb)
            {
                //竖
                float b = _canvasRectCache.y / _stageSize.y;
                Stage.localScale = new Vector3(b, b, b);
            }
            else
            {
                //横(正)
                float b = _canvasRectCache.x / _stageSize.x;
                Stage.localScale = new Vector3(b, b, b);
            }
        }
        private void set2FitScreenMode()
        {
            //获取Screen宽高比
            float c_whb = _canvasRectCache.x / _canvasRectCache.y;
            float s_whb = _stageSize.x / _stageSize.y;
            if (c_whb > s_whb)
            {
                //竖
                float b = _canvasRectCache.y / _stageSize.y;
                Stage.localScale = new Vector3(b, b, b);
            }
            else
            {
                //横(正)
                float b = _canvasRectCache.x / _stageSize.x;
                Stage.localScale = new Vector3(b, b, b);
            }
        }
        private void set2DefaultMode()
        {
            Stage.pivot = new Vector2(.5f, .5f);
            Stage.anchorMin = new Vector2(.5f, .5f);
            Stage.anchorMax = new Vector2(.5f, .5f);
            Stage.sizeDelta = new Vector2(_stageSize.x, _stageSize.y);
        }
        private void set2WidthBaseModeBG()
        {
            float b = _canvasRectCache.x / _bgSize.x;
            _background.localScale = new Vector3(b, b, 1f);
            float c = _canvasRectCache.y - _bgSize.y * b;
            _background.sizeDelta = new Vector2(_background.sizeDelta.x, _bgSize.y + c / b);
        }
        private void set2HeightBaseModeBG()
        {
            float b = _canvasRectCache.y / _bgSize.y;
            _background.localScale = new Vector3(b, b, 1f);
            float c = _canvasRectCache.x - _bgSize.x * b;
            _background.sizeDelta = new Vector2(_bgSize.x + c / b, _background.sizeDelta.y);
        }
        private void set2EnvelopeScreenModeBG()
        {
            //获取Screen宽高比
            float c_whb = _canvasRectCache.x / _canvasRectCache.y;
            float s_whb = _bgSize.x / _bgSize.y;
            if (c_whb < s_whb)
            {
                //竖
                float b = _canvasRectCache.y / _bgSize.y;
                _background.localScale = new Vector3(b, b, 1f);
            }
            else
            {
                //横(正)
                float b = _canvasRectCache.x / _bgSize.x;
                _background.localScale = new Vector3(b, b, 1f);
            }
        }
        private void set2FitScreenModeBG()
        {
            //获取Screen宽高比
            float c_whb = _canvasRectCache.x / _canvasRectCache.y;
            float s_whb = _bgSize.x / _bgSize.y;
            if (c_whb > s_whb)
            {
                //竖
                float b = _canvasRectCache.y / _bgSize.y;
                _background.localScale = new Vector3(b, b, 1f);
            }
            else
            {
                //横(正)
                float b = _canvasRectCache.x / _bgSize.x;
                _background.localScale = new Vector3(b, b, 1f);
            }
        }

        private void set2DefaultModeBG()
        {
            Vector2 pi, aMin, aMax;
            switch (BgAlignType)
            {
                case BGAlignType.tLeft:
                    aMin = aMax = pi = new Vector2(0, 1f);
                    break;
                case BGAlignType.top:
                    aMin = aMax = pi = new Vector2(.5f, 1f);
                    break;
                case BGAlignType.tRight:
                    aMin = aMax = pi = new Vector2(1f, 1f);
                    break;
                case BGAlignType.left:
                    aMin = aMax = pi = new Vector2(0f, .5f);
                    break;
                case BGAlignType.right:
                    aMin = aMax = pi = new Vector2(1f, .5f);
                    break;
                case BGAlignType.bLeft:
                    aMin = aMax = pi = new Vector2(0, 0);
                    break;
                case BGAlignType.bottom:
                    aMin = aMax = pi = new Vector2(.5f, 0);
                    break;
                case BGAlignType.bRight:
                    aMin = aMax = pi = new Vector2(1f, 0);
                    break;
                default://center
                    aMin = aMax = pi = new Vector2(.5f, .5f);
                    break;
            }
            _background.pivot = pi;
            _background.anchorMin = aMin;
            _background.anchorMax = aMax;
            _background.sizeDelta = new Vector2(_bgSize.x, _bgSize.y);
        }





        private void changeScaleMode()
        {
            if (Stage == null)
            {

                GameObject st = transform.Find("AorUICanvas#/AorUIStage#").gameObject;
                if (st != null)
                {
                    _Stage = st.GetComponent<RectTransform>();
                }
                else
                {
                    Log.Warning("AorUIManager.changeScaleModeType() not complete : _stage == null.");
                    return;
                }
            }
            if (_scaleMode == ScaleModeType.noScale)
            {
                _isDirty = false;
                Stage.localScale = new Vector3(1f, 1f, 1f);
                set2DefaultMode();
            }
            else if (_scaleMode == ScaleModeType.normal)
            {
                _isDirty = false;
                Stage.anchorMin = Vector2.zero;
                Stage.anchorMax = new Vector2(1f, 1f);
                Stage.sizeDelta = new Vector2(0, 0);
                Stage.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                set2DefaultMode();
                updateStageSize();
                _isDirty = true;
            }
        }

        private void changeScaleModeBG()
        {
            if (_background == null)
            {

                GameObject bst = transform.Find("AorUICanvas#/AorUIBG#").gameObject;
                if (bst != null)
                {
                    _background = bst.GetComponent<RectTransform>();
                }
                else
                {
                    Log.Warning("AorUIManager.changeBGScaleModeType() not complete : _background == null.");
                    return;
                }
            }

            if (_bgScaleMode == ScaleModeType.noScale)
            {
                _isDirty = false;
                _background.localScale = new Vector3(1f, 1f, 1f);
                set2DefaultModeBG();
            }
            else if (_bgScaleMode == ScaleModeType.normal)
            {
                _isDirty = false;
                _bgAlignType = BGAlignType.center;
                _background.pivot = new Vector2(.5f, .5f);
                _background.anchorMin = Vector2.zero;
                _background.anchorMax = new Vector2(1f, 1f);
                _background.sizeDelta = new Vector2(0, 0);
                _background.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                set2DefaultModeBG();
                updateBGSize();
                _isDirty = true;
            }
        }

        private Dictionary<string, AorSwitchableUI> _UICacheList;
        /// <summary>
        /// 页面缓存池
        /// </summary>
        public Dictionary<string, AorSwitchableUI> UiCacheList
        {
            get { return _UICacheList; }
        }




        //绘制当前画面到某个RT上
        public void RenderToRT(RenderTexture renderTex)
        {
            if (UICamera == null)
                return;

            UICamera.targetTexture = renderTex;
            UICamera.Render();
            UICamera.targetTexture = null;
        }

        //======================================================================

        /// <summary>
        /// 根据layerID获取Layer
        /// </summary>
        /// <param name="layerId"></param>
        public RectTransform GetLayerById(int layerId)
        {
            if (layerId >= 0 && layerId < _Stage.childCount)
            {
                return _Stage.GetChild(layerId).GetComponent<RectTransform>();
            }
            else
            {
                Log.Error("AorUIManager.GetLayerById Error :: invalid layerID = " + layerId);
                return null;
            }
        }

        /// <summary>
        /// 根据layerName获取Layer
        /// </summary>
        public RectTransform GetLayerByName(string layerName)
        {
            Transform t = _Stage.Find("layerName");
            if (t != null)
            {
                return t.GetComponent<RectTransform>();
            }
            return null;
        }

        /// <summary>
        /// 根据layerID获取Layer的CanvasGroup
        /// </summary>
        /// <param name="layerId"></param>
        public CanvasGroup GetLayerCanvasGroupByName(string layerName)
        {
            Transform t = _Stage.Find("layerName");
            if (t != null)
            {
                return t.GetComponent<CanvasGroup>();
            }
            return null;
        }

        /// <summary>
        /// 根据layerID获取Layer的CanvasGroup
        /// </summary>
        /// <param name="layerId"></param>
        public CanvasGroup GetLayerCanvasGroupById(int layerId)
        {
            if (layerId >= 0 && layerId < _Stage.childCount)
            {
                return _Stage.GetChild(layerId).GetComponent<CanvasGroup>();
            }
            else
            {
                Log.Error("AorUIManager.GetLayerCanvasGroupById Error :: invalid layerID = " + layerId);
                return null;
            }
        }

        /// <summary>
        /// 添加一个层
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public int AddLayer(string layerName)
        {
            RectTransform rt = new GameObject(layerName).AddComponent<RectTransform>();
            CanvasGroup cg = rt.gameObject.AddComponent<CanvasGroup>();
            if (_canRaycast)
            {
                cg.ignoreParentGroups = true;
            }
            else
            {
                cg.ignoreParentGroups = false;
            }
            return rt.GetSiblingIndex();
        }

        /// <summary>
        /// 移除一个层
        /// </summary>
        /// <param name="layerId"></param>
        public void RemoveLayer(int layerId)
        {
            if (layerId >= 0 && layerId < _Stage.childCount)
            {
                Transform t = _Stage.GetChild(layerId);
                t.Dispose();
            }
            else
            {
                Log.Error("AorUIManager.RemoveLayer Error :: invalid layerID = " + layerId);
            }
        }
        /// <summary>
        /// 移除一个层
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(string layerName)
        {
            Transform t = _Stage.Find(layerName);
            if (t != null)
            {
                t.Dispose();
            }
            else
            {
                Log.Error("AorUIManager.RemoveLayer Error :: invalid layerName = " + layerName);
            }
        }

        private void PutAllGameObjectInPool(Transform t)
        {
            Transform[] dels = new Transform[t.childCount];
            int i, len = t.childCount;
            for (i = 0; i < len; i++)
            {
                dels[i] = t.GetChild(i);
            }
            for (i = 0; i < len; i++)
            {
                AorUIAssetLoader.PutPrefabInPool(dels[i].gameObject);
            }

        }

        public void ClearLayerChildren(int layerId)
        {
            if (layerId >= 0 && layerId < _Stage.childCount)
            {
                Transform t = _Stage.GetChild(layerId);
                if (t != null)
                {
                    PutAllGameObjectInPool(t);
                }
            }
            else
            {
                Log.Error("AorUIManager.ClearLayerChildren Error :: invalid layerID = " + layerId);
            }
        }

        public void ClearLayerChildren(string layerName)
        {
            Transform t = _Stage.Find(layerName);
            if (t != null)
            {
                PutAllGameObjectInPool(t);
            }
            else
            {
                Log.Error("AorUIManager.RemoveLayer Error :: invalid layerName = " + layerName);
            }
        }

        public void ClearLayerChildren(RectTransform layer)
        {

            if (layer != null)
            {
                PutAllGameObjectInPool(layer);
            }
            else
            {
                Log.Error("AorUIManager.RemoveLayer Error :: invalid layerName = " + layer);
            }
        }

        public void ClearFloatingLayer(bool isTopFloating = false)
        {
            if (isTopFloating)
            {
                PutAllGameObjectInPool(_FloatingLayerT.transform);
            }
            else
            {
                PutAllGameObjectInPool(_FloatingLayerB.transform);
            }
        }

        public void ClearSubFloatingLayer(RectTransform SubFloatingLayer, bool isTopFloating = false)
        {
            if (SubFloatingLayer != null)
            {
                if (SubFloatingLayer.parent == _FloatingLayerT || SubFloatingLayer.parent == _FloatingLayerB)
                {
                    PutAllGameObjectInPool(SubFloatingLayer.transform);
                }
            }
        }

        public void ClearSubFloatingLayer(int SubFloatingLayerId, bool isTopFloating = false)
        {
            Transform t = null;
            if (isTopFloating)
            {
                if (SubFloatingLayerId >= 0 && SubFloatingLayerId < _FloatingLayerT.childCount)
                {
                    t = _FloatingLayerT.GetChild(SubFloatingLayerId);
                }
            }
            else
            {
                if (SubFloatingLayerId >= 0 && SubFloatingLayerId < _FloatingLayerB.childCount)
                {
                    t = _FloatingLayerB.GetChild(SubFloatingLayerId);
                }
            }
            if (t != null)
            {
                PutAllGameObjectInPool(t);
            }
        }

        public void ClearSubFloatingLayer(string SubFloatingLayerName, bool isTopFloating = false)
        {
            Transform t;
            if (isTopFloating)
            {
                t = _FloatingLayerT.Find(SubFloatingLayerName);
            }
            else
            {
                t = _FloatingLayerB.Find(SubFloatingLayerName);
            }
            if (t != null)
            {
                PutAllGameObjectInPool(t);
            }
        }
        /// <summary>
        /// 获取对象在Stage中的位置
        /// </summary>
        /// <param name="target">RectTransform</param>
        /// <returns>Vector2</returns>
        public Vector2 GetStagePostion(RectTransform target)
        {
            return new Vector2(target.position.x / StageScale.x / transform.localScale.x, target.position.y / StageScale.y / transform.localScale.y);
        }
        /// <summary>
        /// 舞台Stage缩放值
        /// </summary>
        public Vector2 StageScale
        {
            get { return new Vector2(Stage.localScale.x, Stage.localScale.y); }
        }
        /// <summary>
        /// 在舞台中获取一个GameObject对象
        /// </summary>
        /// <param name="objectName">string 对象名称</param>
        /// <param name="layerID">int 层编号,默认情况MainLayer=0,TopLayer=1</param>
        /// <returns>GameObject</returns>
        public GameObject GetUIObject(string objectName, int layerID = 0)
        {
            if (layerID >= 0 && layerID < _Stage.childCount)
            {
                return _Stage.GetChild(layerID).Find(objectName).gameObject;
            }
            return null;
        }

        public GameObject GetUIObject(string objectName, string layerName)
        {
            Transform t = _Stage.Find(layerName);
            if (t != null)
            {
                return t.Find(objectName).gameObject;
            }
            return null;
        }

        /// <summary>
        /// 在舞台中获取一个GameObject对象的RectTransform
        /// </summary>
        /// <param name="objectName">string 对象名称</param>
        /// <param name="layerID">int 层编号,默认情况MainLayer=0,TopLayer=1</param>
        /// <returns>RectTransform</returns>
        public RectTransform GetRectTransform(string objectName, int layerID = 0)
        {
            return GetUIObject(objectName, layerID).GetComponent<RectTransform>();
        }



        //===================================================================== 默认的page管理方法组 ==============================================

        /// <summary>
        /// 将page组件加入到页面缓存池,(通常情况程序员不必调用此方法,此方法在AorPage初始化完成后会自行调用)
        /// </summary>
        /// <param name="page">AorPage AorPage对象</param>
        /// <returns>bool 是否成功加入到页面缓存池</returns>
        public void addPageINPool(AorSwitchableUI ui)
        {
            if (!_UICacheList.ContainsKey(ui.Path))
                _UICacheList.Add(ui.Path, ui);
            //<<<******特殊处理******>>>初始化显示_currentPage
            if (ui == _currentPage)
            {
                ui.Open(() =>
                {
                    SetCurrentPage(_currentPage.Path);
                });
            }
            else
            {
                //只有AorPage组件需要保持唯一显示.
                if (ui is AorPage)
                {
                    ui.gameObject.SetActive(false);
                }
            }

            //Log.Debug("AorUIManager.addPageINPool => " + ui.gameObject.name + " : " + ui.id + "  path = " + ui.Path);
            //  return true;

        }

        private bool _canRaycast = true;
        /// <summary>
        /// 是否可以交互
        /// </summary>
        public bool CanRaycast
        {
            get { return _canRaycast; }
            set
            {
                if (value != _canRaycast)
                {
                    _canRaycast = value;
                    int i, len = _Stage.childCount;
                    for (i = 0; i < len; i++)
                    {
                        Transform t = _Stage.GetChild(i);
                        CanvasGroup cg = t.GetComponent<CanvasGroup>();
                        if (cg != null)
                        {
                            if (_canRaycast)
                            {
                                cg.ignoreParentGroups = true;
                            }
                            else
                            {
                                cg.ignoreParentGroups = false;
                            }
                        }
                    }
                    if (_canRaycast)
                    {
                        _FloatingLayerB.GetComponent<CanvasGroup>().blocksRaycasts = true;
                        _FloatingLayerB.GetComponent<CanvasGroup>().interactable = true;
                    }
                    else
                    {
                        _FloatingLayerB.GetComponent<CanvasGroup>().blocksRaycasts = false;
                        _FloatingLayerB.GetComponent<CanvasGroup>().interactable = false;
                    }
                    if (_canRaycast)
                    {
                        _FloatingLayerT.GetComponent<CanvasGroup>().blocksRaycasts = true;
                        _FloatingLayerT.GetComponent<CanvasGroup>().interactable = true;
                    }
                    else
                    {
                        _FloatingLayerT.GetComponent<CanvasGroup>().blocksRaycasts = false;
                        _FloatingLayerT.GetComponent<CanvasGroup>().interactable = false;
                    }

                }
            }
        }

        /// <summary>
        /// 当前显示的AorPage对象
        /// </summary>
        [SerializeField]//此序列化只用于编辑器模式下查看数据使用
        private AorPage _currentPage = null;
        /// <summary>
        /// 当前显示的Page对象
        /// </summary>
        public AorPage CurrentPage
        {
            get { return _currentPage; }
        }

        /// <summary>
        /// 将AorPage对象设置为当前显示的页面.(通过PageTurning方法进项页面转换时无需程序员调用此方法)
        /// </summary>
        /// <param name="path">AorPage.path</param>
        public void SetCurrentPage(string path)
        {

            if (_UICacheList.ContainsKey(path))
            {
                AorPage ap = _UICacheList[path] as AorPage;
                if (ap == null)
                    return;

                _currentPage = ap;
                List<string> delKeys = new List<string>();
                foreach (string s in _UICacheList.Keys)
                {
                    if (s != path)
                    {
                        if (!(_UICacheList[s] is AorPage))
                        {
                            continue;
                        }
                        else
                        {

                            if (_UICacheList[s].isStatic)
                            {
                                _UICacheList[s].gameObject.SetActive(false);
                            }
                            else
                            {
                                delKeys.Add(s);
                            }
                        }


                    }
                }

                if (delKeys.Count > 0)
                {
                    int i, length = delKeys.Count;
                    for (i = 0; i < length; i++)
                    {
                        var del = _UICacheList[delKeys[i]];
                        _UICacheList.Remove(delKeys[i]);
                        //Debug.Log("Destroy :: " + del.gameObject.name);
                        GameObject.Destroy(del.gameObject);
                    }
                    delKeys.Clear();
                }
                delKeys = null;
            }
            else
            {
                Log.Warning("AorUIManager.SetCurrentPage fail : can not find [xId = " + path + "] in _pageList");
            }
        }
        /*
        private void cleanPagePool(string path) {
            int i, length = _UICacheList.Keys.Count;
            string k;
            for (i = 0; i < length; i++) {
                k = _UICacheList.Keys[i];
            }
        }*/
        /// <summary>
        /// 通过id获取xid(AorPage.xid属性,作为AorPage的)
        /// </summary>
        /// <param name="id">stirng id</param>
        /// <returns>string path</returns>
        public string getXidFromID(string id)
        {
            foreach (var ap in _UICacheList.Values)
            {
                if (ap.id == id)
                {
                    return ap.Path;
                }
            }
            return null;
        }

        //------------------------------- 独立模式API --------------------------

        /// <summary>
        /// 通过id从缓存池获取一个AorPage对象
        /// </summary>
        /// <param name="id">string id</param>
        /// <returns>AorPage</returns>
        public UprefabUnitBase getPageFromID(string id)
        {
            foreach (var ap in _UICacheList.Values)
            {
                if (ap.id == id)
                {
                    return ap;
                }
            }
            return null;
        }

        private bool _isPageTurning;
        /// <summary>
        /// 切换到新的页面
        /// </summary>
        /// <param name="instancedNewPageGo">GameObject实例</param>
        /// <param name="layerId">int 层编号,默认情况MainLayer=0,TopLayer=1</param>
        /// <param name="newPageId">AorPage.id,如不传入初始化的AorPage.id则等于此GameObject.name</param>
        public void PageTurning(GameObject instancedNewPageGo, int layerId = 0, string newPageId = "NOSET")
        {

            instancedNewPageGo.transform.SetParent(Stage.GetChild(layerId), false);

            AorPage np = instancedNewPageGo.GetComponent<AorPage>();

            //if (np.id == null || np.id.Trim() == "") {
            if (newPageId == "NOSET")
            {
                np.id = instancedNewPageGo.name;
            }
            else
            {
                np.id = newPageId;
            }
            //}

            np.onEventInit = () =>
            {
                /*
                if (_currentPage == null) {
                    SetCurrentPage(np.xId);
                }*/
                PageTurning(np.id);
            };

        }

        /// <summary>
        /// 切换到新的页面
        /// 注意:页面切换必须是新旧页面都已经加入到缓存池中了,否则会出错.
        /// </summary>
        /// <param name="newPageId">stirng 新页面的id</param>
        public void PageTurning(string newPageId)
        {
            if (_isPageTurning) { return; }
            _isPageTurning = true;

            AorPage ap = getPageFromID(newPageId) as AorPage;
            if (ap != null)
            {

                //----------------------------------------------------------------------
                AorUIAnimator at = ap.gameObject.GetComponent<AorUIAnimator>();
                // Debug.Log("**** " + ct.FadeOUT + " | " + at.FadeIN);
                if (at != null)
                {

                    //检查旧页面是否给新页面传递了延迟时间
                    if (_currentPage != null)
                    {
                        AorUIAnimator ct = _currentPage.gameObject.GetComponent<AorUIAnimator>();
                        if (ct != null)
                        {
                            if (ct.PageTurningInterval > 0)
                            {
                                at.fadeInDelayTime = ct.PageTurningInterval;
                            }
                        }
                    }

                    if (at.FadeIN != AorUIAnimationType.none)
                    {
                        //at有动画
                        ap.gameObject.SetActive(true);
                        ap.transform.SetAsLastSibling();
                        at.fadeIN(() =>
                        {
                            //Debug.Log("||----------------- 新页面FadeIN" + ap.name);
                            SetCurrentPage(ap.Path);
                            _isPageTurning = false;
                        });
                        if (_currentPage != null)
                        {
                            AorUIAnimator ct = _currentPage.gameObject.GetComponent<AorUIAnimator>();
                            if (ct != null)
                            {
                                //_currentPage有动画
                                if (ct.FadeOUT != AorUIAnimationType.none)
                                {
                                    // Debug.Log("||----------------- 旧页面FadeOUT" + ct.name);
                                    ct.fadeOUT();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        //at无动画
                        if (_currentPage != null)
                        {
                            AorUIAnimator ct = _currentPage.gameObject.GetComponent<AorUIAnimator>();
                            if (ct != null)
                            {
                                //_currentPage有动画模块
                                //特殊处理 新页面没有动画而旧页面有动画的情况.
                                ap.gameObject.SetActive(true);
                                at.fadeIN();
                                //Debug.Log("||----------------- 新页面无动画" + ap.name);

                                if (ct.FadeOUT != AorUIAnimationType.none)
                                {
                                    ct.fadeOUT(() =>
                                    {
                                        //Debug.Log("||----------------- 旧页面FadeOUT" + ct.name);
                                        ap.transform.SetAsLastSibling();
                                        SetCurrentPage(ap.Path);
                                        _isPageTurning = false;
                                    });
                                    return;
                                }
                                else
                                {
                                    ap.transform.SetAsLastSibling();
                                    SetCurrentPage(ap.Path);
                                    _isPageTurning = false;
                                }

                            }
                            else
                            {
                                //旧无动画模块
                                ap.gameObject.SetActive(true);
                                ap.transform.SetAsLastSibling();
                                SetCurrentPage(ap.Path);
                                _isPageTurning = false;

                            }
                        }
                        else
                        {
                            ap.gameObject.SetActive(true);
                            at.fadeIN();
                            ap.transform.SetAsLastSibling();
                            SetCurrentPage(ap.Path);
                            _isPageTurning = false;
                        }
                    }
                }
                else
                {
                    //at没有动画模块
                    ap.gameObject.SetActive(true);
                    ap.transform.SetAsLastSibling();
                    SetCurrentPage(ap.Path);
                    _isPageTurning = false;
                }

                //test code
                /*
                ap.open(() => {
                    SetCurrentPage(ap.xId);
                });
                _currentPage.close();
                */

                //------------------------------------------------------------------------------------------------

            }
            else
            {
                Log.Warning("AorUIManager.PageTurning fail : getPageFromID fail : newPageID = " + newPageId);
                _isPageTurning = false;
            }
        }

        //------------------------------- 独立模式API -------------------------- End

        //====================================================================== 默认的page管理方法组 ========================================== end
        [SerializeField]
        private IAorUILogicInterface _AorUIInterfaceLogic;

        /// <summary>
        /// UI全局逻辑类实例的引用
        /// </summary>
        public IAorUILogicInterface AorUIInterfaceLogic
        {
            get { return _AorUIInterfaceLogic; }
            set { _AorUIInterfaceLogic = value; }
        }

        //==================================================================== floatingLayer管理方法组 ==============================================
        /// <summary>
        /// 在浮动层上添加浮动UI对象
        /// </summary>
        /// <param name="posTarget">跟随位置</param>
        /// <param name="displayUIObject">显示的UI对象</param>
        /// <param name="workCamera">世界工作摄像机</param>
        /// <param name="UseScaleByZDistance">是否启用根据Z距离缩放UI对象</param>
        /// <param name="ItemScaleDef">根据Z距离缩放UI对象时的缩放参考值</param>
        /// <param name="isTopFloating">是否防止浮动UI对象在最顶层浮动层,默认为flase即放置在底层浮动层</param>
        /// <returns>FloatingItemHandler:浮动UI对象控制类</returns>
        public FloatingItemHandler AddFloatingUiItem(Transform posTarget, RectTransform displayUIObject, Camera workCamera, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, bool isTopFloating = false)
        {
            return AddFloatingUiItem(posTarget, displayUIObject, workCamera, Vector2.zero, Vector3.zero, UseScaleByZDistance,
                ItemScaleDef, isTopFloating);
        }
        /// <summary>
        /// 在浮动层上添加浮动UI对象
        /// </summary>
        /// <param name="posTarget">跟随位置</param>
        /// <param name="displayUIObject">显示的UI对象</param>
        /// <param name="workCamera">世界工作摄像机</param>
        /// <param name="offset">偏移值</param>
        /// <param name="UseScaleByZDistance">是否启用根据Z距离缩放UI对象</param>
        /// <param name="ItemScaleDef">根据Z距离缩放UI对象时的缩放参考值</param>
        /// <param name="isTopFloating">是否防止浮动UI对象在最顶层浮动层,默认为flase即放置在底层浮动层</param>
        /// <returns>FloatingItemHandler:浮动UI对象控制类</returns>
        public FloatingItemHandler AddFloatingUiItem(Transform posTarget, RectTransform displayUIObject, Camera workCamera, Vector2 offset, Vector2 world_offset, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, bool isTopFloating = false, ScaleModeType _ScaleType = ScaleModeType.widthBase)
        {

            if (isTopFloating)
            {
                displayUIObject.SetParent(FloatingLayerT, false);
            }
            else
            {

                displayUIObject.SetParent(FloatUIBackLayer, false);
            }

            var fih = displayUIObject.GetComponent<FloatingItemHandler>();
            if (fih != null)
            {
                fih.FloatingTarget = posTarget;
                fih.SrceenOffset = offset;
                fih.WorkCamera = workCamera;
                fih.UseScaleByZDistance = UseScaleByZDistance;
                fih.ItemScaleDef = ItemScaleDef;
            }
            else
            {
                fih = FloatingItemHandler.createFloatingItemHandler(this, posTarget, displayUIObject,
                    workCamera, offset, world_offset, UseScaleByZDistance, ItemScaleDef, _ScaleType);
            }

            return fih;
        }
        public FloatingItemHandler AddFloatingUiItem(Transform posTarget, RectTransform displayUIObject, RectTransform SubFloatingLayer, Camera workCamera, Vector2 offset, Vector3 world_offset, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, bool isTopFloating = false)
        {

            if (SubFloatingLayer != null)
            {

                if (
                    SubFloatingLayer.parent == null
                    || SubFloatingLayer.parent != FloatingLayerB.transform
                    || SubFloatingLayer.parent != FloatingLayerT.transform
                    )
                {
                    AorUiRuntimeUtility.ResetRectTransform(SubFloatingLayer);
                    displayUIObject.SetParent(SubFloatingLayer, false);
                    if (isTopFloating)
                    {
                        SubFloatingLayer.SetParent(FloatingLayerT, false);
                    }
                    else
                    {

                        SubFloatingLayer.SetParent(FloatUIBackLayer, false);
                    }

                }
            }
            else
            {
                if (isTopFloating)
                {
                    displayUIObject.SetParent(FloatingLayerT, false);
                }
                else
                {

                    displayUIObject.SetParent(FloatUIBackLayer, false);
                }
            }
            var fih = displayUIObject.GetComponent<FloatingItemHandler>();
            if (fih != null)
            {
                fih.FloatingTarget = posTarget;
                fih.SrceenOffset = offset;
                fih.WorkCamera = workCamera;
                fih.UseScaleByZDistance = UseScaleByZDistance;
                fih.ItemScaleDef = ItemScaleDef;
            }
            else
            {
                fih = FloatingItemHandler.createFloatingItemHandler(this, posTarget, displayUIObject,
                    workCamera, offset, world_offset, UseScaleByZDistance, ItemScaleDef);
            }

            return fih;
        }


        public FloatingItemHandler AddFuncMenuUiItem(Transform posTarget, RectTransform displayUIObject, Camera workCamera, Vector2 offset, Vector2 world_offset, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, bool isTopFloating = false, ScaleModeType _ScaleType = ScaleModeType.widthBase)
        {

            if (isTopFloating)
            {
                displayUIObject.SetParent(FloatingLayerT, false);
            }
            else
            {

                displayUIObject.SetParent(UIBackLayer, false);
            }

            var fih = displayUIObject.GetComponent<FloatingItemHandler>();
            if (fih != null)
            {
                fih.FloatingTarget = posTarget;
                fih.SrceenOffset = offset;
                fih.WorkCamera = workCamera;
                fih.UseScaleByZDistance = UseScaleByZDistance;
                fih.ItemScaleDef = ItemScaleDef;
            }
            else
            {
                fih = FloatingItemHandler.createFloatingItemHandler(this, posTarget, displayUIObject,
                    workCamera, offset, world_offset, UseScaleByZDistance, ItemScaleDef, _ScaleType);
            }

            return fih;
        }

        //

        public FloatingItemHandler AddFloatingUiItem(Transform posTarget, RectTransform displayUIObject, string SubFloatingLayerName, Camera workCamera, Vector2 offset, Vector3 world_offset, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, bool isTopFloating = false)
        {
            string sflName = SubFloatingLayerName;
            if (string.IsNullOrEmpty(sflName))
            {
                sflName = "UNameSubFloatingLayer";
            }

            RectTransform rt = AddSubFloatingLayer(SubFloatingLayerName);

            return AddFloatingUiItem(posTarget, displayUIObject, rt, workCamera, offset, world_offset);

        }

        public RectTransform AddSubFloatingLayer(string LayerName, bool isTopFloating = false)
        {
            Transform t;
            RectTransform rt;
            if (isTopFloating)
            {
                t = _FloatingLayerT.Find(LayerName);
                if (t == null)
                {
                    rt = new GameObject(LayerName).AddComponent<RectTransform>();
                    //rt.gameObject.AddComponent<CanvasGroup>();
                    rt.SetParent(_FloatingLayerT, false);
                }
                else
                {
                    rt = t.GetComponent<RectTransform>();
                }
            }
            else
            {
                t = _FloatingLayerB.Find(LayerName);
                if (t == null)
                {
                    rt = new GameObject(LayerName).AddComponent<RectTransform>();
                    //srt.gameObject.AddComponent<CanvasGroup>();
                    rt.SetParent(_FloatingLayerB, false);
                }
                else
                {
                    rt = t.GetComponent<RectTransform>();
                }
            }
            return rt;
        }
        public RectTransform AddSubFloatingLayerToUIBackLayer(string LayerName)
        {
            if (UIBackLayer == null)
                return null;
            Transform t;
            RectTransform rt;
            t = UIBackLayer.Find(LayerName);
            if (t == null)
            {
                rt = new GameObject(LayerName).AddComponent<RectTransform>();
                //srt.gameObject.AddComponent<CanvasGroup>();
                rt.SetParent(FloatUIBackLayer, false);
            }
            else
            {
                rt = t.GetComponent<RectTransform>();
            }
            return rt;
        }
        public RectTransform GetSubFloatingLayerFromUIBackLayer(string LayerName)
        {
            if (UIBackLayer == null)
                return null;
            Transform t = UIBackLayer.Find(LayerName);
            if (t != null)
            {
                return t.GetComponent<RectTransform>();
            }
            return null;
        }

        public RectTransform GetSubFloatingLayer(string LayerName, bool isTopFloating = false)
        {
            Transform t;
            if (isTopFloating)
            {
                t = _FloatingLayerT.Find(LayerName);
            }
            else
            {
                t = _FloatingLayerB.Find(LayerName);
            }
            if (t != null)
            {
                return t.GetComponent<RectTransform>();
            }
            return null;
        }

        public RectTransform GetSubFloatingLayer(int LayerId, bool isTopFloating = false)
        {
            Transform t = null;
            if (isTopFloating)
            {
                if (LayerId >= 0 && LayerId < _FloatingLayerT.childCount)
                {
                    t = _FloatingLayerT.GetChild(LayerId);
                }
            }
            else
            {
                if (LayerId >= 0 && LayerId < _FloatingLayerB.childCount)
                {
                    t = _FloatingLayerB.GetChild(LayerId);
                }
            }
            if (t != null)
            {
                return t.GetComponent<RectTransform>();
            }
            return null;
        }

        public void RemoveSubFloatingLayer(string LayerName, bool isTopFloating = false)
        {
            Transform t;
            if (isTopFloating)
            {
                t = _FloatingLayerT.Find(LayerName);
            }
            else
            {
                t = _FloatingLayerB.Find(LayerName);
            }
            if (t != null)
            {
                t.Dispose();
            }
        }

        public void RemoveSubFloatingLayer(int LayerId, bool isTopFloating = false)
        {
            Transform t = null;
            if (isTopFloating)
            {
                if (LayerId >= 0 && LayerId < _FloatingLayerT.childCount)
                {
                    t = _FloatingLayerT.GetChild(LayerId);
                }
            }
            else
            {
                if (LayerId >= 0 && LayerId < _FloatingLayerB.childCount)
                {
                    t = _FloatingLayerB.GetChild(LayerId);
                }
            }
            if (t != null)
            {
                t.Dispose();
            }
        }

        /// <summary>
        /// 添加简单的floating，自己设置位置(废弃)
        /// 
        /// 如有此需求,请使用原生代码代替,例:
        ///         displayUIObject.SetParent(FloatingLayerT, false);
        /// 
        /// </summary>
        [Obsolete]
        public void AddToFloatingLayer(RectTransform displayUIObject, bool isTopFloating = false)
        {

            if (isTopFloating)
            {
                displayUIObject.SetParent(FloatingLayerT, false);
            }
            else
            {

                displayUIObject.SetParent(FloatUIBackLayer, false);
            }
        }
        [Obsolete]
        public void AddToFloatingLayer(RectTransform displayUIObject, RectTransform SubFloatingLayer, bool isTopFloating = false)
        {

            if (SubFloatingLayer != null)
            {
                AorUiRuntimeUtility.ResetRectTransform(SubFloatingLayer);
                displayUIObject.SetParent(SubFloatingLayer, false);
                if (isTopFloating)
                {
                    SubFloatingLayer.SetParent(FloatingLayerT, false);
                }
                else
                {

                    SubFloatingLayer.SetParent(FloatUIBackLayer, false);
                }
            }
            else
            {
                if (isTopFloating)
                {
                    displayUIObject.SetParent(FloatingLayerT, false);
                }
                else
                {

                    displayUIObject.SetParent(FloatUIBackLayer, false);
                }
            }
        }
        //==================================================================== floatingLayer管理方法组 ========================================== end

        //=====================================================================  Background 方法组  ================================================
        [SerializeField]
        private ScaleModeType _bgScaleMode = ScaleModeType.envelopeScreen;
        public ScaleModeType BGScaleMode
        {
            get { return _bgScaleMode; }
            set
            {
                _bgScaleMode = value;
                //if (_isStartd) {
                changeScaleModeBG();
                //}
            }
        }
        [SerializeField]
        private BGAlignType _bgAlignType = BGAlignType.center;
        public BGAlignType BgAlignType
        {
            get { return _bgAlignType; }
            set
            {
                _bgAlignType = value;
                //if (_isStartd) {
                changeScaleModeBG();
                //}
            }
        }

        [SerializeField]//此序列化只用于编辑器模式下查看数据使用
        private Vector2 _bgSize = new Vector2(960f, 640f);
        /// <summary>
        /// 背景的设计尺寸
        /// </summary>
        public Vector2 BackgroundSize
        {
            get { return _bgSize; }
            set
            {
                _bgSize = value;
                //if (_isStartd) {
                changeScaleModeBG();
                //}
            }
        }

        private RectTransform _renderTexture;

        private RectTransform _background;
        /// <summary>
        /// 背景层RectTransform实例引用
        /// </summary>

        public RectTransform background
        {
            get { return _background; }
        }

        [SerializeField] //此序列化只用于编辑器模式下查看数据使用
        private bool _isHideBG = true;
        /// <summary>
        /// 是否隐藏背景层(AorUI自带背景对象,通常在3D游戏中是需要将其隐藏,而在过场动画,loading界面等希望遮挡3D层时应将其显示)
        /// </summary>
        public bool isHideBG
        {
            get { return _isHideBG; }
            set
            {
                _isHideBG = value;
                if (_background != null)
                {
                    if (_isHideBG)
                    {
                        _background.gameObject.SetActive(false);
                    }
                    else
                    {
                        _background.gameObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// 获取 背景层的Image组件
        /// </summary>
        /// <returns></returns>
        public RawImage GetBackgroundImage()
        {
            if (background != null)
            {
                return background.GetComponent<RawImage>();
            }
            return null;
        }

        public AorSwitchableUI isUiExist(string path)
        {
            if (_UICacheList.ContainsKey(path) && _UICacheList[path] != null && _UICacheList[path].gameObject != null && _UICacheList[path].gameObject.activeSelf)
            {
                return _UICacheList[path];
            }

            return null;
        }

        public void GetUI(string path, CallBack<object> loadDone)
        {
            //静态关闭的窗口重新激活
            if (_UICacheList.ContainsKey(path) && _UICacheList[path] != null && _UICacheList[path].isStatic && _UICacheList[path].gameObject != null && !_UICacheList[path].gameObject.activeSelf)
            {
                _UICacheList[path].gameObject.SetActive(true);
                loadDone(_UICacheList[path]);
                return;
            }

            //新创建
            AorUIAssetLoader.LoadPrefabFromPool(path, (obj) =>
            {
                AorSwitchableUI ap = obj.GetComponent<AorSwitchableUI>();
                if (ap != null)
                {
                    //                    ap.Path = path;
                    loadDone(ap);
                }
                else
                    loadDone(obj);
            });
        }


        public void CloseUI(AorSwitchableUI ui)
        {
            if (_UICacheList != null && !ui.isStatic)
                _UICacheList.Remove(ui.Path);
        }


        /// <summary>
        /// 淡入背景
        /// </summary>
        /// <param name="duration">持续时间(秒)</param>
        public void BGFadeIn(float duration = 0.5f)
        {
            if (background != null)
            {

                RawImage cg = background.GetComponent<RawImage>();
                if (cg != null)
                {

                    if (duration <= 0)
                    {
                        cg.color = Color.white;
                        isHideBG = false;
                        return;
                    }
                    else
                    {
                        cg.color = new Color(0, 0, 0, 1);
                        isHideBG = false;
                        cg.DOColor(Color.white, duration);
                    }


                }
            }
        }

        /// <summary>
        /// 淡出背景
        /// </summary>
        /// <param name="duration">持续时间(秒)</param>
        public void BGFadeOut(float duration = 0.5f)
        {
            if (background != null)
            {
                RawImage cg = background.GetComponent<RawImage>();
                if (cg != null)
                {

                    if (duration <= 0)
                    {
                        cg.color = new Color(0, 0, 0, 1);
                        isHideBG = true;
                    }
                    else
                    {

                        cg.DOColor(new Color(0, 0, 0, 1), duration).OnComplete(() =>
                        {
                            isHideBG = true;
                        });

                    }

                }
            }
        }


        //=====================================================================  Background 方法组  ============================================ end


        /// <summary>
        /// 淡入背景
        /// </summary>
        /// <param name="duration">持续时间(秒)</param>
        public void RTFadeIn(float duration = 0.5f)
        {
            if (_renderTexture != null)
            {
                _renderTexture.gameObject.SetActive(true);
                RawImage cg = _renderTexture.GetComponent<RawImage>();
                if (cg != null)
                {

                    if (duration <= 0)
                    {
                        cg.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
                        cg.color = new Color(1, 1, 1, 0);
                        cg.DOColor(new Color(1, 1, 1, 1), duration);
                    }


                }
            }
        }

        /// <summary>
        /// 淡出背景
        /// </summary>
        /// <param name="duration">持续时间(秒)</param>
        public void RTFadeOut(float duration = 0.5f)
        {
            if (_renderTexture != null)
            {
                RawImage cg = _renderTexture.GetComponent<RawImage>();
                if (cg != null)
                {

                    if (duration <= 0)
                    {
                        cg.color = new Color(1, 1, 1, 0);
                        cg.gameObject.SetActive(false);
                    }
                    else
                    {

                        cg.DOColor(new Color(1, 1, 1, 0), duration).OnComplete(() =>
                        {
                            cg.gameObject.SetActive(false);
                        });

                    }

                }
            }
        }

        //=================================================================== AorUIManager 事件 =========================================

        [Serializable]
        public class AorSwitchableUIEvent : UnityEvent<AorSwitchableUI> { }

        [SerializeField]
        private AorSwitchableUIEvent m_onAorSwitchableUIInited = new AorSwitchableUIEvent();
        public AorSwitchableUIEvent onAorSwitchableUIInited
        {
            get { return m_onAorSwitchableUIInited; }
            set { m_onAorSwitchableUIInited = value; }
        }

        [SerializeField]
        private AorSwitchableUIEvent m_onAorSwitchableUIOpen = new AorSwitchableUIEvent();
        public AorSwitchableUIEvent onAorSwitchableUIOpen
        {
            get { return m_onAorSwitchableUIOpen; }
            set { m_onAorSwitchableUIOpen = value; }
        }

        [SerializeField]
        private AorSwitchableUIEvent m_onAorSwitchableUIClose = new AorSwitchableUIEvent();
        public AorSwitchableUIEvent onAorSwitchableUIClose
        {
            get { return m_onAorSwitchableUIClose; }
            set { m_onAorSwitchableUIClose = value; }
        }

    }
}