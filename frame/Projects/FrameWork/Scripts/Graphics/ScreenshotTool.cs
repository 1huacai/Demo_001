using System;
using UnityEngine;
using FrameWork.App;
using FrameWork.Manager;

namespace FrameWork.Graphics
{
    /// <summary>
    /// 截屏工具,添加到最后的摄像机上,Callback中取得rt
    /// </summary>
    public class ScreenshotTool : MonoBehaviour
    {

        public Action<RenderTexture> Callback;
        public bool AutoDestroy = true;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (Callback != null)
                Callback(source);

            Callback = null;

            if (AutoDestroy)
                Destroy(this);
        }

        /// <summary>
        /// 获得3d视图截屏,包含特效和后期
        /// </summary>
        /// <param name="cb">截图回调</param>
        public static void GetAllImage(Action<RenderTexture> cb)
        {
            ScreenshotTool st = SingletonManager.GetManager<GraphicsManager>().FinalCamera.gameObject.AddComponent<ScreenshotTool>();
            st.Callback = cb;
        }

        public static void GetAllImageUI ( Camera camera, Action<RenderTexture> cb )
        {
            ScreenshotTool st = camera.gameObject.AddComponent<ScreenshotTool> ();
            st.Callback = cb;


        }


        /// <summary>
        /// 获得3d视图截屏,只是人物和场景
        /// </summary>
        /// <param name="cb">截图回调</param>
        public static void GetSceneImage(Action<RenderTexture> cb)
        {
            ScreenshotTool st = SingletonManager.GetManager<GraphicsManager>().MainCamera.gameObject.AddComponent<ScreenshotTool>();
            st.Callback = cb;
        }
    }





}
