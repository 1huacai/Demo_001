using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[SelectionBase]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class AorScrollRect : ScrollRect {

    [Serializable]
    public class AorScrollRectEvent : UnityEvent {}

    [Serializable]
    public class AorScrollRectTipEvent : UnityEvent<RectTransform> {}

    [Serializable]
    public class AorScrollRectDragEvent : UnityEvent<PointerEventData> {}

    /// <summary>
    /// tip显示组件(头部)
    /// </summary>
    [SerializeField]
    private RectTransform _updateHeadTipRT;

    private CanvasGroup _headTipCG;
    /// <summary>
    /// tip显示组件(底部)
    /// </summary>
    [SerializeField]
    private RectTransform _updateFootTipRT;

    private CanvasGroup _footTipCG;

    /// <summary>
    /// tip显示组件淡出时间
    /// </summary>
    public float TipFadeDuration = 0.5f;
    public float TipFadeFPS = 60f;

    /// <summary>
    /// 作用阀值
    /// </summary>
    [SerializeField] 
    private Vector2 _updateTipApplyThreshold = new Vector2(100, 100);

    private RectTransform _rt;

    protected override void Awake() {
        base.Awake();

        _rt = GetComponent<RectTransform>();

        updateTipInit();

    }

    public void setUpdateTip(Vector2 ApplyThreshold,GameObject assetGameObject_head = null, GameObject assetGameObject_foot = null) {
        RectTransform hrt = null, frt = null;
        if (assetGameObject_head != null) {
            GameObject ins_h = GameObject.Instantiate(assetGameObject_head) as GameObject;
            ins_h.name = assetGameObject_head.name;
            hrt = ins_h.GetComponent<RectTransform>();
            if (hrt == null) {
                GameObject.DestroyImmediate(ins_h);   
            }
        }
        if (assetGameObject_foot != null) {
            GameObject ins_f = GameObject.Instantiate(assetGameObject_foot) as GameObject;
            ins_f.name = assetGameObject_foot.name;
            frt = ins_f.GetComponent<RectTransform>();
            if (frt == null) {
                GameObject.DestroyImmediate(ins_f);
            }
        }
        if (hrt != null || frt != null) {
            setUpdateTip(ApplyThreshold, hrt, frt);
        }
    }

    public void setUpdateTip(Vector2 ApplyThreshold, RectTransform updateHeadTipRt = null, RectTransform updateFootTipRt = null) {
        if (updateHeadTipRt != null) {
            _updateHeadTipRT = updateHeadTipRt;
            _updateFootTipRT = updateFootTipRt;
            _updateTipApplyThreshold = ApplyThreshold;
            updateTipInit();
        }
    }

    private void updateTipInit() {
        if (content == null) return;

        if (_updateHeadTipRT != null) {

            _headTipCG = _updateHeadTipRT.GetComponent<CanvasGroup>();
            if (_headTipCG == null) {
                _headTipCG = _updateHeadTipRT.gameObject.AddComponent<CanvasGroup>();
            }
            _updateHeadTipRT.SetParent(transform, false);
            _updateHeadTipRT.SetAsFirstSibling();

            if (Application.isPlaying) {
                _headTipCG.alpha = 0;
            }

        }

        if (_updateFootTipRT != null) {

            _footTipCG = _updateFootTipRT.GetComponent<CanvasGroup>();
            if (_footTipCG == null) {
                _footTipCG = _updateFootTipRT.gameObject.AddComponent<CanvasGroup>();
            }
            
            _updateFootTipRT.SetParent(transform, false);
            _updateFootTipRT.SetAsFirstSibling();

            if (Application.isPlaying) {
                _footTipCG.alpha = 0;
            }
        }

    }

    

    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        
        _endTipX = 0;
        _endTipY = 0;
        
        _hasHeadTipInit = false;
        _hasFootTipInit = false;

        _hasHeadEventSended = false;
        _hasFootEventSended = false;

        if (_headTipCoroutine != null) {
            StopCoroutine(_headTipCoroutine);
            _headTipCoroutine = null;
        }

        if (_footTipCoroutine != null) {
            StopCoroutine(_footTipCoroutine);
            _footTipCoroutine = null;
        }

        _isKMoving = false;
        _dragPlus = true;

        if (m_onBeginDragUnityEvent != null) {
            m_onBeginDragUnityEvent.Invoke(eventData);
        }
    }

    private IEnumerator alphaFade(CanvasGroup cg,float lm, float fs) {

        float animTimeNum = 0;

        while (true) {
            animTimeNum += Time.deltaTime;
            if (animTimeNum >= lm) {
                cg.alpha -= animTimeNum * fs;
                if (cg.alpha <= 0) {
                    break;
                }
                animTimeNum = 0;
            }
            yield return new WaitForEndOfFrame();
        }
    }

   

    public override void OnDrag(PointerEventData eventData) {
        base.OnDrag(eventData);

        if (m_onDragUnityEvent != null) {
            m_onDragUnityEvent.Invoke(eventData);
        }
    }

    private Coroutine _headTipCoroutine;
    private Coroutine _footTipCoroutine;
    
    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);

        _isKMoving = true;
        _dragPlus = false;

        if (_headTipCG != null) {
            _headTipCoroutine = StartCoroutine(alphaFade(_headTipCG, 1f/TipFadeFPS, 1f/TipFadeDuration));
        }

        if (_footTipCG != null) {
            _footTipCoroutine = StartCoroutine(alphaFade(_footTipCG, 1f/TipFadeFPS, 1f/TipFadeDuration));
        }

        if (m_onEndDragUnityEvent != null) {
            m_onEndDragUnityEvent.Invoke(eventData);
        }
    }
    
    private int _endTipX, _endTipY;
    private bool _hasHeadTipInit, _hasFootTipInit;
    private bool _hasHeadEventSended, _hasFootEventSended;

    private bool _isKMoving = false;
    private bool _dragPlus = false;


    protected override void LateUpdate() {
        base.LateUpdate();
        
        if (_dragPlus) {

            if (horizontal && vertical) {
                //updateTip不支持四向滑动;
                return;
            }

            float apNum, apLimit;

            if (horizontal) {

                if (content.anchoredPosition.x > 0) {

                    if (_headTipCG != null) {
                        apNum = content.anchoredPosition.x;
                        apLimit = _updateTipApplyThreshold.x;
                        _headTipCG.alpha = Mathf.Clamp01(apNum / apLimit);

                        if (!_hasHeadTipInit) {
                            _hasHeadTipInit = true;
                            if (onHeadTipStartUnityEvent != null) {
                                onHeadTipStartUnityEvent.Invoke(_updateHeadTipRT);
                            }
                        }

                    }

                    if (content.anchoredPosition.x > _updateTipApplyThreshold.x) {

                        _endTipX = -1;

                        if (_headTipCG != null) {
                            if (!_hasHeadEventSended) {
                                _hasHeadEventSended = true;
                                if (onEnableHeadUpdateUnityEvent != null) {
                                    onEnableHeadUpdateUnityEvent.Invoke(_updateHeadTipRT);
                                }
                            }
                        }
                    }

                } else if (content.anchoredPosition.x < (_rt.rect.width - content.rect.width)) {

                    if (_footTipCG != null) {
                        apNum = Mathf.Abs(_rt.rect.width - content.rect.width - content.anchoredPosition.x);
                        apLimit = _updateTipApplyThreshold.x;
                        _footTipCG.alpha = Mathf.Clamp01(apNum / apLimit);

                        if (!_hasFootTipInit) {
                            _hasFootTipInit = true;
                            if (onFootTipStartUnityEvent != null) {
                                onFootTipStartUnityEvent.Invoke(_updateFootTipRT);
                            }
                        }

                    }

                    if (content.anchoredPosition.x < (_rt.rect.width - content.rect.width - _updateTipApplyThreshold.x)) {

                        _endTipX = 1;

                        if (_footTipCG != null) {
                            if (!_hasFootEventSended) {
                                _hasFootEventSended = true;
                                if (onEnableFootUpdateUnityEvent != null) {
                                    onEnableFootUpdateUnityEvent.Invoke(_updateFootTipRT);
                                }
                            }
                        }
                    }

                }


            } else if (vertical) {

                if (content.anchoredPosition.y < 0) {

                    if (_headTipCG != null) {
                        apNum = Mathf.Abs(content.anchoredPosition.y);
                        apLimit = _updateTipApplyThreshold.y;
                        _headTipCG.alpha = Mathf.Clamp01(apNum / apLimit);

                        if (!_hasHeadTipInit) {
                            _hasHeadTipInit = true;
                            if (onHeadTipStartUnityEvent != null) {
                                onHeadTipStartUnityEvent.Invoke(_updateHeadTipRT);
                            }
                        }

                    }

                    if (content.anchoredPosition.y < -_updateTipApplyThreshold.y) {

                        _endTipY = -1;

                        if (_headTipCG != null) {
                            if (!_hasHeadEventSended) {
                                _hasHeadEventSended = true;
                                if (onEnableHeadUpdateUnityEvent != null) {
                                    onEnableHeadUpdateUnityEvent.Invoke(_updateHeadTipRT);
                                }
                            }
                        }
                    }

                } else if (content.anchoredPosition.y > (content.rect.height - _rt.rect.height)) {

                    if (_footTipCG != null) {
                        apNum = Mathf.Abs(content.rect.height - _rt.rect.height - content.anchoredPosition.y);
                        apLimit = _updateTipApplyThreshold.y;
                        _footTipCG.alpha = Mathf.Clamp01(apNum / apLimit);

                        if (!_hasFootTipInit) {
                            _hasFootTipInit = true;
                            if (onFootTipStartUnityEvent != null) {
                                onFootTipStartUnityEvent.Invoke(_updateFootTipRT);
                            }
                        }

                    }

                    if (content.anchoredPosition.y > (content.rect.height - _rt.rect.height + _updateTipApplyThreshold.y)) {

                        _endTipY = 1;

                        if (_footTipCG != null) {
                            if (!_hasFootEventSended) {
                                _hasFootEventSended = true;
                                if (onEnableFootUpdateUnityEvent != null) {
                                    onEnableFootUpdateUnityEvent.Invoke(_updateFootTipRT);
                                }
                            }
                        }

                    }
                }

            }
        }

        if (_isKMoving) {
            if (Vector2.Distance(Vector2.zero,velocity) <= 0.15f) {
                _isKMoving = false;

                if (_endTipX == -1) {
                    if (m_onUpdateHeadEvent != null) {
                        m_onUpdateHeadEvent.Invoke();
                    }
                } else if (_endTipX == 1) {
                    if (m_onUpdateFootEvent != null) {
                        m_onUpdateFootEvent.Invoke();
                    }
                }

                if (_endTipY == -1) {
                    if (m_onUpdateHeadEvent != null) {
                        m_onUpdateHeadEvent.Invoke();
                    }
                } else if (_endTipY == 1) {
                    if (m_onUpdateFootEvent != null) {
                        m_onUpdateFootEvent.Invoke();
                    }
                }

            }

        }
    }

    [SerializeField]
    private AorScrollRectDragEvent m_onBeginDragUnityEvent = new AorScrollRectDragEvent();
    public AorScrollRectDragEvent onBeginDragUnityEvent { get { return m_onBeginDragUnityEvent; } set { m_onBeginDragUnityEvent = value; } }
    
    [SerializeField]
    private AorScrollRectDragEvent m_onDragUnityEvent = new AorScrollRectDragEvent();
    public AorScrollRectDragEvent onDragUnityEvent { get { return m_onDragUnityEvent; } set { m_onDragUnityEvent = value; } }

    [SerializeField]
    private AorScrollRectDragEvent m_onEndDragUnityEvent = new AorScrollRectDragEvent();
    public AorScrollRectDragEvent onEndDragUnityEvent { get { return m_onEndDragUnityEvent; } set { m_onEndDragUnityEvent = value; } }

    /// <summary>
    /// <事件> "头部刷新(到头了要求刷新之前的数据)"Tip组件开始显示时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectTipEvent m_onHeadTipStart = new AorScrollRectTipEvent();
    public AorScrollRectTipEvent onHeadTipStartUnityEvent { get { return m_onHeadTipStart; } set { m_onHeadTipStart = value; } }

    /// <summary>
    /// <事件> "脚部刷新(到底了要求刷新之后的数据)"Tip组件开始显示时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectTipEvent m_onFootTipStart = new AorScrollRectTipEvent();
    public AorScrollRectTipEvent onFootTipStartUnityEvent { get { return m_onFootTipStart; } set { m_onFootTipStart = value; } }

    /// <summary>
    /// <事件> 当用户达到"头部刷新(到头了要求刷新之前的数据)"阀值时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectTipEvent m_onEnableHeadUpdate = new AorScrollRectTipEvent();
    public AorScrollRectTipEvent onEnableHeadUpdateUnityEvent { get { return m_onEnableHeadUpdate; } set { m_onEnableHeadUpdate = value; } }


    /// <summary>
    /// <事件> 当用户达到"脚部刷新(到底了要求刷新之后的数据)"阀值时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectTipEvent m_onEnableFootUpdate = new AorScrollRectTipEvent();
    public AorScrollRectTipEvent onEnableFootUpdateUnityEvent { get { return m_onEnableFootUpdate; } set { m_onEnableFootUpdate = value; } }



    //----------------------------------------------------------------------

    /// <summary>
    /// <事件> 当用户触发"头部刷新(到头了要求刷新之前的数据)"时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectEvent m_onUpdateHeadEvent = new AorScrollRectEvent();
    public AorScrollRectEvent onUpdateHeadUnityEvent { get { return m_onUpdateHeadEvent; } set { m_onUpdateHeadEvent = value; } }

    /// <summary>
    /// <事件> 当用户触发"脚部刷新(到底了要求刷新之后的数据)"时调用该委托
    /// </summary>
    [SerializeField]
    private AorScrollRectEvent m_onUpdateFootEvent = new AorScrollRectEvent();
    public AorScrollRectEvent onUpdateFootUnityEvent { get { return m_onUpdateFootEvent; } set { m_onUpdateFootEvent = value; } }

    public void _SetHorizontalNormalizedPosition(float value)
    {
        this.SetNormalizedPosition(value, 0);
    }
    public void _SetVerticalNormalizedPosition(float value)
    {
        this.SetNormalizedPosition(value, 1);
    }

}