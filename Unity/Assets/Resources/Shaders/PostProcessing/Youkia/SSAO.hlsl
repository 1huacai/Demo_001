#ifndef YOUKIA_SSAO
#define YOUKIA_SSAO

#include "../StdLib.hlsl"
#include "../Youkia.hlsl"
#include "../Colors.hlsl"
#include "../MRT.hlsl"
#include "../Blur.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float4 _MainTex_TexelSize;

TEXTURE2D_SAMPLER2D(_SSAOMaskTex, sampler_SSAOMaskTex);
TEXTURE2D_SAMPLER2D(_SSAOTex, sampler_SSAOTex);
TEXTURE2D_SAMPLER2D(_SSAOTexTmp, sampler_SSAOTexTmp);
float4 _SSAOMaskTex_TexelSize;
float4 _SSAOTex_TexelSize;
float4 _SSAOTexTmp_TexelSize;

// Constants
// kContrast determines the contrast of occlusion. This allows users to control over/under
// occlusion. At the moment, this is not exposed to the editor because it's rarely useful.
static const float kContrast = 0.6;

// The constant below controls the geometry-awareness of the bilateral
// filter. The higher value, the more sensitive it is.
static const float kGeometryCoeff = 0.8;

// The constants below are used in the AO estimator. Beta is mainly used for suppressing
// self-shadowing noise, and Epsilon is used to prevent calculation underflow. See the
// paper (Morgan 2011 http://goo.gl/2iz3P) for further details of these constants.
static const float kBeta = 0.002;
#define EPSILON         1.0e-4

// x: intensity, y: sample count, z: radius, w: down sample
half4 _SSAOParam;
// x: shadow intensity, y: shadow radius
half4 _SSAOShadowParam;

#define INTENSITY _SSAOParam.x
#define SAMPLE_COUNT _SSAOParam.y
#define RADIUS _SSAOParam.z
#define DOWNSAMPLE _SSAOParam.w
#define RCP_DOWNSAMPLE rcp(DOWNSAMPLE * DOWNSAMPLE)
#define RCP_DOWNSAMPLE_G rcp(DOWNSAMPLE)
#define INTENSITY_SHADOW _SSAOShadowParam.x
#define RADIUS_SHADOW _SSAOShadowParam.y

float4 PackAONormal(float ao, float3 n)
{
    return float4(ao, n * 0.5 + 0.5);
}

float3 GetPackedNormal(float4 p)
{
    return p.gba * 2.0 - 1.0;
}

float GetPackedAO(float4 p)
{
    return p.r;
}

float CompareNormal(float3 d1, float3 d2)
{
    return smoothstep(kGeometryCoeff, 1.0, dot(d1, d2));
}

// Boundary check for depth sampler
// (returns a very large value if it lies out of bounds)
float CheckBounds(float2 uv, float d)
{
    float ob = any(uv < 0) + any(uv > 1);
#if defined(UNITY_REVERSED_Z)
    ob += (d <= 0.00001);
#else
    ob += (d >= 0.99999);
#endif
    return ob * 1e8;
}

// Sample point picker
float3 PickSamplePoint(float2 uv, float randAddon, int index)
{
    uv += _taaJitter.xy;
    float2 positionSS = uv * _ScreenParams.xy * DOWNSAMPLE;
    float gn = InterleavedGradientNoise(positionSS, index);
    float u = frac(unity_noise_randomValue(0.0, index + randAddon) + gn) * 2.0 - 1.0;
    float theta = (unity_noise_randomValue(1.0, index + randAddon) + gn) * TWO_PI;
    return float3(CosSin(theta) * sqrt(1.0 - u * u), u);
}

float SampleAndGetLinearDepth(float2 uv)
{
    // float d = YoukiaDepth01(uv);
    // return d * _ProjectionParams.z + CheckBounds(uv, d);
    return YoukiaDepthLinearEye(uv);
}

// Check if the camera is perspective.
// (returns 1.0 when orthographic)
float CheckPerspective(float x)
{
    return lerp(x, 1.0, unity_OrthoParams.w);
}

// Reconstruct view-space position from UV and depth.
// p11_22 = (unity_CameraProjection._11, unity_CameraProjection._22)
// p13_31 = (unity_CameraProjection._13, unity_CameraProjection._23)
float3 ReconstructViewPos(float2 uv, float depth, float2 p11_22, float2 p13_31)
{
    return float3((uv * 2.0 - 1.0 - p13_31) / p11_22 * depth, depth);
    // return float3((uv * 2.0 - 1.0 - p13_31) / p11_22 * CheckPerspective(depth), depth);
}

