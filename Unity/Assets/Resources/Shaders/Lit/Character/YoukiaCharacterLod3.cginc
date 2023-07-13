#ifndef YOUKIACHARACTERLOD3_INCLUDED
#define YOUKIACHARACTERLOD3_INCLUDED

#include "../../Library/GPUSkinningLibrary.cginc"
#include "YoukiaCharacter.cginc"

struct v2fbase
{
    half4 pos : SV_POSITION;
    fixed2 uv : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    #if defined (_LOD3_BUMP)
        half3 TtoW[3] : TEXCOORD2;  
    #else
        half3 worldNormal : TEXCOORD2;
    #endif
    // xyz: sh, w: nl
    half4 sh : TEXCOORD5;
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    // xyz: sh, w: sub light nl
    half4 giColor : TEXCOORD8;
    // eff
    CHARACTER_EFFECT_DECLARE(9)

    YOUKIA_LIGHTING_COORDS(10, 11)
    // YOUKIA_ATMOSPERE_DECLARE(9, 10)
    // YOUKIA_HEIGHTFOG_DECLARE(13)

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fadd
{
    half4 pos : SV_POSITION;
    fixed2 uv : TEXCOORD0;
    float4 worldPos : TEXCOORD1;

    half3 worldNormal : TEXCOORD2;
    half3 sh : TEXCOORD3;

    UNITY_LIGHTING_COORDS(4, 5)

    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;

    half3 giColor : TEXCOORD8;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//--------------------------------------------------------------------------------------------------
// vert
v2fbase vertbase(appdata_gpu v) 
{
    v2fbase o;
    UNITY_INITIALIZE_OUTPUT(v2fbase,o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    fixed3 normal = v.normal;
    half4 vertex = v.vertex;
    half4 tangent = v.tangent;

    // 2 bones
    #ifdef _GPUAnimation
        vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
    #endif

    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    #if defined (_LOD3_BUMP)
        half3 worldNormal = 0;
        T2W(normal, tangent, o.TtoW, worldNormal);
    #else
        half3 worldNormal = o.worldNormal = UnityObjectToWorldNormal(normal);  
    #endif
    
    EMData emData;
    YoukiaEMLod(o.worldPos, emData);
    YoukiaVertSH(worldNormal, o.worldPos, o.sh.rgb, o.giColor.rgb, emData);
    o.sh.w = saturate(dot(worldNormal, o.lightDir));
    o.giColor.w = saturate(dot(worldNormal, _gVSLFwdVec_C));

    // eff
    half nv = saturate(dot(worldNormal, o.viewDir.xyz));
    CHARACTER_EFFECT_VERT(nv, o.effColor);

    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
    // height fog
    // YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
    // atmosphere
    // YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

    return o;
}

v2fadd vertadd(appdata_gpu v) 
{
    v2fadd o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_OUTPUT(v2fadd,o);

    fixed3 normal = v.normal;
    half4 vertex = v.vertex;
    half4 tangent = v.tangent;

    // 2 bones
    #ifdef _GPUAnimation
        vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
    #endif

    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    o.worldNormal = UnityObjectToWorldNormal(normal);  
    
    o.sh = ShadeSHPerVertex(o.worldNormal, o.sh);
    o.sh += YoukiaGI_IndirectDiffuse(o.worldNormal, o.worldPos, o.giColor.rgb);

    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

    return o;
}

//--------------------------------------------------------------------------------------------------
// frag
void fragbase(v2fbase i, in out half4 col, out half nv)
{
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    fixed3 lightDir = i.lightDir;
    half2 uv = i.uv.xy;
    half nl = i.sh.w;
    half nl_sub = i.giColor.w;

    // tex
    // rgb：color, a: alpha
    fixed4 c = tex2D(_MainTex, uv);
    #if defined (_LOD3_BUMP)
        // rg: normal, b: ao, a: 自发光
        fixed4 n = tex2D(_BumpMetaMap, uv);
    #endif
    // r: m, g: r
    fixed4 m = tex2D(_MetallicMap, uv);
    
    col = c * _Color;
    half alpha = col.a;
    fixed3 albedo = col.rgb;

    #if defined (_LOD3_BUMP)
        clip(alpha - _Cutoff);
    #endif

    half metallic = MetallicCalc(m.r);
    half smoothness = SmoothnessCalc(m.g);
    
    // normal
    #if defined (_LOD3_BUMP)
        // ao
        half3 ao = OcclusionCalc(n.b);
        half3 normal = WorldNormal(n, i.TtoW);
    #else
        half3 ao = 1;
        half3 normal = i.worldNormal;
    #endif
    
    // em
    EMData emData;
    YoukiaEM(worldPos, emData);
    #if defined (UNITY_UV_STARTS_AT_TOP)
        i.sh.rgb += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor.rgb, emData);
    #endif

    // shadow
    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh.rgb, i.giColor.rgb, emData);
    CharacterGIScale(gi, nl, colShadow, ao);

    // pbs
    half oneMinusReflectivity;
    half3 specColor = 0;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

    #if defined (_LOD3_BUMP)
        col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
    #else
        col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, nl, colShadow, atten);
    #endif

