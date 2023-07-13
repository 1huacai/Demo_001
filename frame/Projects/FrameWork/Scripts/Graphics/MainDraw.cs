using UnityEngine;
using FrameWork.App;
using FrameWork.GUI.AorUI;
using FrameWork.Manager;

namespace FrameWork.Graphics
{
    /// <summary>
    /// 显示器绘制类
    /// </summary>
    public class MainDraw : MonoBehaviour
    {



        public Material lastMaterial
        {

            get
            {
                return SingletonManager.GetManager<GraphicsManager>().RenderMaterial;

            }

        }
        public int DrawPassCount = 0;


        public Material blackMaterial;
        private GraphicsManager mgr;



        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init()
        {

            //blackMaterial = new Material("Shader \"Hidden/Alpha\" {" + "SubShader {" + " Pass {" +
            //     " ZTest Always Cull Off ZWrite Off" + " Blend One Zero" + " Color (0,0,0,1)" + " }}}");

            blackMaterial = new Material(ApplicationCore.Instance.GetConstShader("Hidden/Alpha"));
            SetMaskColor(new Color(0, 0, 0, 1));
            SetExposure(1);

            mgr = SingletonManager.GetManager<GraphicsManager>();
        }


        /// <summary>
        /// 设置遮盖颜色
        /// </summary>
        /// <param name="color">遮盖颜色</param>
        public void SetMaskColor(Color color)
        {
            if (lastMaterial != null)
            {
                lastMaterial.SetColor("_MaskColor", color);
            }

        }




        /// <summary>
        /// 设置曝光度
        /// </summary>
        /// <param name="exp">曝光度</param>
        public void SetExposure(float exp)
        {
            if (lastMaterial != null)
            {
                lastMaterial.SetFloat("_Exposure", exp);
            }

        }

        void OnPostRender()
        {
            if (mgr == null)
            {
                DrawBlackScreen();
                return;
            }

            if (mgr.CurrentSubCamera == null)
            {
                DrawBlackScreen();
                return;
            }



            if (mgr.StopRender)
            {
                //画面保持功能暂时没了!
                DrawBlackScreen();
                return;
            }
 
         //   DrawToScreen();

        }


        //        void LateUpdate()
        //        {
        //            DrawToScreen();
        //        }

        public void DrawToScreen()
        {
            if(null == lastMaterial)
            {
                return;
            }
            UnityEngine.Graphics.SetRenderTarget(null);
            GL.PushMatrix();
          //  lastMaterial.mainTexture = mgr.MainRenderTexture;
            lastMaterial.SetPass(DrawPassCount);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
            GL.End();
            GL.PopMatrix();
        }

        void DrawBlackScreen()
        {
            if (null == blackMaterial)
            {
                return;
            }
            UnityEngine.Graphics.SetRenderTarget(null);
            //绘制到屏幕
            GL.PushMatrix();
            blackMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
            GL.End();
            GL.PopMatrix();
            // UnityEngine.Graphics.SetRenderTarget(mgr.MainRt);
        }

    }

}


