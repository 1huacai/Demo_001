#ifndef YOUKIACHARACTERLOD2_INCLUDED
#define YOUKIACHARACTERLOD2_INCLUDED

#include "YoukiaCharacter.cginc"
#include "YoukiaHair.cginc"
#include "../../Library/YoukiaEffect.cginc"

half4 _UVSpeed;

//--------------------------------------------------------------------------------------------------
// base
struct v2fbase
{
    float4 pos : SV_POSITION;
    #if defined(_UVSPEED)
        float2 uv : TEXCOORD0;
    #else
        half2 uv : TEXCOORD0;
    #endif
    float4 worldPos : TEXCOORD1;
    #if defined (_LOD2_UBER)
        half4 TtoW[3] : TEXCOORD2;
    #else
        half3 TtoW[3] : TEXCOORD2;
    #endif
    #if _SCATTERING
        half4 sh : TEXCOORD5;
    #else
        half3 sh : TEXCOORD5;
    #endif
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    // xyz: gi color; w: vert r
    half4 giColor : TEXCOORD8;
    // eff
    CHARACTER_EFFECT_DECLARE(9)

    YOUKIA_ATMOSPERE_DECLARE(10, 11)
    YOUKIA_LIGHTING_COORDS(12, 13)
    YOUKIA_HEIGHTFOG_DECLARE(14)

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fbase vertbase(appdata_gpu v) 
{
    v2fbase o;
    UNITY_INITIALIZE_OUTPUT(v2fbase,o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    fixed3 normal = v.normal;
    half4 vertex = v.vertex;
    half4 tangent = v.tangent;

    #ifdef _GPUAnimation
        #if defined (_GPU_4Bone)
            // 4bone
            vertex = AnimationSkinningFourBoneWeight(v.uv2, v.uv3, v.vertex, normal, tangent);
        #else
            // 2 bone
            vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
        #endif
    #endif
    
    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

    #if defined(_UVSPEED)
        o.uv.xy += _UVSpeed * _Time.x;
    #endif
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    #if defined (_LOD2_UBER)
        half3 worldNormal, worldTangent, worldBinormal = 0;
        T2W(normal, tangent, worldNormal, worldTangent, worldBinormal);
        half3 t = 0;
        #if _ANISOTROPIC
            Unity_RotateAboutFwdAxis_Degrees_float(worldTangent, _AnisoDir, t);
        #endif

        T2W_Vec2Array(worldTangent, worldBinormal, worldNormal, o.TtoW, t);
    #else
        half3 worldNormal = 0;
        T2W(normal, tangent, o.TtoW, worldNormal);
    #endif

    #if _SCATTERING
        o.sh.w = dot(worldNormal, o.lightDir) * 0.5 + 0.5;
        // o.sh.w = -o.sh.w;
    #endif

    YoukiaVertSH(worldNormal, o.worldPos, o.sh.xyz, o.giColor.rgb);
    o.giColor.w = v.color.r;

    half nv = saturate(dot(worldNormal, o.viewDir.xyz));
    CHARACTER_EFFECT_VERT(nv, o.effColor);

    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
    // height fog
    YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
    // atmosphere
    YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

    return o;
}

half4 fragbase(v2fbase i, out half nv) 
{
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    fixed3 lightDir = i.lightDir;
    half2 uv = i.uv.xy;

    #if _MonsterLod2
        // death dissolve
        half4 dissolveColor = DeathDissolve(uv);
    #endif

    // tex
    // rgb：color, a: alpha
    fixed4 c = tex2D(_MainTex, uv);
    // rg: normal, b: ao, a: 自发光
    fixed4 n = tex2D(_BumpMetaMap, uv);
    // r: m, g: r
    fixed4 m = tex2D(_MetallicMap, uv);

    #if defined (_LOD2_UBER)
        half sss = m.b;
        half a = saturate(ceil(m.a));
        half curvature = sss;
    #endif
    
    fixed4 col = c * _Color;
    half alpha = col.a;
    fixed3 albedo = col.rgb;

    #if defined (_LOD2_UBER)
        clip(alpha - _Cutoff);
    #endif

    half metallic = MetallicCalc(m.r);
    half smoothness = SmoothnessCalc(m.g);
    // normal
    half3 normal = WorldNormal(n, i.TtoW);

    // shadow
    YOUKIA_LIGHT_ATTENUATION_Rec(atten, i, i.worldPos.xyz, colShadow, _RecShadow);

    // ao
    half occlusion = saturate(pow(n.b, _AOCorrect));
    half3 ao = lerp(1, lerp(_AOColor.rgb, 1, occlusion), _AO);

    // skin ao
    half giScale = _gCharacterGIScale;
    #if defined (_LOD2_UBER)
        half sssFlg = FastSign(sss);
        smoothness = lerp(smoothness, SmoothnessSpecCalc(1 - m.g), sssFlg);
        giScale = lerp(_gCharacterGIScale, _gCharacterSkinGIScale, sssFlg);
        #if _SCATTERING
            UNITY_BRANCH
            if (sss > 0)
            {
                ao = SkinAO(occlusion, _AO, normal, viewDir, sss);
                ao = lerp(ao * 0.4f + 0.6, ao, i.sh.w * atten);
            }
        #endif
    #endif

    #if defined (UNITY_UV_STARTS_AT_TOP)
        i.sh.rgb += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor.rgb);
    #endif

    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    CharacterGIScale(gi, normal, lightDir, colShadow, ao, giScale);

    #if defined (_LOD2_UBER)
        YoukiaSSSData SSS = YoukiaSSS();

        // Anisotropic
        half3 b = 0;
        #if _ANISOTROPIC
            b = Binormal(normal, i.TtoW);
        #endif
    #endif

    // pbs
    half oneMinusReflectivity;
    half3 specColor = 0;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);
    
