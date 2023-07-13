using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.App;
using FrameWork.Graphics;
using FrameWork.Manager;

public class RadiaBlurEffect : MonoSwitch, IPostEffect
{
    public float Level = 50; //力度
    private bool isInit;

    private Material renderMat;
    private GraphicsManager mgr;

    void addEffect()
    {
        if (renderMat == null)
        {
            renderMat = new Material(ApplicationCore.Instance.GetConstShader("Hidden/PostEffect/RadiaBlur"));
        }

        if (!mgr)
            mgr = SingletonManager.GetManager<GraphicsManager>();

        if (mgr != null && mgr.DrawCard != null)
        {


            (mgr.DrawCard as BaseDrawCard).AddEffect(this);



        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        waitYKApplicationWhenDo(this, () =>
        {
            addEffect();
            isInit = true;
        });

    }





    void OnDisable()
    {
        if (mgr != null && mgr.DrawCard != null)
        {
            (mgr.DrawCard as BaseDrawCard).RemoveEffect(this);
        }
    }



    public void Render(ref RenderTexture SrcRT, ref RenderTexture DstRT)
    {
        if (renderMat == null || renderMat.shader == null)
            return;

        renderMat.SetFloat("_Level", Level);

        Vector3 pos = mgr.CurrentSubCamera.GetComponent<Camera>().WorldToScreenPoint(transform.position);

        renderMat.SetFloat("_CenterX", pos.x / Screen.width);
        renderMat.SetFloat("_CenterY", pos.y / Screen.height);

        Graphics.Blit(SrcRT, DstRT, renderMat);


    }
}
