#ifndef YOUKIA_HEIGHTFOG
#define YOUKIA_HEIGHTFOG

#include "../StdLib.hlsl"
#include "../Youkia.hlsl"
#include "../Colors.hlsl"
#include "../MRT.hlsl"

// height
half4 _HFColor, _HFColor1;
// x: intensity, y: distance, z: height, w: height far
float4 _HFParam;
 // x: near enable, y: near, z: near scale
half4 _HFNearParam;
// x: scale, y: strength, zw: speed
half4 _HFNoiseParam;
// sky box
// x: skybox distance, y: skybox intensity, z: skybox height scale, w: skybox height
half4 _HFSkyboxParam;

sampler2D _FogLut;
sampler2D _HeightFogNoiseTex;

// -----------------------------------------------
// fog height map
// not use now

// sampler2D _FogHeightMap;
// half _fogHeightMapSize_x;
// half _fogHeightMapSize_y;
// half _fogHeightMapOffset_x;
// half _fogHeightMapOffset_z;
// half _fogHeightMapStartHeight;
// half _fogHeightMapMaxHeight;
// half _fogHeightMapMinFalloff;
// -----------------------------------------------

#include "../PostProcessMulti.hlsl"

#define MAXFOGHEIGHT 100

inline half HeightFog(half intensity, float fogHeight, half fogFalloff, float3 wsPos, half noise = 0)
{
    // 相机高度
    half camHeight = _WorldSpaceCameraPos.y - fogHeight;
    half fogDensity = exp2(-fogFalloff * camHeight) * intensity;

    // 人眼于物体的相对高度
    half eyeHeight = wsPos.y - _WorldSpaceCameraPos.y;
    half falloff = fogFalloff * eyeHeight + noise;
    falloff = falloff == 0 ? 0.00001f : falloff;

    half fogFactor = (1 - exp2(-falloff)) / falloff;
    half fog = fogFactor * fogDensity;
    fog = saturate(fog);

    return fog;
}

inline half WarFog(half intensity, float fogHeight, half fogFalloff, float3 wsPos, half isGuideScene, half noise = 0)
{
    // 相机高度
    half camHeight = _WorldSpaceCameraPos.y - fogHeight;
    half fogDensity = exp2(-fogFalloff * camHeight) * intensity;
    fogDensity = lerp(fogDensity, intensity, isGuideScene);

    // 人眼于物体的相对高度
    half eyeHeight = wsPos.y - _WorldSpaceCameraPos.y;
    half falloff = fogFalloff * eyeHeight + noise;

    half fogFactor = (1 - exp2(-falloff)) / falloff;
    fogFactor = lerp(fogFactor, intensity, isGuideScene);
    half fog = fogFactor * fogDensity;
    fog = saturate(fog);

    return fog;
}

half Fog(int pmId, float3 rayDir, float dis, half skybox, float3 wsPos, in out half3 fogColor)
{
    // x: intensity, y: distance, z: height, w: height far
    float4 pmParam = GetHFParam(pmId);
    // x: near enable, y: near, z: near scale
    half4 pmNearParam = GetHFNearParam(pmId);
    // x: scale, y: strength, zw: speed
    half4 pmNoiseParam = GetHFNoiseParam(pmId);

    half farScale = saturate(dis * (1.0 / pmParam.y));

    half fogFalloff = 1;
    half fogHeight = 0;
    half intensity = pmParam.x;

    half noise = 0;
    #ifdef _HEIGHTFOG_NOISE
        noise = tex2D(_HeightFogNoiseTex, wsPos.xz * pmNoiseParam.x + pmNoiseParam.zw * _Time.x);
        noise = lerp(0, noise, pmNoiseParam.y);
    #endif

    // #ifdef _HEIGHTFOG_HEIGHTMAP
    //     fogFalloff = 1;
    //     fogHeight = _fogHeightMapStartHeight;

    //     float2 heightMap_uv = float2(((wsPos.x - _fogHeightMapOffset_x) - floor(wsPos.x / _fogHeightMapSize_x) * _fogHeightMapSize_x) / _fogHeightMapSize_x,
    //         ((wsPos.z - _fogHeightMapOffset_z) - floor(wsPos.z / _fogHeightMapSize_y) * _fogHeightMapSize_y) / _fogHeightMapSize_y);
    //     half4 heightMapValue = tex2D(_FogHeightMap, heightMap_uv);

    //     fogFalloff = max((-heightMapValue.a + 1) * 2, _fogHeightMapMinFalloff);
    //     fogHeight = fogHeight + heightMapValue.r * (_fogHeightMapMaxHeight - _fogHeightMapStartHeight);
    // #else
        half2 foglut = GetHFLut(farScale, pmId).rg;

        fogFalloff = max(foglut.g, 0.0001f);
        fogHeight = lerp(pmParam.z, pmParam.w, foglut.r);
    // #endif

    fogFalloff = lerp(fogFalloff, _HFSkyboxParam.z, skybox);
    fogHeight = lerp(fogHeight, _HFSkyboxParam.w, skybox);
    intensity = lerp(intensity, _HFSkyboxParam.y, skybox);
    noise = lerp(noise, 0, skybox);
    half fog = HeightFog(intensity, fogHeight, fogFalloff, wsPos, noise);
    // near
    half disFactor = saturate(dis * (1.0 / pmNearParam.y));
    half fade = saturate(pow(disFactor, pmNearParam.z));
    fade = lerp(1, fade, pmNearParam.x);
    fog *= fade;
    
    fogColor = lerp(GetHFColor(pmId).rgb, GetHFColor1(pmId).rgb, farScale);

    return fog;
}

inline half4 HeightFog(half2 texcoord)
{
    float depth = YoukiaDepth01(texcoord);
    float3 wsPos = GetWorldPositionFromLinearDepthValue(texcoord, depth).xyz;

    // multi
    int pmBlendId;
    half pmBlend;
    int pmId = YoukiaEMId(wsPos, pmBlendId, pmBlend);

    float3 rayDir = wsPos - _WorldSpaceCameraPos;
    float dis = length(rayDir);

    // skybox
    // half skybox = saturate((depth - _HFSkyboxParam.x) / abs(depth - _HFSkyboxParam.x));
    half skybox = depth > _HFSkyboxParam.x ? 1 : 0;

    half3 fogColor = 0;
    half fog = Fog(pmId, rayDir, dis, skybox, wsPos, fogColor);
    #if _EM
        UNITY_BRANCH
        if (pmBlend < 1)
        {
            half3 fogColorBlend = 0;
            half fogBlend = Fog(pmBlendId, rayDir, dis, skybox, wsPos, fogColorBlend);

            fog = fog * pmBlend;
            fogBlend = fogBlend * (1 - pmBlend);
            half fogSum = fog + fogBlend;
            fogColor = lerp(fogColorBlend, fogColor, saturate(fog / fogSum));
            fog = fogSum;
        }
    #endif
    
    // 环境亮度
    fogColor = EnvirLumChange(fogColor);

    return half4(fogColor, fog);
}

#endif
