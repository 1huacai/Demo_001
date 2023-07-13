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
//简单处理事件 from AorScrollRect
public class SimpleScrollRect : ScrollRect
{

    [Serializable]
    public class SimpleRectReachTopEvent : UnityEvent<bool> { }


    /// <summary>
    /// 作用阀值
    /// </summary>
    [SerializeField]
    private float mThreshold = 2.0f;

    private RectTransform _rt;
    private bool IsReachTop;
    private bool IsReachDown;
    private bool IsInitTopDownState;

    protected override void Awake()
    {
        base.Awake();
        _rt = GetComponent<RectTransform>();
    }
    protected override void Start()
    {
        base.Start();
    }
    public void InitState()
    {
        IsInitTopDownState = true;
        if (horizontal && vertical)
        {
            //updateTip不支持四向滑动;
            return;
        }
        if (horizontal)
        {
            if (content.anchoredPosition.x >= 0 - mThreshold)
            {
                IsReachTop = true;
            }
            if (content.anchoredPosition.x <= (_rt.rect.width - content.rect.width + mThreshold))
            {
                IsReachDown = true;
            }
        }
        else if (vertical)
        {
            if (content.anchoredPosition.y <= 0 + mThreshold)
            {
                IsReachTop = true;
            }
            if (content.anchoredPosition.y >= (content.rect.height - _rt.rect.height - mThreshold))
            {
                IsReachDown = true;
            }
        }
        RechTopEvent.Invoke(IsReachTop);
        RechDownEvent.Invoke(IsReachDown);
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (IsInitTopDownState == false) return;
        if (horizontal)
        {
            if (content.anchoredPosition.x >= 0 - mThreshold)
            {
                if (!IsReachTop && RechTopEvent != null)
                {
                    IsReachTop = true;
                    RechTopEvent.Invoke(true);
                }
            }
            else
            {
                if (IsReachTop && RechTopEvent != null)
                {
                    IsReachTop = false;
                    RechTopEvent.Invoke(false);
                }
            }
            if (content.anchoredPosition.x <= (_rt.rect.width - content.rect.width + mThreshold))
            {
                if (!IsReachDown && RechDownEvent != null)
                {
                    IsReachDown = true;
                    RechDownEvent.Invoke(true);
                }
            }
            else
            {
                if (IsReachDown && RechDownEvent != null)
                {
                    IsReachDown = false;
                    RechDownEvent.Invoke(false);
                }
            }
        }
        else if (vertical)
        {
            if (content.anchoredPosition.y <= 0 + mThreshold)
            {
                if (!IsReachTop && RechTopEvent != null)
                {
                    IsReachTop = true;
                    RechTopEvent.Invoke(true);
                }
            }
            else
            {
                if (IsReachTop && RechTopEvent != null)
                {
                    IsReachTop = false;
                    RechTopEvent.Invoke(false);
                }
            }
            if (content.anchoredPosition.y >= (content.rect.height - _rt.rect.height - mThreshold))
            {
                if (!IsReachDown && RechDownEvent != null)
                {
                    IsReachDown = true;
                    RechDownEvent.Invoke(true);
                }
            }
            else
            {
                if (IsReachDown && RechDownEvent != null)
                {
                    IsReachDown = false;
                    RechDownEvent.Invoke(false);
                }
            }
        }
    }

    [SerializeField]
    private SimpleRectReachTopEvent mRechTopEvent = new SimpleRectReachTopEvent();
    public SimpleRectReachTopEvent RechTopEvent { get { return mRechTopEvent; } set { mRechTopEvent = value; } }

    [SerializeField]
    private SimpleRectReachTopEvent mRechDownEvent = new SimpleRectReachTopEvent();
    public SimpleRectReachTopEvent RechDownEvent { get { return mRechDownEvent; } set { mRechDownEvent = value; } }
}