// Try reconstructing normal accurately from depth buffer.
// Low:    DDX/DDY on the current pixel
// Medium: 3 taps on each direction | x | * | y |
// High:   5 taps on each direction: | z | x | * | y | w |
// https://atyuwen.github.io/posts/normal-reconstruction/
// https://wickedengine.net/2019/09/22/improved-normal-reconstruction-from-depth/
half3 ReconstructNormal(float2 uv, float depth, float3 vpos, float2 p11_22, float2 p13_31)
{
    return normalize(cross(ddy(vpos), ddx(vpos)));
}

void SampleDepthNormalView(float2 uv, float2 p11_22, float2 p13_31, out float depth, out half3 normal, out float3 vpos)
{
    depth = SampleAndGetLinearDepth(uv);
    vpos = ReconstructViewPos(uv, depth, p11_22, p13_31);
    normal = ReconstructNormal(uv, depth, vpos, p11_22, p13_31);
}

float3x3 GetCoordinateConversionParameters(out float2 p11_22, out float2 p13_31)
{
    float3x3 camProj = (float3x3)unity_CameraProjection;

    p11_22 = float2(camProj._11, camProj._22);
    p13_31 = float2(camProj._13, camProj._23);
    
    return camProj;
}

half4 FragSSAO(VaryingsDefault i) : SV_Target
{
    half2 uv = i.texcoordStereo;

    // grass mask
    half4 mask = GetMask(uv);
    half grassMask = GetMaskGrass(mask);

    // shadow 
    half atten = SAMPLE_TEXTURE2D(_gSSS, sampler_gSSS, uv);
    atten = lerp(atten, 1, grassMask);
    RADIUS = lerp(RADIUS * RADIUS_SHADOW, RADIUS, atten);
    INTENSITY = lerp(INTENSITY * INTENSITY_SHADOW, INTENSITY, atten);

    // Parameters used in coordinate conversion
    float2 p11_22, p13_31;
    float3x3 camProj = GetCoordinateConversionParameters(p11_22, p13_31);

    // Get the depth, normal and view position for this fragment
    float depth_o;
    half3 norm_o;
    float3 vpos_o;
    SampleDepthNormalView(uv, p11_22, p13_31, depth_o, norm_o, vpos_o);

    // This was added to avoid a NVIDIA driver issue.
    float randAddon = uv.x * 1e-10;

    float rcpSampleCount = rcp(SAMPLE_COUNT);
    float ao = 0.0;
    [unroll(4)]
    for (int s = 0; s < int(SAMPLE_COUNT); s++)
    {
        #if defined(SHADER_API_D3D11)
            // This 'floor(1.0001 * s)' operation is needed to avoid a DX11 NVidia shader issue.
            s = floor(1.0001 * s);
        #endif

        // Sample point
        float3 v_s1 = PickSamplePoint(uv, randAddon, s);

        // Make it distributed between [0, _Radius]
        v_s1 *= sqrt((s + 1.0) * rcpSampleCount) * RADIUS;

        v_s1 = faceforward(v_s1, -norm_o, v_s1);
        float3 vpos_s1 = vpos_o + v_s1;

        // Reproject the sample point
        float3 spos_s1 = mul(camProj, vpos_s1);
        // float2 uv_s1_01 = clamp((spos_s1.xy * rcp(vpos_s1.z) + 1.0) * 0.5, 0.0, 1.0);
        // float2 uv_s1_01 = (spos_s1.xy / CheckPerspective(vpos_s1.z) + 1.0) * 0.5;
        float2 uv_s1_01 = (spos_s1.xy / vpos_s1.z + 1.0) * 0.5;

        // Depth at the sample point
        float depth_s1 = SampleAndGetLinearDepth(uv_s1_01);

        // Relative position of the sample point
        float3 vpos_s2 = ReconstructViewPos(uv_s1_01, depth_s1, p11_22, p13_31);
        float3 v_s2 = vpos_s2 - vpos_o;

        // Estimate the obscurance value
        float a1 = max(dot(v_s2, norm_o) - kBeta * depth_o, 0.0);
        float a2 = dot(v_s2, v_s2) + EPSILON;
        ao += a1 * rcp(a2);
    }

    // Intensity normalization
    ao *= RADIUS;

    // Apply contrast
    ao = PositivePow(ao * INTENSITY * rcpSampleCount, kContrast);

    return saturate(1 - ao);

    // bilateral filter
    // return PackAONormal(ao, norm_o);
}

