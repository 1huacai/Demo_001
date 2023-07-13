using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoopListItemStart
{
    bool OnLoopListItemStart(Transform item);
}
[RequireComponent(typeof(Animation))] public class UITweenItem : MonoBehaviour
{
    public string openName = "OpenAnim";
    public string closeName = "CloseAnim";
    private Animation anim = null;
    /// <summary>
    /// 是否是前进动画
    /// </summary>
    private bool isForward = false;
    /// <summary>
    /// 完成回调
    /// </summary>
    private Action<bool> callBack = null;
    /// <summary>
    /// 当前的动画名称
    /// </summary>
    private string curAnim = "";
    /// <summary>
    /// 是否停止在第一帧
    /// </summary>
    private bool playOneFarem = false;
    private List<ILoopListItemStart> _tempLoopListItemStart = new List<ILoopListItemStart>();
    private void Awake()
    {
        anim = GetComponent<Animation>();
    }
    private void Start()
    {
        if (playOneFarem)
        {
            PlayOneFrame();
        }
        InvokeScaleChangeInParent(this.transform);
    }
    public void InvokeScaleChangeInParent(Transform trans)
    {
        _tempLoopListItemStart.Clear();
        trans.GetComponentsInParent(false, _tempLoopListItemStart);
        for (int num = _tempLoopListItemStart.Count - 1; num >= 0; num--)
        {
            ILoopListItemStart itemStart = _tempLoopListItemStart[num];
            itemStart.OnLoopListItemStart(trans);
        }
    }
    public void PlayOneFrame()
    {
        ResetOneFrame(openName);
    }
    /// <summary>
    /// 重置到动画片段的第一帧
    /// </summary>
    /// <param name="animName"></param>
    public void ResetOneFrame(string animName)
    {
        AnimationState state = anim[animName];
        anim.Play(animName);
        state.time = 0;
        anim.Sample();
        state.enabled = false;
    }
    public void PlayAnimation(bool forward, Action<bool> callBack = null)
    {
        isForward = forward;
        this.callBack = callBack;
        string animName = forward ? openName : closeName;
        this.PlayAnimationByName(animName, callBack);
    }
    public void PlayAnimationByName(string animName, Action<bool> callBack = null)
    {
        this.callBack = callBack;
        if (string.IsNullOrEmpty(animName))
        {
            PlayFinish();
            return;
        }
        float length = GetAnimLength(animName);
        if (length == 0)
        {
            PlayFinish();
            return;
        }
        if (curAnim == animName) return;
        curAnim = animName;
        if (this.gameObject.activeInHierarchy)
        {
            anim.Play(curAnim);
        }
        CancelInvoke("PlayFinish");
        Invoke("PlayFinish", length);
    }
    private float GetAnimLength(string name)
    {
        float length = 0;
        if (anim != null)
        {
            var ani = anim[name];
            if (ani != null)
            {
                length = ani.clip.length;
            }
        }
        return length + 0.034f;
    }
    private void PlayFinish()
    {
        if (this.callBack != null)
        {
            this.callBack(true);
            curAnim = "";
        }
    }
}