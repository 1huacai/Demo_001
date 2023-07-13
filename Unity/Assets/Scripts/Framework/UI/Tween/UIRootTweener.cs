using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UITweenItem))]
public class UIRootTweener : MonoBehaviour
{
    public List<UITweenBase> tweenBases = new List<UITweenBase>();
    
    private UITweenItem rootAnimation = null;
    private bool isForward = false;

    /// <summary>
    /// 是否Enable 播放动画
    /// </summary>
    public bool PlayEnable = false;

    /// <summary>
    /// 是否播放第一帧
    /// </summary>
    public bool PlayFrame = false;

    private void Awake()
    {
        rootAnimation = this.GetComponent<UITweenItem>();
        if (rootAnimation == null)
        {
            rootAnimation = this.gameObject.AddComponent<UITweenItem>();
        }
    }

    private void Start()
    {
        if (PlayFrame)
        {
            PlayOneFrame();
        }
    }

    private void OnEnable()
    {
        if(PlayEnable)
        {
            PlayAnimation(true);
        }
    }

    private void OnDisable()
    {
        PlayAnimation(false);
    }

    public void OnStringEvent(string key)
    {
        for (int i = 0, max = tweenBases.Count; i < max; i++)
        {
            UITweenBase tweenBase = tweenBases[i];
            if (tweenBase.tweenKey == key)
            {
                if (isForward)
                {
                    tweenBase.OpenTween();
                }
                else
                {
                    tweenBase.CloseTween();
                }
            }
        }
    }
    /// <summary>
    /// 重置到动画的第一帧
    /// </summary>
    private void PlayOneFrame()
    {
        rootAnimation.PlayOneFrame();
        for (int i = 0,max = tweenBases.Count; i <max; i++)
        {
            tweenBases[i].PlayOneFrame();
        }
    }

    public void PlayAnimation(bool forward, Action<bool> callBack = null)
    {
        this.isForward = forward;
        rootAnimation.PlayAnimation(forward, callBack);
    }

    public void PlayAnimationName(string animName, Action<bool> callBack = null)
    {
        rootAnimation.PlayAnimationByName(animName, callBack);
    }
    public void ResetOneFrame(string animName)
    {
        rootAnimation.ResetOneFrame(animName);
    }
}
