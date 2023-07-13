using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrameWork.Graphics
{


    /// <summary>
    /// 绘制卡接口类
    /// </summary>
    public interface IGraphicDraw
    {
        //天空雾气渲染之后
        void OnSkyRenderAfter(ref RenderTexture source);

        //半透明处理之后
        void OnPreEffectAfter(ref RenderTexture source);

        void OnEffectRenderAfter(ref RenderTexture source);

        void OnPostEffectRenderAfter(ref RenderTexture source);

        Material GetMaterial();
        void OnBlitEnd();
    }


}
