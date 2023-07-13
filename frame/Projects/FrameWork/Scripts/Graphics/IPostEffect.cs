using UnityEngine;

namespace FrameWork.Graphics
{
    public interface IPostEffect
    {

        void Render(ref RenderTexture SrcRT, ref RenderTexture DstRT);
    }
}
