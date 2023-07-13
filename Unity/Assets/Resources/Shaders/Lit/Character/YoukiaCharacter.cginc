#ifndef YOUKIACHARACTER_INCLUDED
#define YOUKIACHARACTER_INCLUDED

half _cull;

struct appdata_t 
{
    half4 vertex : POSITION;
    half2 texcoord : TEXCOORD0;
    #if defined (_UV2)
        half2 texcoord1 : TEXCOORD1;
    #endif
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct appdata_gpu
{
    half4 vertex : POSITION;
    half2 texcoord : TEXCOORD0;
    #if defined (_GPUAnimation)
        float4 uv2 : TEXCOORD1;
        float4 uv3 : TEXCOORD2;
    #endif
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//--------------------------------------------------------------------------------------------------
// tangent 2 world
// aniso
inline void T2WAniso(half3 normal, half4 tangent, half3 viewDir, in out half4 TtoW[3], out half3 worldNormal)
{
    half3 worldTangent, worldBinormal = 0;
    T2W(normal, tangent, worldNormal, worldTangent, worldBinormal);
    // Anisotropic
    fixed3 t = 0;
    Unity_RotateAboutFwdAxis_Degrees_float(worldTangent, _AnisoDir, t);

    T2W_Vec2Array(worldTangent, worldBinormal, worldNormal, TtoW, t);
}

inline half3 Binormal(half3 worldNormal, half4 TtoW[3])
{
    half3 t = half3(TtoW[0].w, TtoW[1].w, TtoW[2].w);
    half3 b = normalize(cross(worldNormal, t));

    return b;
}

//--------------------------------------------------------------------------------------------------
// unpack normal -> world normal
// cull off 时重新计算世界法线方向
inline half3 WorldNormalCullOff(half4 n, half4 TtoW[3], half3 viewDir)
{
    half3 worldNormal = WorldNormal(n, TtoW);
    half3 reverseNormal = dot(viewDir, worldNormal) < 0 ? -worldNormal : worldNormal;
    worldNormal = lerp(reverseNormal, worldNormal, saturate(_cull));

    return worldNormal;
}

inline half3 WorldNormalCullOff(half4 n, half3 TtoW[3], half3 viewDir)
{
    half3 worldNormal = WorldNormal(n, TtoW);
    half3 reverseNormal = dot(viewDir, worldNormal) < 0 ? -worldNormal : worldNormal;
    worldNormal = lerp(reverseNormal, worldNormal, saturate(_cull));

    return worldNormal;
}

//--------------------------------------------------------------------------------------------------
// character gi
inline void CharacterGIScale(in out UnityGI gi, half nl, half3 colShadow, half3 ao, half giScale)
{
    // gi 暗部增强
    gi.indirect.diffuse = CharacterGIScale(nl, colShadow.r, gi.indirect.diffuse, giScale);
    // character light color
    gi.light.color = CharacterColorLerp();
    gi.indirect.diffuse = CharacterGILerp(gi.indirect.diffuse);
    gi.indirect.diffuse *= ao;
    gi.indirect.specular *= ao;
}

inline void CharacterGIScale(in out UnityGI gi, half3 normal, half3 lightDir, half3 colShadow, half3 ao, half giScale)
{
    // gi 暗部增强
    gi.indirect.diffuse = CharacterGIScale(normal, lightDir, colShadow.r, gi.indirect.diffuse, giScale);
    // character light color
    gi.light.color = CharacterColorLerp();
    gi.indirect.diffuse = CharacterGILerp(gi.indirect.diffuse);
    gi.indirect.diffuse *= ao;
    gi.indirect.specular *= ao;
}

inline void CharacterGIScale(in out UnityGI gi, half3 normal, half3 lightDir, half3 colShadow, half3 ao)
{
    CharacterGIScale(gi, normal, lightDir, colShadow, ao, _gCharacterGIScale);
}

inline void CharacterGIScale(in out UnityGI gi, half nl, half3 colShadow, half3 ao)
{
    CharacterGIScale(gi, nl, colShadow, ao, _gCharacterGIScale);
}


//--------------------------------------------------------------------------------------------------
// emission
half4 _ColorEmission;
half _EmissionBreath;
inline half3 Emission(half3 color, half3 c, half mask)
{
    color += c * mask * _ColorEmission;
    return color;
}

inline half3 EmissionBreath(half3 color, half3 c, half mask)
{
    half3 emission = c * mask * _ColorEmission;
    emission *= cos(_Time.y * _EmissionBreath) * 0.5 + 0.5;

    color += emission;
    return color;
}

//--------------------------------------------------------------------------------------------------
// rim
half4 _RimColor;
half _RimStrength, _RimRange;
inline half3 Rim(half3 color, half nv, half mask = 1)
{
    half rim = 1.0 - nv;
    rim = saturate(pow(rim, _RimRange));
    color += rim * _RimColor * _RimStrength * mask;

    return color;
}

//--------------------------------------------------------------------------------------------------
// rim light
inline half3 CharacterRimLight(half3 normal, half3 viewDir)
{
    half3 reverseNormal = dot(viewDir, normal) < 0 ? -normal : normal;
    normal = lerp(reverseNormal, normal, saturate(_cull));
    half nl = saturate(dot(normal, normalize(_gVSLFwdVec_C_Rim)));
    nl = pow(nl, _gVSLFwdVec_C_Rim.w);
    return max(0, nl * _gVSLColor_C_Rim.rgb);
}

inline half3 CharacterRimLight(half3 rimLight, half shadow)
{
    return rimLight * shadow;
}

inline half3 CharacterRimLightFrag(half3 normal, half shadow)
{
    half nl = saturate(dot(normal, normalize(_gVSLFwdVec_C_Rim)));
    nl = pow(nl, _gVSLFwdVec_C_Rim.w);
    return max(0, nl * _gVSLColor_C_Rim.rgb) * shadow;
}

#define YOUKIA_RIMLIGHT_DECLARE(idx1) \
    half3 rimLight : TEXCOORD##idx1;

#define YOUKIA_TRANSFER_RIMLIGHT(o, worldNormal) \
    o.rimLight = CharacterRimLight(worldNormal, o.viewDir.rgb)

#define YOUKIA_RIMLIGHT(col, i, atten) \
    col.rgb += CharacterRimLight(i.rimLight, atten)

//--------------------------------------------------------------------------------------------------
// flow map
inline half FlowMapSparkle(half2 uv, half2 flowUV, float phase, half2 coordCenter)
{
    half2 coord = (uv + flowUV * phase) * _FlowMapScale;
    half2 coordFloor = floor(coord);
    coord -= coordFloor;
    half sparkle = 1 - saturate(length(coord - coordCenter) / _FlowMapRadius);

    return sparkle;
}

inline half3 FlowMap(half3 albedo, half2 uv)
{
    half3 flowMap = tex2D(_FlowMap, uv);
    half2 flowUV = (flowMap.rg * 2 - 1) * _FlowMapStrength;

    float time = _Time.y * _FlowMapSpeed;
    float phase0 = frac(time * 0.5f + 0.5f);
    float phase1 = frac(time * 0.5f + 1.0f);
    
    float flowlerp = saturate(abs((0.5f - phase0) / 0.5f));

    half2 coordCenter = 0.5f;
    uv += half2(_FlowMapUVOffsetU, _FlowMapUVOffsetV);
    half sparkle0 = FlowMapSparkle(uv, flowUV, phase0, coordCenter);
    half sparkle1 = FlowMapSparkle(uv, flowUV, phase1, coordCenter);

    half3 sparkle = lerp(sparkle0, sparkle1, flowlerp) * albedo * _FlowColor;

    return sparkle * saturate((1 - flowMap.b));
}

#endif