//---------------------------------------------------------------------------
// blur
// Geometry-aware separable bilateral filter
half4 Blur(TEXTURE2D_ARGS(tex, samp), float2 uv, float2 delta) : SV_Target
{
    float4 p0  = SAMPLE_TEXTURE2D(tex, samp, uv                 );
    float4 p1a = SAMPLE_TEXTURE2D(tex, samp, uv - delta * 1.3846153846);
    float4 p1b = SAMPLE_TEXTURE2D(tex, samp, uv + delta * 1.3846153846);
    float4 p2a = SAMPLE_TEXTURE2D(tex, samp, uv - delta * 3.2307692308);
    float4 p2b = SAMPLE_TEXTURE2D(tex, samp, uv + delta * 3.2307692308);

    float3 n0 = GetPackedNormal(p0);

    float w0  =                                           0.2270270270;
    float w1a = CompareNormal(n0, GetPackedNormal(p1a)) * 0.3162162162;
    float w1b = CompareNormal(n0, GetPackedNormal(p1b)) * 0.3162162162;
    float w2a = CompareNormal(n0, GetPackedNormal(p2a)) * 0.0702702703;
    float w2b = CompareNormal(n0, GetPackedNormal(p2b)) * 0.0702702703;

    float s;
    s  = GetPackedAO(p0)  * w0;
    s += GetPackedAO(p1a) * w1a;
    s += GetPackedAO(p1b) * w1b;
    s += GetPackedAO(p2a) * w2a;
    s += GetPackedAO(p2b) * w2b;

    s *= rcp(w0 + w1a + w1b + w2a + w2b);
    
    return PackAONormal(s, n0);
}

// Geometry-aware bilateral filter (single pass/small kernel)
float BlurSmall(TEXTURE2D_ARGS(tex, samp), float2 uv, float2 delta)
{
    half4 p0 = SAMPLE_TEXTURE2D(tex, samp, UnityStereoTransformScreenSpaceTex(uv));
    half4 p1 = SAMPLE_TEXTURE2D(tex, samp, UnityStereoTransformScreenSpaceTex(uv + float2(-delta.x, -delta.y)));
    half4 p2 = SAMPLE_TEXTURE2D(tex, samp, UnityStereoTransformScreenSpaceTex(uv + float2( delta.x, -delta.y)));
    half4 p3 = SAMPLE_TEXTURE2D(tex, samp, UnityStereoTransformScreenSpaceTex(uv + float2(-delta.x,  delta.y)));
    half4 p4 = SAMPLE_TEXTURE2D(tex, samp, UnityStereoTransformScreenSpaceTex(uv + float2( delta.x,  delta.y)));

    half3 n0 = GetPackedNormal(p0);

    half w0 = 1.0;
    half w1 = CompareNormal(n0, GetPackedNormal(p1));
    half w2 = CompareNormal(n0, GetPackedNormal(p2));
    half w3 = CompareNormal(n0, GetPackedNormal(p3));
    half w4 = CompareNormal(n0, GetPackedNormal(p4));

    half s;
    s  = GetPackedAO(p0) * w0;
    s += GetPackedAO(p1) * w1;
    s += GetPackedAO(p2) * w2;
    s += GetPackedAO(p3) * w3;
    s += GetPackedAO(p4) * w4;

    return s / (w0 + w1 + w2 + w3 + w4);
}

half4 FragHorizontalBlur(VaryingsDefault i) : SV_Target
{
    half2 uv = i.texcoordStereo;
    float2 delta = float2(_SSAOTex_TexelSize.x * RCP_DOWNSAMPLE * 2.0, 0.0);
    return Blur(TEXTURE2D_PARAM(_SSAOTexTmp, sampler_SSAOTexTmp), uv, delta);
}

half4 FragVerticalBlur(VaryingsDefault i) : SV_Target
{
    half2 uv = i.texcoordStereo;
    float2 delta = float2(0.0, _SSAOTex_TexelSize.y * RCP_DOWNSAMPLE * 2.0);
    return Blur(TEXTURE2D_PARAM(_SSAOTex, sampler_SSAOTex), uv, delta);
}

half4 FinalBlur(VaryingsDefault i) : SV_Target
{
    half2 uv = i.texcoordStereo;
    float2 delta = _SSAOTex_TexelSize.xy * RCP_DOWNSAMPLE;
    return 1.0 - BlurSmall(TEXTURE2D_PARAM(_SSAOTexTmp, sampler_SSAOTexTmp), uv, delta);
}

// Gaussian blur
VaryingsGaussianBlur VertBlurH(AttributesDefault v)
{
    return VertH(v, RCP_DOWNSAMPLE_G, _SSAOTex_TexelSize);
}

VaryingsGaussianBlur VertBlurV(AttributesDefault v)
{
    return VertV(v, RCP_DOWNSAMPLE_G, _SSAOTex_TexelSize);
}

half4 FragGaussainBlurH(VaryingsGaussianBlur i) : SV_Target
{
    return FragGaussian(TEXTURE2D_PARAM(_SSAOTex, sampler_SSAOTex), i);
}

half4 FragGaussainBlurV(VaryingsGaussianBlur i) : SV_Target
{
    return FragGaussian(TEXTURE2D_PARAM(_SSAOTexTmp, sampler_SSAOTexTmp), i);
}

#endif
