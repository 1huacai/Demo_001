#ifndef YOUKIAHAIR_INCLUDED
#define YOUKIAHAIR_INCLUDED

#include "YoukiaCharacter.cginc"

sampler2D _AlbedoTex;

// Anisotropic
half4 _AnisoColor_0, _AnisoColor_1;
half _AnisoShift_0, _AnisoExpo_0, _AnisoShift_1, _AnisoExpo_1;

//------------------------------------------------------------
// fwd base

struct v2f_forward
{
    half4 pos : SV_POSITION;
    fixed4 uv : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    half3 TtoW[3] : TEXCOORD2;  
    half3 sh : TEXCOORD5;
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    half3 giColor : TEXCOORD8;

    #if _LOD1_HAIR
        YOUKIA_RIMLIGHT_DECLARE(9)
    #else
        YOUKIA_ATMOSPERE_DECLARE(9, 10)
        YOUKIA_HEIGHTFOG_DECLARE(13)
    #endif
    
    YOUKIA_LIGHTING_COORDS(11, 12)
    // eff
    CHARACTER_EFFECT_DECLARE(14)

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f_forward vert_forward(appdata_t v) 
{
    v2f_forward o;
    UNITY_INITIALIZE_OUTPUT(v2f_forward, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMetaMap);
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    half3 worldNormal = 0;
    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
    
    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

    #if _LOD1_HAIR
        YOUKIA_TRANSFER_RIMLIGHT(o, worldNormal);
    #else
        // height fog
        YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
        // atmosphere
        YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)
    #endif

    // eff
    half nv = saturate(dot(worldNormal, o.viewDir.xyz));
    CHARACTER_EFFECT_VERT(nv, o.effColor);

    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
    
    return o;
}

half4 frag_fwdColor(v2f_forward i, in out half nv, inout half3 normal, inout half shadowAtten)
{
    UNITY_SETUP_INSTANCE_ID(i);
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    half3 lightDir = i.lightDir;
    
    // tex
    // rgb：color, a: alpha
    fixed4 c = tex2D(_MainTex, i.uv.xy);
    // rg: normal, b: ao, a: 自发光
    fixed4 n = tex2D(_BumpMetaMap, i.uv.zw);
    
    fixed4 col = c * _Color;
    col.rgb = lerp(1, c.b, _AO) * _Color.rgb * tex2D(_AlbedoTex, i.uv.zw);
    half alpha = col.a;
    fixed3 abledo = col.rgb;

    half metallic = _Metallic;
    half smoothness = 1 - _Roughness;

    // normal
    normal = WorldNormal(n, i.TtoW);
    #if defined (UNITY_UV_STARTS_AT_TOP)
        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
    #endif

    // ao
    half3 ao = lerp(1, lerp(_AOColor.rgb, 1, n.b), _AO);

    // shadow
    YOUKIA_LIGHT_ATTENUATION_Rec(atten, i, i.worldPos.xyz, colShadow, _RecShadow);
    abledo *= ao;
    shadowAtten = atten;
    
    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    CharacterGIScale(gi, normal, lightDir, colShadow, ao);

    half oneMinusReflectivity;
    half3 specColor;
    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

    // Anisotropic
    fixed3 b = half3(i.TtoW[0].y, i.TtoW[1].y, i.TtoW[2].y);

    half anisoNoise1 = c.r;
    half anisoNoise2 = c.g;
    
    YoukiaAnisoData aniso = YoukiaAniso(b, _AnisoColor_0, _AnisoShift_0 * anisoNoise1, _AnisoExpo_0, _AnisoColor_1, _AnisoShift_1 * anisoNoise2, _AnisoExpo_1);

    // pbs
    col = BRDF_Hair_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten, aniso, c.rgb);
    
    #ifdef _SUB_LIGHT_C
        UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
        col += BRDF_Hair_PBS_SUB(col.rgb, specColor, normal, viewDir, brdfPreData, light, aniso, c.rgb);
    #endif

     #if _UNITY_RENDER
        // rim light
        col.rgb += CharacterRimLightFrag(normal, atten);
    #endif

    col.a = alpha;

    #if _LOD1_HAIR
        // rim light
        YOUKIA_RIMLIGHT(col, i, atten);
    #else
        // height fog
        YOUKIA_HEIGHTFOG(col, i)
        // atmosphere
        YOUKIA_ATMOSPHERE(col, i)
    #endif

    nv = brdfPreData.nv;

    return col;
}

fixed4 frag_forward(v2f_forward i, inout half3 normal, inout half shadow)
{
    half nv = 0;
    return frag_fwdColor(i, nv, normal, shadow);
}

//------------------------------------------------------------
// fwd add

struct v2f_add
{
    half4 pos : SV_POSITION;
    fixed4 uv : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    half3 TtoW[3] : TEXCOORD2;  
    half3 sh : TEXCOORD5;
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    half3 giColor : TEXCOORD8;
    UNITY_LIGHTING_COORDS(9, 10)
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f_add vert_add(appdata_t v) 
{
    v2f_add o;
    UNITY_INITIALIZE_OUTPUT(v2f_add,o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMetaMap);
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    half3 worldNormal = 0;
    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
    
    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

    return o;
}

fixed4 frag_add(v2f_add i) : SV_Target 
{
    UNITY_SETUP_INSTANCE_ID(i);
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    half3 lightDir = i.lightDir;

    // tex
    // rgb：color, a: alpha
    fixed4 c = tex2D(_MainTex, i.uv.xy);
    // rg: normal, b: ao, a: 自发光
    fixed4 n = tex2D(_BumpMetaMap, i.uv.zw);
    
    fixed4 col = c * _Color;
    col.rgb = lerp(1, c.b, _AO) * _Color.rgb * tex2D(_AlbedoTex, i.uv.zw);
    half alpha = col.a;
    fixed3 abledo = col.rgb;

    half metallic = _Metallic;
    half smoothness = 1 - _Roughness;

    // normal
    half3 normal = WorldNormal(n, i.TtoW);
    
    // light
    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
    fixed3 colShadow = atten;

    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    gi.indirect.specular *= colShadow;

    half oneMinusReflectivity;
    half3 specColor;
    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

    // Anisotropic
    YoukiaAnisoData aniso = (YoukiaAnisoData)0;
    fixed3 b = half3(i.TtoW[0].y, i.TtoW[1].y, i.TtoW[2].y);
    half anisoNoise1 = c.r;
    half anisoNoise2 = c.g;
    aniso = YoukiaAniso(b, _AnisoColor_0, _AnisoShift_0 + anisoNoise1, _AnisoExpo_0, _AnisoColor_1, _AnisoShift_1 + anisoNoise2, _AnisoExpo_1);

    // pbs
    col = BRDF_Hair_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten, aniso, c.rgb, 1);

    col.a = alpha;
    
    return col;
}

#endif