#ifndef YOUKIABIGMAP_INCLUDED
#define YOUKIABIGMAP_INCLUDED

#include "../Library/GPUSkinningLibrary.cginc"

// top tex
half _TopType;

// emission
half4 _ColorEmission;
half _EmissionStrength;

half _ReceiveShadow;

sampler2D _EmissionTex;

// uv speed
half4 _UVSpeed;

struct appdata_t 
{
    float4 vertex : POSITION;
    fixed2 texcoord : TEXCOORD0;
    #if defined(_UV2) ||  defined(_UV2_MASK)
        fixed2 texcoord2 : TEXCOORD1;
    #endif
    #ifdef _GPUAnimation
        float4 texcoord3 : TEXCOORD2;
    #endif
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//--------------------------------------------------------------------------------------------------
// base
struct v2fbase
{
    float4 pos : SV_POSITION;
    #if defined(_UVSPEED)
        float4 uv : TEXCOORD0;
    #else
        fixed4 uv : TEXCOORD0;
    #endif
    float4 worldPos : TEXCOORD1;
    #if defined(_BUMPTEXTURE)
        half3 TtoW[3] : TEXCOORD2;
    #else
        half3 worldNormal : TEXCOORD2;
    #endif
    half3 sh : TEXCOORD5;
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    half3 giColor : TEXCOORD8;

    YOUKIA_LIGHTING_COORDS(9, 10)
    // YOUKIA_HEIGHTFOG_DECLARE(11)
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fbase vertbase(appdata_t v) 
{
    v2fbase o;
    UNITY_INITIALIZE_OUTPUT(v2fbase,o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    half3 normal = v.normal;
    float4 vertex = v.vertex;
    half4 tangent = v.tangent;
    // 2 bones
    #ifdef _GPUAnimation
        vertex = AnimationSkinning(v.texcoord3, v.vertex, normal, tangent);
    #endif

    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #if defined(_UV2) || defined(_UV2_MASK)
        o.uv.zw = v.texcoord2;
    #else
        o.uv.zw = TRANSFORM_TEX(v.texcoord, _MetallicMap);
    #endif

    #if defined(_UVSPEED)
        o.uv.xy += _Time.x * _UVSpeed * _MainTex_ST.xy;
        o.uv.zw += _Time.x * _UVSpeed * _MetallicMap_ST.xy;
    #endif

    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

    half3 worldNormal = 0;
    #if defined(_BUMPTEXTURE)
        T2W(normal, tangent, o.TtoW, worldNormal);
    #else
        worldNormal = o.worldNormal = UnityObjectToWorldNormal(v.normal);
    #endif

    EMData emData;
    YoukiaEMLod(o.worldPos, emData);
    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor, emData);

    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
    // height fog
    // YOUKIA_TRANSFER_HEIGHTFOG_EM(o, 0, emData)

    return o;
}

FragmentOutput fragbase(v2fbase i)
{
    UNITY_SETUP_INSTANCE_ID(i);
    
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    half3 lightDir = i.lightDir;
    
    // tex
    // rgb: color, a: AO
    fixed4 c = tex2D(_MainTex, i.uv);

    #if defined(_UV2_MASK)
        fixed4 m_uv2 = tex2D(_MetallicMap, i.uv.zw);
    #elif defined(_UV2)
        // r: transparent, g: lighting, b: sss
        fixed4 m = tex2D(_MetallicMap, i.uv);
    #else
        // r: transparent, g: lighting, b: sss
        fixed4 m = tex2D(_MetallicMap, i.uv.zw);
    #endif
    
    fixed4 col;
    col.rgb = c.rgb * _Color.rgb;
    #if defined(_UV2_MASK)
        half alpha = c.a * _Color.a;
        clip(alpha - _Cutoff);
    #elif defined(_UV2)
        half alpha = m.r * _Color.a * i.uv.w;
    #else
        half alpha = m.r * _Color.a;
    #endif
    
    fixed3 albedo = col.rgb;

    half3 normal = 0;
    #if defined(_BUMPTEXTURE)
        // rg: normal, b: Metallic, a: Roughness
        fixed4 n = tex2D(_BumpMetaMap, i.uv);

        // 写固定值，防止美术修改乱掉
        _Roughness = 1;
        _Metallic = 1;
        half metallic = MetallicCalc(n.b);
        half smoothness = SmoothnessCalc(n.a);
        
        // normal
        _NormalStrength = 1;
        normal = UnpackNormalYoukia(n, _NormalStrength);
        normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
    #else
        half metallic = _Metallic;
        half smoothness = 1 - _Roughness;
        normal = i.worldNormal;
    #endif

    // ao
    #if defined(_UV2_MASK)
        half3 ao = AOCalc(m_uv2.r);
    #else
        half3 ao = AOCalc(c.a);
    #endif
    ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);
    
