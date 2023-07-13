#ifndef UNITY_POSTFX_DEPTH_OF_FIELD
#define UNITY_POSTFX_DEPTH_OF_FIELD

#include "../StdLib.hlsl"
#include "../Colors.hlsl"
#include "../MRT.hlsl"
#include "DiskKernels.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float4 _MainTex_TexelSize;

TEXTURE2D_SAMPLER2D(_CoCTex, sampler_CoCTex);

TEXTURE2D_SAMPLER2D(_DepthOfFieldTex, sampler_DepthOfFieldTex);
float4 _DepthOfFieldTex_TexelSize;

TEXTURE2D_SAMPLER2D(_gSceneTex, sampler_gSceneTex);

// Camera parameters
float _Distance;
float _LensCoeff;  // f^2 / (N * (S1 - f) * film_width * 2)
float _MaxCoC;
float _RcpMaxCoC;
float _RcpAspect;
half _DOFBlurRadius;

half4 _DepthOfFieldColor;

struct VaryingsSimpleBlur
{
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float2 texcoordStereo : TEXCOORD1;
    float4 uv1 : TEXCOORD2;
    float4 uv2 : TEXCOORD3;	
    #if STEREO_INSTANCING_ENABLED
        uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
    #endif
};

// simple blur
VaryingsSimpleBlur VertSimpleBlur(AttributesDefault v)
{
    VaryingsSimpleBlur o;
    o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

    #if UNITY_UV_STARTS_AT_TOP
        o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
    #endif

    o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

    o.uv1.xy = o.texcoordStereo.xy + _DOFBlurRadius * _MainTex_TexelSize * half2(1, 1);
    o.uv1.zw = o.texcoordStereo.xy + _DOFBlurRadius * _MainTex_TexelSize * half2(-1, 1);
    o.uv2.xy = o.texcoordStereo.xy + _DOFBlurRadius * _MainTex_TexelSize * half2(-1, -1);
    o.uv2.zw = o.texcoordStereo.xy + _DOFBlurRadius * _MainTex_TexelSize * half2(1, -1);

    return o;
}

// CoC calculation
half4 FragCoC(VaryingsDefault i) : SV_Target
{
    float depth = LinearEyeDepth(YoukiaDepth(i.texcoordStereo));
    half coc = (depth - _Distance) * _LensCoeff / max(depth, 1e-4);
    return saturate(coc * 0.5 * _RcpMaxCoC + 0.5);
}

// Prefilter: downsampling and premultiplying
half4 FragPrefilter(VaryingsDefault i) : SV_Target
{
    float3 duv = _MainTex_TexelSize.xyx * float3(0.5, 0.5, -0.5);
    float2 uv0 = UnityStereoTransformScreenSpaceTex(i.texcoord - duv.xy);
    float2 uv1 = UnityStereoTransformScreenSpaceTex(i.texcoord - duv.zy);
    float2 uv2 = UnityStereoTransformScreenSpaceTex(i.texcoord + duv.zy);
    float2 uv3 = UnityStereoTransformScreenSpaceTex(i.texcoord + duv.xy);

    // Sample source colors
    half3 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0).rgb;
    half3 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).rgb;
    half3 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2).rgb;
    half3 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv3).rgb;

    // Sample CoCs
    half coc0 = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, uv0).r * 2.0 - 1.0;
    half coc1 = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, uv1).r * 2.0 - 1.0;
    half coc2 = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, uv2).r * 2.0 - 1.0;
    half coc3 = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, uv3).r * 2.0 - 1.0;

// #endif

    // Apply CoC and luma weights to reduce bleeding and flickering
    float w0 = abs(coc0) / (Max3(c0.r, c0.g, c0.b) + 1.0);
    float w1 = abs(coc1) / (Max3(c1.r, c1.g, c1.b) + 1.0);
    float w2 = abs(coc2) / (Max3(c2.r, c2.g, c2.b) + 1.0);
    float w3 = abs(coc3) / (Max3(c3.r, c3.g, c3.b) + 1.0);

    // Weighted average of the color samples
    half3 avg = c0 * w0 + c1 * w1 + c2 * w2 + c3 * w3;
    avg /= max(w0 + w1 + w2 + w3, 1e-4);

    // Select the largest CoC value
    half coc_min = min(coc0, Min3(coc1, coc2, coc3));
    half coc_max = max(coc0, Max3(coc1, coc2, coc3));
    half coc = (-coc_min > coc_max ? coc_min : coc_max) * _MaxCoC;

    // Premultiply CoC again
    avg *= smoothstep(0, _MainTex_TexelSize.y * 2, abs(coc));

#if defined(UNITY_COLORSPACE_GAMMA)
    avg = SRGBToLinear(avg);
