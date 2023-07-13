#ifndef UNITY_POSTFX_BLUR
#define UNITY_POSTFX_BLUR

#include "StdLib.hlsl"

// Gaussian blur
static const float weight[3] = {0.4026, 0.2442, 0.0545};

struct VaryingsGaussianBlur
{
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float2 texcoordStereo : TEXCOORD1;
    float4 texcoord1 : TEXCOORD2;
    float4 texcoord2 : TEXCOORD3;
};

VaryingsGaussianBlur VertGaussian(AttributesDefault v, half4 blurSize, float4 texelSize)
{
    VaryingsGaussianBlur o;
    o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

    #if UNITY_UV_STARTS_AT_TOP
        o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
    #endif

    o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

    o.texcoord1 = o.texcoordStereo.xyxy + blurSize.xyxy * float4(1, 1, -1, -1) * texelSize.xyxy;
    o.texcoord2 = o.texcoordStereo.xyxy + blurSize.xyxy * float4(1, 1, -1, -1) * 2.0 * texelSize.xyxy;
    return o;
}

VaryingsGaussianBlur VertH(AttributesDefault v, half blurSize, float4 texelSize)
{
    half4 sizeHori = half4(blurSize, 0, blurSize, 0);
    return VertGaussian(v, sizeHori, texelSize);
}

VaryingsGaussianBlur VertV(AttributesDefault v, half blurSize, float4 texelSize)
{
    half4 sizeVert = half4(0, blurSize, 0, blurSize);
    return VertGaussian(v, sizeVert, texelSize);
}

half4 FragGaussian(TEXTURE2D_ARGS(tex, samp), VaryingsGaussianBlur i)
{
    half4 color = SAMPLE_TEXTURE2D(tex, samp, i.texcoordStereo) * weight[0];

    color += SAMPLE_TEXTURE2D(tex, samp, i.texcoord1.xy) * weight[1];
    color += SAMPLE_TEXTURE2D(tex, samp, i.texcoord1.zw) * weight[1];

    color += SAMPLE_TEXTURE2D(tex, samp, i.texcoord2.xy) * weight[2];
    color += SAMPLE_TEXTURE2D(tex, samp, i.texcoord2.zw) * weight[2];

    return color;
}

// Box blur

#endif // UNITY_POSTFX_BLUR
