#ifndef YOUKIA_POSTFX
#define YOUKIA_POSTFX

#include "StdLib.hlsl"

//--------------------------------------------------------------------------
// 深度图重建世界坐标
uniform float4x4 _gInverseVP;

inline float4 GetWorldPositionFromLinearDepthValue(half2 uv, half linearDepth) 
{
    float camPosZ = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * linearDepth;

    // unity_CameraProjection._m11 = near / t，其中t是视锥体near平面的高度的一半。
    // 投影矩阵的推导见：http://www.songho.ca/opengl/gl_projectionmatrix.html。
    // 这里求的height和width是坐标点所在的视锥体截面（与摄像机方向垂直）的高和宽，并且
    // 假设相机投影区域的宽高比和屏幕一致。
    float height = 2 * camPosZ / unity_CameraProjection._m11;
    float width = _ScreenParams.x / _ScreenParams.y * height;

    float camPosX = width * uv.x - width / 2;
    float camPosY = height * uv.y - height / 2;
    float4 camPos = float4(camPosX, camPosY, camPosZ, 1.0);
    return mul(unity_CameraToWorld, camPos);
}

inline float4 GetWorldPositionFromDepthValue(half2 uv, half depth) 
{
    float4 clipPos;
    clipPos.xy = uv * 2 - 1;
    clipPos.z = depth;
    clipPos.w = 1;

    float4 wsPos = mul(_gInverseVP, clipPos);
    wsPos /= wsPos.w;

    return wsPos;
}

//--------------------------------------------------------------------------
// tone mapping

// half _A, _B, _C, _D, _E;
inline half3 ACESToneMapping(half3 color, half adapted_lum)
{
	const half A = 2.51f;
	const half B = 0.03f;
	const half C = 2.43f;
	const half D = 0.59f;
	const half E = 0.14f;
    
	color *= adapted_lum;
	return (color * (A * color + B)) / (color * (C * color + D) + E);
}

half3 ACESToneMapping(half3 x)
{
	half a = 2.51f;
	half b = 0.03f;
	half c = 2.43f;
	half d = 0.59f;
	half e = 0.14f;
	return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
}

//--------------------------------------------------------------------------
// 场景亮度改变
half _gEnvirLum;
inline half3 EnvirLumChange(half3 color)
{
	color = color * (1 - _gEnvirLum);

	return color;
}

//--------------------------------------------------------------------------
// Encoding/decoding [0..1) floats into 8 bit/channel RGBA. Note that 1.0 will not be encoded properly.
inline float4 EncodeFloatRGBA( float v )
{
    float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
    float kEncodeBit = 1.0/255.0;
    float4 enc = kEncodeMul * v;
    enc = frac (enc);
    enc -= enc.yzww * kEncodeBit;
    return enc;
}
inline float DecodeFloatRGBA( float4 enc )
{
    float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
    return dot( enc, kDecodeDot );
}

// Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
inline float2 EncodeFloatRG( float v )
{
    float2 kEncodeMul = float2(1.0, 255.0);
    float kEncodeBit = 1.0/255.0;
    float2 enc = kEncodeMul * v;
    enc = frac (enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}
inline float DecodeFloatRG( float2 enc )
{
    float2 kDecodeDot = float2(1.0, 1/255.0);
    return dot( enc, kDecodeDot );
}

// -----------------------------------------------------------------------------
// Dither
inline half YoukiaDither(half2 uv)
{
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    return saturate(DITHER_THRESHOLDS[index]);
}

uniform float4 _taaJitter;
inline half YoukiaTAADither(half2 uv)
{
    uv += _taaJitter.xy * _ScreenParams.xy;
    return YoukiaDither(uv);
}

//-------------------------------------------------------------------------
// 顶视uv
inline half2 UVOrthographic(float3 wsPos, float3 camPos, float size)
{
    half2 uv = wsPos.xz - camPos.xz;
    uv = uv / (size * 2);
    uv += 0.5f;

    return uv;
}

// 顶视uv-增加一个采样的像素偏移，测试是否能缓解大地图迷雾透视问题
inline half2 UVOrthographicWithOffset(float3 wsPos, float3 camPos, float size, half2 texelSize, half xOffset, half yOffset)
{
    half2 uv = wsPos.xz - camPos.xz;
    uv = uv / (size * 2);
    uv += 0.5f;

    uv.x -= xOffset * texelSize.x;
    uv.y -= yOffset * texelSize.y;

    return uv;
}

//-------------------------------------------------------------------------
// noise
// unity noise node
// ref: https://docs.unity3d.com/Packages/com.unity.shadergraph@7.1/manual/Simple-Noise-Node.html
inline float unity_noise_randomValue(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
}

inline float unity_noise_randomValue(float u, float v)
{
    float f = dot(float2(12.9898, 78.233), float2(u, v));
    return frac(43758.5453 * sin(f));
}

inline float unity_noise_interpolate(float a, float b, float t)
{
    return (1.0-t)*a + (t*b);
}

inline float unity_valueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_noise_randomValue(c0);
    float r1 = unity_noise_randomValue(c1);
    float r2 = unity_noise_randomValue(c2);
    float r3 = unity_noise_randomValue(c3);

    float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
    float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}

inline float Unity_SimpleNoise_float(float2 UV, float Scale)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3-0));
    t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3-1));
    t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3-2));
    t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    return t;
}

inline half3 unity_random_vector(float2 uv)
{
    half3 vec = half3(0, 0, 0);
    vec.x = unity_noise_randomValue(uv) * 2 - 1;
    vec.y = unity_noise_randomValue(uv * uv) * 2 - 1;
    vec.z = saturate(unity_noise_randomValue(uv * uv * uv) + 0.2);
    return normalize(vec);  
}

//-------------------------------------------------------------------------
// Trigonometric function utility
float2 CosSin(float theta)
{
    float sn, cs;
    sincos(theta, sn, cs);
    return float2(cs, sn);
}

//From  Next Generation Post Processing in Call of Duty: Advanced Warfare [Jimenez 2014]
// http://advances.realtimerendering.com/s2014/index.html
float InterleavedGradientNoise(float2 pixCoord, int frameCount)
{
    const float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
    float2 frameMagicScale = float2(2.083f, 4.867f);
    pixCoord += frameCount * frameMagicScale;
    return frac(magic.z * frac(dot(pixCoord, magic.xy)));
}

//-----------------------------------------------------------------------
// uv
// polar coordinates
inline half2 UVPolar(half2 uv, half2 offset = 0, half radialScale = 1, half lengthScale = 1)
{
    half2 delta = uv - 0.5f;
    half radius = length(delta) * 2 * radialScale;
    half angle = frac(atan2(delta.x, delta.y) * 1.0 / 6.28 * lengthScale);
    half2 Out = half2(radius, angle) + offset;

    return Out;
}

#endif // YOUKIA_POSTFX
