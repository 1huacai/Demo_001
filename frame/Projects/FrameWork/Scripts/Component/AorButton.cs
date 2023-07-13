using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AorButton : Button, IGrayMember
{
    public static Action<string> OnSoundPlay;
    public static bool IsDisableAllBtn {
        get { return m_IsDisableAllBtn; }
        set {
            m_IsDisableAllBtn = value;
        }
    }
    private static bool m_IsDisableAllBtn= false;
    public bool EnableClickSound = true;
    
    public bool IgnoreClickThreshold = false;
    public bool IgnoreClickInterval = false;
    private string _ClickSoundPath = "Sound/Common/SimpleButton";
    public string ClickSoundPath
    {
        get
        {
            return _ClickSoundPath;
        }
        set
        {
            _ClickSoundPath = value;
        }
    }


    public float ClickThreshold = 40f;
    /// <summary>
    /// 点击间隔
    /// </summary>
    private float ClickInterval = 0.3f;
    /// <summary>
    /// 上次点击时间
    /// </summary>
    public static float lastClickTime = -1;

    private Dictionary<int, Color> oldColors;


    public override void OnPointerClick(PointerEventData eventData)
    {
        if (IsDisableAllBtn)
        {
            return;
        }
        if (IgnoreClickThreshold || (Vector3.Distance(eventData.pressPosition, eventData.position) < ClickThreshold))
        {
            if (IgnoreClickInterval || Time.realtimeSinceStartup - lastClickTime >= ClickInterval)
            {
                base.OnPointerClick(eventData);
                lastClickTime = Time.realtimeSinceStartup;

                if (EnableClickSound && !string.IsNullOrEmpty(ClickSoundPath) && OnSoundPlay != null)
                    OnSoundPlay(ClickSoundPath);
            }
        }

    }

    public bool IsGray
    {
        get
        {
            if (targetGraphic != null)
            {
                return ((IGrayMember)targetGraphic).IsGray;
            }
            return false;
        }
    }

    public void SetGrayWithInteractable(bool bo)
    {
        interactable = !bo;
        SetGray(bo);
    }

    /// <summary>
    /// 变灰
    /// </summary>
    public void SetGray(bool bo)
    {
        if (targetGraphic != null)
            ((IGrayMember)targetGraphic).SetGray(bo);

        GrayManager.SetGaryWithAllChildren(transform, bo);

    }


    protected override void OnDestroy()
    { 
        base.OnDestroy();
        targetGraphic = null;
        oldColors = null;
        if (null != onClick)
        {
            //释放可能注入的Lamda表达式形成的闭包
            onClick.RemoveAllListeners();
        }

    }
}