    // sub light
    #ifdef _SUB_LIGHT_C
        UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, nl_sub, atten);
    #endif

    #if defined (_LOD3_BUMP)
        // emission
        col.rgb = EmissionBreath(col.rgb, c.rgb, n.a);
    #else
        col.rgb = Emission(col.rgb, c.rgb, m.b);
    #endif

    // height fog
    // YOUKIA_HEIGHTFOG(col, i)
    // atmosphere
    // YOUKIA_ATMOSPHERE(col, i)

    col.rgb = BigMapFog(col.rgb, worldPos, emData);

    nv = brdfPreData.nv;

    col.a = alpha;
}

FragmentOutput fragbase(v2fbase i) 
{
    UNITY_SETUP_INSTANCE_ID(i);

    half4 col = 0;
    half nv;
    fragbase(i, col, nv);

    #if _RIMEFFECT
        // rim
        col.rgb = Rim(col.rgb, nv);
    #endif

    // effect
    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

    return OutPutCharacter(col);
}

FragmentOutput fragbase_monster(v2fbase i) 
{
    UNITY_SETUP_INSTANCE_ID(i);

    half4 col = 0;
    half nv;
    fragbase(i, col, nv);
    
    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

    return OutPutCharacter(col);
}

FragmentOutput fragbase_monster_bump(v2fbase i) 
{
    UNITY_SETUP_INSTANCE_ID(i);

    half4 col = 0;
    half nv;
    fragbase(i, col, nv);

    // rim
    col.rgb = Rim(col.rgb, nv);

    // eff
    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

    return OutPutCharacter(col);
}

fixed4 fragadd(v2fadd i) : SV_Target 
{
    UNITY_SETUP_INSTANCE_ID(i);
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    half3 lightDir = i.lightDir;
    half2 uv = i.uv.xy;

    // tex
    // rgb：color, a: alpha
    fixed4 c = tex2D(_MainTex, uv);
    // r: m, g: r
    fixed4 m = tex2D(_MetallicMap, uv);
    
    fixed4 col = c * _Color;
    half alpha = col.a;
    fixed3 albedo = col.rgb;

    half metallic = MetallicCalc(m.r);
    half smoothness = SmoothnessCalc(m.g);

    half3 normal = i.worldNormal;
    
    // light
    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
    fixed3 colShadow = atten;

    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    gi.indirect.specular *= colShadow;

    //sss
    YoukiaSSSData SSS = YoukiaSSS();

    // pbs
    half oneMinusReflectivity;
    half3 specColor;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

    col = BRDF_Unity_PBS_Add(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

    col.a = alpha;

    return col;
}

// shadow caster
half4 fragShadowCharacter(v2fShadow i) : COLOR 
{
    half4 color = tex2D(_MainTex, TRANSFORM_TEX(i.tex, _MainTex));
    clip(color.a * _Color.a - _Cutoff);
    SHADOW_CASTER_FRAGMENT(i)
}
#endif