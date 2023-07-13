using System.Collections.Generic;
using UnityEngine;

public class UITweenBase : MonoBehaviour
{
    public string tweenKey = "";

    public List<UITweenItem> tweenItemList = new List<UITweenItem>();

    public virtual void OpenTween()
    {
        for (int i = 0, max = tweenItemList.Count; i < max; i++)
        {
            tweenItemList[i].PlayAnimation(true, null);
        }
    }

    public virtual void CloseTween()
    {
        for (int i = 0, max = tweenItemList.Count; i < max; i++)
        {
            tweenItemList[i].PlayAnimation(false, null);
        }
    }

    public void PlayOneFrame()
    {
        for (int i = 0, max = tweenItemList.Count; i < max; i++)
        {
            tweenItemList[i].PlayOneFrame();
        }
    }
}