#endif

    return half4(avg, coc);
}

// Prefilter: downsampling and premultiplying
half4 FragDownSample(VaryingsDefault i) : SV_Target
{
    // Sample source colors
    half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
    half coc = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, i.texcoordStereo).r;
    col.a = coc;
    
    return col;
}

// Bokeh filter with disk-shaped kernels
half4 FragBlur(VaryingsDefault i) : SV_Target
{
    half4 samp0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

    half4 bgAcc = 0.0; // Background: far field bokeh
    half4 fgAcc = 0.0; // Foreground: near field bokeh

    UNITY_LOOP
    for (int si = 0; si < kSampleCount; si++)
    {
        float2 disp = kDiskKernel[si] * _MaxCoC;
        float dist = length(disp);

        float2 duv = float2(disp.x * _RcpAspect, disp.y);
        half4 samp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.texcoord + duv));

        // BG: Compare CoC of the current sample and the center sample
        // and select smaller one.
        half bgCoC = max(min(samp0.a, samp.a), 0.0);

        // Compare the CoC to the sample distance.
        // Add a small margin to smooth out.
        const half margin = _MainTex_TexelSize.y * 2;
        half bgWeight = saturate((bgCoC   - dist + margin) / margin);
        half fgWeight = saturate((-samp.a - dist + margin) / margin);

        // Cut influence from focused areas because they're darkened by CoC
        // premultiplying. This is only needed for near field.
        fgWeight *= step(_MainTex_TexelSize.y, -samp.a);

        // Accumulation
        bgAcc += half4(samp.rgb, 1.0) * bgWeight;
        fgAcc += half4(samp.rgb, 1.0) * fgWeight;
    }

    // Get the weighted average.
    bgAcc.rgb /= bgAcc.a + (bgAcc.a == 0.0); // zero-div guard
    fgAcc.rgb /= fgAcc.a + (fgAcc.a == 0.0);

    // BG: Calculate the alpha value only based on the center CoC.
    // This is a rather aggressive approximation but provides stable results.
    bgAcc.a = smoothstep(_MainTex_TexelSize.y, _MainTex_TexelSize.y * 2.0, samp0.a);

    // FG: Normalize the total of the weights.
    fgAcc.a *= PI / kSampleCount;

    // Alpha premultiplying
    half alpha = saturate(fgAcc.a);
    half3 rgb = lerp(bgAcc.rgb, fgAcc.rgb, alpha);

    return half4(rgb, alpha);
}

half3 BlurColor(half cocBlur, half4 col, half4 blurCol)
{
    half flg = saturate(floor(cocBlur));
    
    return col.rgb * (1 - flg) + blurCol.rgb * flg;
}

// simple blur
half4 FragSimpleBlur(VaryingsSimpleBlur i) : SV_Target
{
    half4 col = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo));
    half a = col.a;
    half4 sum = col;

    half4 tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv1.xy));
    // sum.rgb += BlurColor(tmp.a, col, tmp);
    sum += tmp;
    tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv1.zw));
    // sum.rgb += BlurColor(tmp.a, col, tmp);
    sum += tmp;
    tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv2.xy));
    // sum.rgb += BlurColor(tmp.a, col, tmp);
    sum += tmp;
    tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv2.zw));
    // sum.rgb += BlurColor(tmp.a, col, tmp);
    sum += tmp;

    sum *= 0.2f;
    // col.a = a;
    // col.rgb = sum.rgb;
    col = sum;
    return col;
}

// Combine with source
half4 FragCombine(VaryingsDefault i) : SV_Target
{
    half4 dof = SAMPLE_TEXTURE2D(_DepthOfFieldTex, sampler_DepthOfFieldTex, i.texcoordStereo) * _DepthOfFieldColor;

    half coc = SAMPLE_TEXTURE2D(_CoCTex, sampler_CoCTex, i.texcoordStereo).r;
    coc = (coc - 0.5) * 2.0 * _MaxCoC;

    // Convert CoC to far field alpha value.
    float ffa = smoothstep(_MainTex_TexelSize.y * 2.0, _MainTex_TexelSize.y * 4.0, coc);

    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

#if defined(UNITY_COLORSPACE_GAMMA)
    color = SRGBToLinear(color);
#endif

    half alpha = Max3(dof.r, dof.g, dof.b);

    // lerp(lerp(color, dof, ffa), dof, dof.a)
    color = lerp(color, float4(dof.rgb, alpha), ffa + dof.a - ffa * dof.a);
    // color = lerp(color, float4(dof.rgb, alpha), ffa);

#if defined(UNITY_COLORSPACE_GAMMA)
    color = LinearToSRGB(color);
#endif

    return color;
}

#endif // UNITY_POSTFX_DEPTH_OF_FIELD