    #if defined (_LOD2_UBER)
        col = CharacterBRDF(a, albedo, specColor, normal, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten, SSS, b, sss, curvature);

        // sub light
        #ifdef _SUB_LIGHT_C
            UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
            col += CharacterBRDF_SUB(a, albedo, specColor, normal, normal, viewDir, brdfPreData, light, atten, SSS, b, sss, curvature);
        #endif

    #else
        col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

        // sub light
        #ifdef _SUB_LIGHT_C
            UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
            col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, 1);
        #endif
    #endif
    
    #if _UNITY_RENDER
        // rim light
        col.rgb += CharacterRimLightFrag(normal, atten);
    #endif

    #if _MonsterLod2
        col.rgb = EmissionBreath(col.rgb, c.rgb, n.a);
    #else
        col.rgb = Emission(col.rgb, c.rgb, n.a);
    #endif

    // height fog
    YOUKIA_HEIGHTFOG(col, i)
    // atmosphere
    YOUKIA_ATMOSPHERE(col, i)

    nv = brdfPreData.nv;
    col.a = alpha;

    #if _MonsterLod2
    // dissolve
    col.rgb = lerp(col.rgb, dissolveColor.rgb, dissolveColor.a);
    #endif
    
    return col;
}

//--------------------------------------------------------------------------------------------------
// add
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

v2fadd vertadd(appdata_gpu v) 
{
    v2fadd o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_OUTPUT(v2fadd, o);

    fixed3 normal = v.normal;
    half4 vertex = v.vertex;
    half4 tangent = v.tangent;

    #ifdef _GPUAnimation
        #if defined (_GPU_4Bone)
            // 4bone
            vertex = AnimationSkinningFourBoneWeight(v.uv2, v.uv3, v.vertex, normal, tangent);
        #else
            // 2 bone
            vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
        #endif
    #endif

    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    o.worldNormal = UnityObjectToWorldNormal(normal);  
    
    o.sh = ShadeSHPerVertex(o.worldNormal, o.sh);
    o.sh += YoukiaGI_IndirectDiffuse(o.worldNormal, o.worldPos, o.giColor);

    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

    return o;
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

//--------------------------------------------------------------------------------------------------
// shadow
half _ShadowAlpha;

half4 fragShadowCharacter(v2fShadow i) : COLOR 
{
    half4 color = tex2D(_MainTex, TRANSFORM_TEX(i.tex, _MainTex));
    clip(color.a * _Color.a - _Cutoff - _ShadowAlpha);
    // clip(color.a * _Color.a - _ShadowAlpha);
    SHADOW_CASTER_FRAGMENT(i)
}

#endif