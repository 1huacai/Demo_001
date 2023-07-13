using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.App;
using FrameWork.Graphics;
using FrameWork.Manager;

public class InversionEffect :MonoSwitch,  IPostEffect
{
    [Range(0,1)]
    public float inversionPower = 1;
    public Color inversionColor = Color.white;
    [Range(0, 1)]
    public float grayAmount = 0;

    private Material renderMat;
    private bool isInit;
    private GraphicsManager mgr;

    protected override void OnEnable()
    {
        base.OnEnable();

        waitYKApplicationWhenDo(this, () =>
        {
            addEffect();
            isInit = true;
        });

    }
    void addEffect()
    {
        if (renderMat == null)
        {
            renderMat = new Material(ApplicationCore.Instance.GetConstShader("Hidden/PostEffect/inversion"));
        }

        if (!mgr)
            mgr = SingletonManager.GetManager<GraphicsManager>();

        if (mgr != null && mgr.DrawCard != null)
        {


            (mgr.DrawCard as BaseDrawCard).AddEffect(this);



        }
    }

    void OnDisable()
    {
        if (mgr != null && mgr.DrawCard != null)
        {
            (mgr.DrawCard as BaseDrawCard).RemoveEffect(this);
        }
        if(renderMat)
            DestroyImmediate(renderMat);
    }


    public void Render(ref RenderTexture SrcRT,ref RenderTexture DstRT)
    {
        if (renderMat == null || renderMat.shader == null)
            return;

        renderMat.SetFloat("_InversionPower", inversionPower);
        renderMat.SetColor("_InversionColor", inversionColor);
        renderMat.SetFloat("_GrayAmount", grayAmount);

        Graphics.Blit(SrcRT, DstRT, renderMat);


    }
 
}