    // em
    EMData emData;
    YoukiaEM(worldPos, emData);

    // top tex
    #if defined(_UV2_MASK)
        #ifdef _TOPTEX
            half2 topUV = i.uv.zw;
            half topType = _TopType;

            #if _UNITY_RENDER
                YoukiaTopTex_9Tex(topType, topUV, m_uv2.g, albedo);
            #else
                TerrainSurface(worldPos, m_uv2.g, albedo);
            #endif
            
        #endif
    #endif

    #if defined (UNITY_UV_STARTS_AT_TOP)
        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor, emData);
    #endif

    // light
    YOUKIA_LIGHT_ATTENUATION_EM(atten, i, i.worldPos.xyz, colShadow, emData);
    colShadow = lerp(1, colShadow, _ReceiveShadow);

    albedo *= ao;
    UnityGI gi = GetUnityGI_bigmap(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor, emData);
    gi.indirect.diffuse *= ao;
    gi.indirect.specular *= ao;

    half oneMinusReflectivity;
    half3 specColor;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);
    // pbs
    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

    // sub light
    #ifdef _SUB_LIGHT_S
        UnityLight light = CreateUnityLight(YoukiaVSLSceneColor(emData), YoukiaVSLSceneRotation(emData));
        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, atten);
    #endif

    #if defined(_UV2_MASK)
        half4 emission = tex2D(_EmissionTex, i.uv.xy);
        col.rgb += emission.rgb * c.rgb * _Color.rgb * _EmissionStrength;
    #else
        col.rgb += m.g * _ColorEmission * c.rgb * _Color.rgb * _EmissionStrength;
    #endif

    col.a = alpha;

    // height fog
    // YOUKIA_HEIGHTFOG(col, i)

    col.rgb = BigMapFog(col.rgb, worldPos, emData);

    // lum
    col.rgb = SceneLumChange(col.rgb);
    
    return OutPutDefault(col);
}

//--------------------------------------------------------------------------------------------------
// add
struct v2fadd
{
    float4 pos : SV_POSITION;
    fixed4 uv : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    half3 worldNormal : TEXCOORD2;
    half3 sh : TEXCOORD3;
    UNITY_LIGHTING_COORDS(4, 5)
    fixed3 viewDir : TEXCOORD8;
    fixed3 lightDir : TEXCOORD9;

    half3 giColor : TEXCOORD10;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fadd vertadd(appdata_t v) 
{
    v2fadd o;
    UNITY_INITIALIZE_OUTPUT(v2fadd, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    half3 normal = v.normal;
    float4 vertex = v.vertex;
    half4 tangent = v.tangent;
    // 2 bones
    #ifdef _GPUAnimation
        vertex = AnimationSkinning(v.texcoord3, v.vertex, normal, tangent);
    #endif

    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #if defined(_UV2) || defined(_UV2_MASK)
        o.uv.zw = v.texcoord2;
    #else
        o.uv.zw = TRANSFORM_TEX(v.texcoord, _MetallicMap);
    #endif
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

    // tex
    // rgb: color, a: AO
    fixed4 c = tex2D(_MainTex, i.uv);
    // rg: normal, b: Metalli, a: Roughness
    fixed4 n = tex2D(_BumpMetaMap, i.uv);
    // r: transparent, g: lighting, b: sss
    #if defined(_UV2)
        fixed4 m = tex2D(_MetallicMap, i.uv);
    #else
        fixed4 m = tex2D(_MetallicMap, i.uv.zw);
    #endif

    fixed4 col;
    col.rgb = c.rgb * _Color.rgb;
    half alpha = m.r * _Color.a;
    fixed3 albedo = col.rgb;

    #if defined(_UV2)
        half height = i.uv.w;
        alpha *= height;
    #endif

    #if defined(_BUMPTEXTURE)
        // 写固定值，防止美术修改乱掉
        _Roughness = 1;
        _Metallic = 1;
        half metallic = MetallicCalc(n.b);
        half smoothness = SmoothnessCalc(n.a);
    #else
        half metallic = _Metallic;
        half smoothness = 1 - _Roughness;
    #endif

    // normal
    half3 normal = i.worldNormal;

    // ao
    half3 ao = AOCalc(c.a);
    ao = lerp(1, lerp(_AOColor.rgb, 1, c.a), _AO);

    // light
    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
    fixed3 colShadow = atten;
    albedo *= ao;
    
    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    gi.indirect.specular *= colShadow;
    
    half oneMinusReflectivity;
    half3 specColor;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    gi.indirect.specular *= colShadow;
    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);
    
    // pbs
    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

    col.a = alpha;

    return col;
}

#endif