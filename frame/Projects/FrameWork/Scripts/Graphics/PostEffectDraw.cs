using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.App;
using FrameWork.Manager;

namespace FrameWork.Graphics
{
    public class PostEffectDraw : MonoBehaviour
    {
        public GraphicsManager mgr;
        public GraphicsManager.eCameraType cameraType;

        //        void OnPostRender()
        //        {
        //
        //            if (!mgr)
        //                mgr = YKApplication.Instance.GetManager<GraphicsManager>();
        //
        //
        //            switch (cameraType)
        //            {
        //                case GraphicsManager.eCameraType.Effect:
        //                    mgr.DrawCard.OnEffectRenderAfter(ref mgr.MainRenderTexture);
        //                    // UnityEngine.Graphics.Blit(source, destination);
        //                    break;
        //                case GraphicsManager.eCameraType.PostEffect:
        //                    mgr.DrawCard.OnPostEffectRenderAfter(ref mgr.MainRenderTexture);
        //
        //                    break;
        //            }
        //
        //            //  mgr.DrawCard.OnPostEffectRenderAfter(ref source);
        //
        //        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (!mgr)
                mgr = SingletonManager.GetManager<GraphicsManager>();


            switch (cameraType)
            {
                case GraphicsManager.eCameraType.Effect:
                    mgr.DrawCard.OnEffectRenderAfter(ref source);
                    UnityEngine.Graphics.Blit(source, destination);
                    break;
                case GraphicsManager.eCameraType.PostEffect:
                    mgr.DrawCard.OnPostEffectRenderAfter(ref source);
                    UnityEngine.Graphics.Blit(source, destination, mgr.RenderMaterial);
                    break;
            }

        }
    }
}