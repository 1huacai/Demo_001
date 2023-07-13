using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FrameWork.App;
using FrameWork.Manager;

namespace FrameWork.Graphics
{

    /// <summary>
    /// 一个基本的绘制卡,图形管理器通过自定义的绘制卡来实现各种效果
    /// </summary>
    public class BaseDrawCard : IGraphicDraw
    {
        protected List<IPostEffect> PostEffects = new List<IPostEffect>();
        protected Material mat;

        private Material _reverseMat;
        protected Material reverseMat
        {
            get
            {
                if (_reverseMat == null)
                    _reverseMat = new Material(ApplicationCore.Instance.GetConstShader("Hidden/PostEffect/reverse"));


                return _reverseMat;
            }

        }


        private bool _RttMode;

        private GraphicsManager _Graphicsmgr;
        protected GraphicsManager Graphicsmgr
        {
            get
            {
                if (!_Graphicsmgr)
                    _Graphicsmgr = SingletonManager.GetManager<GraphicsManager>();

                return _Graphicsmgr;
            }

        }

        private PostEffectDraw postEffectDraw;

        public void AddEffect(IPostEffect postEffect)
        {

            if (!Graphicsmgr.EnablePostEffect)
                return;

            if (!PostEffects.Contains(postEffect))
                PostEffects.Add(postEffect);

            if (!postEffectDraw)
            {
                postEffectDraw = Graphicsmgr.PostEffectCamera.GetComponent<PostEffectDraw>();
            }

            postEffectDraw.enabled = true;



        }


        public void RemoveEffect(IPostEffect postEffect)
        {

            PostEffects.Remove(postEffect);


            if (PostEffects.Count == 0)
                postEffectDraw.enabled = false;



        }



        public BaseDrawCard()
        {
            //    Shader = YKApplication.Instance.ConstShader["Hidden/PostEffect/DrawShader"];
        }

        public virtual void OnBlitEnd()
        {

        }

        public virtual void OnEffectRenderAfter(ref RenderTexture source)
        {

        }

        void setMat()
        {

            mat = new Material(ApplicationCore.Instance.GetConstShader("Hidden/PostEffect/DrawShader"));

            mat.SetTexture("_CurveTex", Texture2D.blackTexture);

        }

        public virtual Material GetMaterial()
        {

            if (mat == null)
            {

                setMat();
            }


            return mat;
        }

        public virtual void OnEffectRenderBefore(ref RenderTexture source)
        {
            //throw new NotImplementedException();
        }

        public virtual void OnSkyRenderAfter(ref RenderTexture source)
        {
            //  throw new NotImplementedException();
        }

        public virtual void OnPreEffectAfter(ref RenderTexture source)
        {
            //  throw new NotImplementedException();
        }


        void copy(ref RenderTexture mainRt, ref RenderTexture rt)
        {

            //默认后期模式需要区分平台,不然上下颠倒
            if (Application.isEditor)
            {

                UnityEngine.Graphics.Blit(mainRt, rt, reverseMat);
            }
            else
            {
                UnityEngine.Graphics.Blit(mainRt, rt);
            }
        }

        private HashSet<Type> renderDic = new HashSet<Type>();
        public virtual void OnPostEffectRenderAfter(ref RenderTexture source)
        {

            if (PostEffects.Count > 0)
            {

                RenderTexture rt = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, source.depth, source.format);
                rt.name = "BaseDrawCard_RT";
                copy(ref source, ref rt);

                for (int i = 0; i < PostEffects.Count; i++)
                {
                    Type t = PostEffects[i].GetType();
                    if (!renderDic.Contains(t))
                    {
                        renderDic.Add(t);
                    }
                    else
                    {
                        continue;
                    }

                    if (PostEffects[i] != null)
                        PostEffects[i].Render(ref rt, ref source);

                    //多个混合效果要把画面留下重用
                    if (PostEffects.Count > 1)
                        copy(ref source, ref rt);
                }
                renderDic.Clear();
                //rt.Release(); 
                RenderTexture.ReleaseTemporary(rt);

            }

            //   Graphicsmgr.FinalDraw.EnableEfectDrawScript();

        }
    }
}
