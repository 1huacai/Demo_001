#ifndef UNITY_POSTFX_MASK
#define UNITY_POSTFX_MASK

#include "StdLib.hlsl"

TEXTURE2D_SAMPLER2D(_MaskBlurTex, sampler_MaskBlurTex);
float4 _MaskBlurTex_TexelSize;
TEXTURE2D_SAMPLER2D(_MaskBlurMaskTex, sampler_MaskBlurMaskTex);
float4 _MaskBlurMaskTex_TexelSize;

half2 _MaskCenter;
half4 _MaskSettings; // x: intensity, y: smoothness, z: roundness, w: rounded
half4 _MaskParam; // x: radius, y: opacity

half4 MaskTexture(half4 color, half2 texcoord)
{
    half mask = SAMPLE_TEXTURE2D(_MaskBlurMaskTex, sampler_MaskBlurMaskTex, texcoord).r;
    mask = lerp(1, mask, _MaskParam.y);

    half4 blur = SAMPLE_TEXTURE2D(_MaskBlurTex, sampler_MaskBlurTex, texcoord);

    color = lerp(blur, color, mask);
    return color;
}

half4 Mask(half4 color, half2 texcoord)
{
    half2 d = abs(texcoord - _MaskCenter) * _MaskSettings.x;

    // type - 0: 默认, 1 - 旋转, 2 - 正圆
    half type = _MaskSettings.w;

    half xLerp = saturate(type);
    half yLerp = 1 - abs(type - 1);
    
    d.x *= lerp(1.0, _ScreenParams.x / _ScreenParams.y, xLerp);
    d.y *= lerp(1.0, _ScreenParams.y / _ScreenParams.x, yLerp);
    d = pow(saturate(d), _MaskSettings.z); // Roundness
    half mfactor = pow(saturate(1.0 - dot(d, d)), _MaskSettings.y);

    mfactor = lerp(1, mfactor, _MaskParam.y);

    half4 blur = SAMPLE_TEXTURE2D(_MaskBlurTex, sampler_MaskBlurTex, texcoord);
    color = lerp(blur, color, mfactor);

    return color;
}

#endif // UNITY_POSTFX_MASK
