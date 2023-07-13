using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FrameWork.GUI.AorUI.Components
{
    public class FloatingItemHandler : MonoBehaviour
    {

        public static bool GlobalUpdateToggle = true;

        public static FloatingItemHandler createFloatingItemHandler(AorUIManager uiManager, Transform floatingTarget, RectTransform displayUITarget, Camera workCamera, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, ScaleModeType _ScaleType = ScaleModeType.widthBase)
        {
            return createFloatingItemHandler(uiManager, floatingTarget, displayUITarget, workCamera, Vector2.zero, Vector3.zero, UseScaleByZDistance, ItemScaleDef, _ScaleType);
        }

        public static FloatingItemHandler createFloatingItemHandler(AorUIManager uiManager, Transform floatingTarget, RectTransform displayUITarget, Camera workCamera, Vector2 srceenOffset, Vector3 worldOffset, bool UseScaleByZDistance = false, float ItemScaleDef = 100f, ScaleModeType _ScaleType = ScaleModeType.widthBase)
        {
            FloatingItemHandler fih = displayUITarget.gameObject.GetComponent<FloatingItemHandler>();
            if( null == fih)
            {
                fih = displayUITarget.gameObject.AddComponent<FloatingItemHandler>();
            }
            fih.UiManager = uiManager;
            fih.FloatingTarget = floatingTarget;
            fih.SrceenOffset = srceenOffset;
            fih.WorldOffset = worldOffset;
            fih.WorkCamera = workCamera;
            fih.UseScaleByZDistance = UseScaleByZDistance;
            fih.ItemScaleDef = ItemScaleDef;
            if (_ScaleType == ScaleModeType.widthBase)
                fih.UIScale = uiManager.StageScale;
            else if (_ScaleType == ScaleModeType.heightBase)
                fih.UIScale = uiManager.GetHeightBaseMode();
            return fih;
        }

        public AorUIManager UiManager;
        private Transform _FloatingTarget;
        public Transform FloatingTarget
        {
            set
            {
                _FloatingTarget = value;
                itemLastPosition = Vector3.zero;
            }

            get { return _FloatingTarget; }
        }
        public Vector3 FloatingPos;
        public Vector2 SrceenOffset;
        public Vector3 WorldOffset;
        public Camera WorkCamera;
        public CanvasGroup _CanvasGroup;

        public float DistanceLimit = 200f;

        public bool UpdateToggle = true;
        public bool UseScaleByZDistance = false;
        public float ItemScaleDef = 100f;
        public Vector2 UIScale;
        public object param;
        private Vector3 cameraLastPosition;
        private Vector3 cameraLastRotaion;
        private Vector3 itemLastPosition;
        public Vector3 ItemLastPosition
        {
            get
            {
                return itemLastPosition;
            }
            set
            {
                 itemLastPosition=value;
            }
        }
        private float cameraLastFOV;

        [HideInInspector, NonSerialized]
        public RectTransform _rect;
        [HideInInspector, NonSerialized]
        public bool IsInit = false;

        private void Awake()
        {
            _CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_CanvasGroup == null)
            {
                _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

        }

        private void Start()
        {
            AheadInit();
         
        }

        public void AheadInit()
        {
            if (IsInit == false)
            {
                _rect = gameObject.GetComponent<RectTransform>();
             

                if ((FloatingTarget != null && WorkCamera != null)||FloatingPos!=Vector3.zero)
                {
                    setPostionOnScreen();
                }
                IsInit = true;
            }
        }
        void OnEnable()
        {
            if (IsInit && ((FloatingTarget != null && WorkCamera != null) || FloatingPos != Vector3.zero))
            {

                setPostionOnScreen();
            }


        }

        public void setPostionOnScreen()
        {

            if (!GlobalUpdateToggle || !UpdateToggle ||
                WorkCamera == null || null == _CanvasGroup)
            {
                return;
            }
            if (FloatingPos != Vector3.zero)
            {


                if (cameraLastPosition == WorkCamera.transform.position &&
                    cameraLastRotaion == WorkCamera.transform.eulerAngles &&
                    itemLastPosition == FloatingPos &&
                    cameraLastFOV == WorkCamera.fieldOfView)
                    return;
                cameraLastPosition = WorkCamera.transform.position;
                cameraLastRotaion = WorkCamera.transform.eulerAngles;
                itemLastPosition = FloatingPos;
                cameraLastFOV = WorkCamera.fieldOfView;

                Vector3 posDir1 = (cameraLastPosition - itemLastPosition).normalized;
                float dis1 = Vector3.Distance(cameraLastPosition, itemLastPosition);
                if (posDir1 != Vector3.zero && Vector3.Dot(WorkCamera.transform.forward, posDir1) < 0 && dis1 < DistanceLimit)
                {
                    Vector3 ScreenPos = WorkCamera.WorldToScreenPoint(itemLastPosition + WorldOffset);
                    if (UseScaleByZDistance)
                    {
                        float sd = ItemScaleDef / Vector3.Distance(cameraLastPosition, FloatingPos);
                        _rect.localScale = UIScale * sd;
                        _rect.anchoredPosition = new Vector2(ScreenPos.x + SrceenOffset.x * UIScale.x * sd,
                            ScreenPos.y + SrceenOffset.y * UIScale.y * sd);
                    }
                    else
                    {
                        _rect.localScale = UIScale;
                        _rect.anchoredPosition = new Vector2(ScreenPos.x + SrceenOffset.x * UIScale.x,
                            ScreenPos.y + SrceenOffset.y * UIScale.y);
                    }
                    //_CanvasGroup.alpha = 1;
                    //_CanvasGroup.interactable = true;
                    //_CanvasGroup.blocksRaycasts = true;

                    //
                    if (m_onVisible != null)
                    {
                        m_onVisible.Invoke(this);
                    }

                }
                else
                {
                    //_CanvasGroup.alpha = 0;
                    //_CanvasGroup.interactable = false;
                    //_CanvasGroup.blocksRaycasts = false;

                    //
                    if (m_onInvisible != null)
                    {
                        m_onInvisible.Invoke(this);
                    }

                }

                return;
            }
            if (FloatingTarget == null)
                return;
            if (cameraLastPosition == WorkCamera.transform.position &&
            cameraLastRotaion == WorkCamera.transform.eulerAngles &&
        itemLastPosition == FloatingTarget.position &&
        cameraLastFOV == WorkCamera.fieldOfView)
                return;
            cameraLastPosition = WorkCamera.transform.position;
            cameraLastRotaion = WorkCamera.transform.eulerAngles;
            itemLastPosition = FloatingTarget.position;
            cameraLastFOV = WorkCamera.fieldOfView;

            Vector3 posDir = (cameraLastPosition - itemLastPosition).normalized;
            float dis = Vector3.Distance(cameraLastPosition, itemLastPosition);
            if (posDir != Vector3.zero && Vector3.Dot(WorkCamera.transform.forward, posDir) < 0 && dis < DistanceLimit)
            {
                Vector3 ScreenPos = WorkCamera.WorldToScreenPoint(itemLastPosition + WorldOffset);
                if (UseScaleByZDistance)
                {
                    float sd = ItemScaleDef / Vector3.Distance(cameraLastPosition, FloatingTarget.transform.position);
                    _rect.localScale = UIScale * sd;
                    _rect.anchoredPosition = new Vector2(ScreenPos.x + SrceenOffset.x * UIScale.x * sd,
                        ScreenPos.y + SrceenOffset.y * UIScale.y * sd);
                }
                else
                {
                    _rect.localScale = UIScale;
                    _rect.anchoredPosition = new Vector2(ScreenPos.x + SrceenOffset.x * UIScale.x,
                        ScreenPos.y + SrceenOffset.y * UIScale.y);
                }
                //_CanvasGroup.alpha = 1;
                //_CanvasGroup.interactable = true;
                //_CanvasGroup.blocksRaycasts = true;

                //
                if (m_onVisible != null)
                {
                    m_onVisible.Invoke(this);
                }

            }
            else
            {
                //_CanvasGroup.alpha = 0;
                //_CanvasGroup.interactable = false;
                //_CanvasGroup.blocksRaycasts = false;

                //
                if (m_onInvisible != null)
                {
                    m_onInvisible.Invoke(this);
                }

            }
        }



 

        private void LateUpdate()
        {
            if ((FloatingPos!=Vector3.zero||FloatingTarget != null )&& WorkCamera != null && GlobalUpdateToggle && UpdateToggle)
            {
                setPostionOnScreen();
            }
            if (_rect.localScale != Vector3.zero&&((FloatingPos ==Vector3.zero&& FloatingTarget == null)||ItemScaleDef==0))
            {
                _rect.localScale = Vector3.zero;
            }
        }


 

        //---------------------------------------

        [Serializable]
        public class FloatItemVisibleEvent : UnityEvent<FloatingItemHandler> { }
        [Serializable]
        public class FloatItemInvisibleEvent : UnityEvent<FloatingItemHandler> { }


        [SerializeField]
        private FloatItemVisibleEvent m_onVisible = new FloatItemVisibleEvent();
        public FloatItemVisibleEvent onVisible
        {
            get { return m_onVisible; }
            set { m_onVisible = value; }
        }

        [SerializeField]
        private FloatItemInvisibleEvent m_onInvisible = new FloatItemInvisibleEvent();
        public FloatItemInvisibleEvent onInvisible
        {
            get { return m_onInvisible; }
            set { m_onInvisible = value; }
        }

    }

}
