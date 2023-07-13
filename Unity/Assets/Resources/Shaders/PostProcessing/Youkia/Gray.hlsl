#ifndef YOUKIA_GRAY
#define YOUKIA_GRAY

#include "../StdLib.hlsl"
#include "../Youkia.hlsl"
#include "../Colors.hlsl"
#include "../MRT.hlsl"

// x: intensity, y: radius, z: radius width, w: scale
float4 _GrayParam;
// x: radius outer, y: radius2, z: scale2
float4 _GrayParam1;
// xy: noise texture tilling, zw: speed
float4 _GrayParam2;

// xy: pos1, zw: pos2
float4 _GrayPosition;
// x: radius 2, y: radius width 2, z: scale 2, w: radius outer 2
float4 _GrayParam3;

sampler2D _GrayNoiseTex;
half4 _GrayNoiseColor;

TEXTURE2D_SAMPLER2D(_GrayLut, sampler_GrayLut);
float4 _GrayLutParam;

inline half Fade(float3 wsPos, float2 position, float radius, float scale)
{
    float dis = distance(wsPos.xz, position);
    half fade = saturate(dis / radius);
    fade = pow(fade, scale);

    return fade;
}

inline half3 GrayEffect(half3 color, half2 uv, half particleMask)
{
    #if GRAY
        half intensity = _GrayParam.x;
        float2 position = _GrayPosition.xy;
        float2 position2 = _GrayPosition.zw;
        float radius = _GrayParam.y;
        float radiusWidth = _GrayParam.z;
        half scale = _GrayParam.w;
        float radiusOuter = _GrayParam1.x;
        float radiusOuter2 = _GrayParam1.y;
        half scale2 = _GrayParam1.z;
        float2 noiseTilling = _GrayParam2.xy;
        float2 noiseSpeed = _GrayParam2.zw;

        float depth = YoukiaDepth01(uv);
        float3 wsPos = GetWorldPositionFromLinearDepthValue(uv, depth).xyz;

        float dis = distance(wsPos.xz, position);
        half fade = saturate(dis / radiusOuter);
        fade = pow(fade, scale);

        half fade2 = Fade(wsPos, position2, radiusOuter2, scale2);

        half3 colLut = LinearToSRGB(saturate(color.rgb));
        colLut = ApplyLut2D(TEXTURE2D_PARAM(_GrayLut, sampler_GrayLut), colLut, _GrayLutParam);
        colLut = SRGBToLinear(colLut);
        colLut = lerp(colLut, color, particleMask);

        half3 eff = lerp(color.rgb, colLut, fade2 * fade);

        // radius width
        half width = saturate(dis - radius) * saturate(radiusOuter - dis);

        float2 uvNoise = (wsPos.xz - (position - radiusOuter * 0.5)) / (radiusOuter);

        float2 uvNoisePolar = UVPolar(uvNoise, 0, noiseTilling.x, noiseTilling.y);
        uvNoisePolar.xy = uvNoisePolar.yx;
        uvNoisePolar.y = -uvNoisePolar.y;

        half4 noise = tex2D(_GrayNoiseTex, uvNoisePolar + noiseSpeed * _Time.y) * _GrayNoiseColor;
        
        eff = lerp(eff, noise, width * noise.a);

        color = lerp(color, eff, intensity);
    #endif

    return color;
}

#endif
