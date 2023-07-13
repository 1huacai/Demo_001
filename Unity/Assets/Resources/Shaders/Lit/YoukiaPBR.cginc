#ifndef YOUKIAPBR_INCLUDED
#define YOUKIAPBR_INCLUDED

// emission
half4 _ColorEmission;
half _EmissionStrength;

// top tex
half _TopTex;
half _ReceiveShadow;

// uv2 fade
half _FadeScale;
half _UV2Fade;

struct appdata_t 
{
    float4 vertex : POSITION;
    fixed2 texcoord : TEXCOORD0;
    #if defined(_UV2) || defined(_UV2_FADE)
        fixed2 texcoord2 : TEXCOORD1;
    #endif
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    // half3 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline half UV2Fade(half alpha, half height)
{
    height = pow(height, _FadeScale);
    height = lerp(1, height, _UV2Fade);
    return saturate(alpha * height);
}

//--------------------------------------------------------------------------------------------------
// base
struct v2fbase
{
    float4 pos : SV_POSITION;
    #if defined(_UV2) || defined(_UV2_FADE)
        fixed4 uv : TEXCOORD0;
    #else
        fixed2 uv : TEXCOORD0;
    #endif
    float4 worldPos : TEXCOORD1;
    half3 TtoW[3] : TEXCOORD2;
    half3 sh : TEXCOORD5;
    fixed3 viewDir : TEXCOORD6;
    fixed3 lightDir : TEXCOORD7;
    half3 giColor : TEXCOORD8;

    YOUKIA_ATMOSPERE_DECLARE(9, 10)
    YOUKIA_LIGHTING_COORDS(11, 12)
    YOUKIA_HEIGHTFOG_DECLARE(13)
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fbase vertbase(appdata_t v) 
{
    v2fbase o;
    UNITY_INITIALIZE_OUTPUT(v2fbase,o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #if defined(_UV2)
        o.uv.zw = TRANSFORM_TEX(v.texcoord2.xy, _MainTex);
    #endif
    #if defined(_UV2_FADE)
        o.uv.zw = v.texcoord2;
    #endif
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

    fixed3 worldNormal;
    T2W(v.normal, v.tangent, o.TtoW, worldNormal);

    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
    
    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
    // height fog
    YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
    // atmosphere
    YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

    return o;
}

FragmentOutput fragbase(v2fbase i) 
{
    UNITY_SETUP_INSTANCE_ID(i);
    
    float3 worldPos = i.worldPos;
    fixed3 viewDir = i.viewDir;
    half3 lightDir = i.lightDir;
    fixed3 worldNormal = fixed3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
    
    // tex
    // rgb: color, a: AO
    fixed4 c = tex2D(_MainTex, i.uv);
    // rg: normal, b: Metallic, a: Roughness
    fixed4 n = tex2D(_BumpMetaMap, i.uv);
    // r: transparent, g: lighting, b: sss
    fixed4 m = tex2D(_MetallicMap, i.uv);
    #if defined(_UV2)
        fixed4 m_uv2 = tex2D(_MetallicMap, i.uv.zw);
    #endif

    fixed4 col;
    col.rgb = c.rgb * _Color.rgb;
    half alpha = m.r * _Color.a;
    #if defined(_ALPHACUT)
        clip(alpha - _Cutoff);
    #endif
    #if defined(_UV2_FADE)
        alpha = UV2Fade(alpha, i.uv.w);

        // dissolve
        #if _USE_DISSOLVE
            half dissolveAlpha = lerp(i.uv.w, 1 - i.uv.w, _DissolveReverse);
            half dissolveStrength = clamp(_DissolveStrength - dissolveAlpha, 0.0f, 1.1f);
            half4 dissolve = Dissolve(worldPos.xz * _DissolveTexScale, dissolveStrength, _DissolveNoiseStrength);
            col.rgb = lerp(col.rgb, dissolve.rgb, dissolve.a);
        #endif

    #endif
    fixed3 albedo = col.rgb;

    // 写固定值，防止美术修改乱掉
    _Roughness = 1;
    _Metallic = 1;
    half metallic = MetallicCalc(n.b);
    half smoothness = SmoothnessCalc(n.a);

    // normal
    _NormalStrength = 1;
    fixed3 normal = UnpackNormalYoukia(n, _NormalStrength);

    // ao
    // ao 矫正
    #if defined(_UV2)
        half3 ao = lerp(1, lerp(_AOColor.rgb, 1, m_uv2.a), _AO);
        half3 aoEnvir = lerp(1, lerp(_AOColor.rgb, 1, m_uv2.a), _AOEnvir);
    #else
        half3 ao = AOCalc(c.a);
        ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);
    #endif

     #ifdef _TOPTEX
     UNITY_BRANCH
     if (_TopTex > 0)
     {
    //  fixed3 wsNormal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
    //  YoukiaTopTex(worldPos, wsNormal, albedo, normal, metallic, smoothness, c.a, _Metallic, _Roughness);
        YoukiaTopTex(worldPos, worldNormal, albedo, normal, c.a);
     }
     #endif

    // multi top tex
    //#ifdef _TOPTEX
    //UNITY_BRANCH
    //if(_TopType > 0)
    //{
        // fixed3 wsNormal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
        //half3 wsNormal = worldNormal;
        //YoukiaTopTex(worldPos, wsNormal, albedo, normal, metallic, smoothness, c.a, _Metallic, _Roughness, _TopType);
    //}
    //#endif

    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

    #if defined (UNITY_UV_STARTS_AT_TOP)
        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
    #endif

    // light
    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
    colShadow = lerp(1, colShadow, _ReceiveShadow);
    atten = lerp(1, atten, _ReceiveShadow);
    albedo *= ao;

    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    #if defined(_UV2)
        gi.indirect.diffuse *= aoEnvir;
        gi.indirect.specular *= aoEnvir;
    #else
        gi.indirect.diffuse *= ao;
        gi.indirect.specular *= ao;
    #endif

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
        UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, atten);
    #endif

    // #if defined(_EMISSION_ENBALE)
        col.rgb += m.g * _ColorEmission * c.rgb * _Color.rgb * _EmissionStrength;
    // #else
    //     #if _EMISSION
    //         col.rgb += m.g * _ColorEmission * c.rgb * _Color.rgb * _EmissionStrength;
    //     #endif
    // #endif

    col.a = alpha;

    // height fog
    YOUKIA_HEIGHTFOG(col, i)
    // atmosphere
    YOUKIA_ATMOSPHERE(col, i)

    // lum
    col.rgb = SceneLumChange(col.rgb);
    
    return OutPutDefault(col);
}

//--------------------------------------------------------------------------------------------------
// add
struct v2fadd
{
    float4 pos : SV_POSITION;
    #if defined(_UV2_FADE)
        fixed4 uv : TEXCOORD0;
    #else
        fixed2 uv : TEXCOORD0;
    #endif
    float4 worldPos : TEXCOORD1;
    #if defined(UNITY_UV_STARTS_AT_TOP)
        half3 TtoW[3] : TEXCOORD2;
    #else
        half3 worldNormal : TEXCOORD2;
    #endif
    half3 sh : TEXCOORD5;

    UNITY_LIGHTING_COORDS(6, 7)

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

    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #if defined(_UV2_FADE)
        o.uv.zw = v.texcoord2;
    #endif
    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
    
    #if defined(UNITY_UV_STARTS_AT_TOP)
        half3 worldNormal = 0;
        T2W(v.normal, v.tangent, o.TtoW, worldNormal);
    #else
        half3 worldNormal = o.worldNormal = UnityObjectToWorldNormal(v.normal);  
    #endif
    
    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

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
    fixed4 m = tex2D(_MetallicMap, i.uv);

    fixed4 col;
    col.rgb = c.rgb * _Color.rgb;
    half alpha = m.r * _Color.a;
    #if defined(_UV2_FADE)
        alpha = UV2Fade(alpha, i.uv.w);
    #endif
    fixed3 albedo = col.rgb;
    
    // gamma correct
    // 写固定值，防止美术修改乱掉
    _Roughness = 1;
    _Metallic = 1;
    half metallic = MetallicCalc(n.b);
    half smoothness = SmoothnessCalc(n.a);

    // normal
    fixed3 worldNormal;
    #if defined(UNITY_UV_STARTS_AT_TOP)
        worldNormal = fixed3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
        _NormalStrength = 1;
        fixed3 normal = UnpackNormalYoukia(n, _NormalStrength);
    #else
        fixed3 normal = worldNormal = i.worldNormal;
    #endif
    
    // ao
    half3 ao = AOCalc(c.a);
    ao = lerp(1, lerp(_AOColor.rgb, 1, c.a), _AO);

    // light
    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

    fixed3 colShadow = atten;
    albedo *= ao;

    // multi top tex
    //#ifdef _TOPTEX
    //UNITY_BRANCH
    //if(_TopType > 0)
    //{
    //    YoukiaTopTex(worldPos, worldNormal, albedo, normal, metallic, smoothness, c.a, _Metallic, _Roughness, _TopType);
    //}
        //YoukiaTopTex(worldPos, worldNormal, albedo, ao);
    //#endif

    #if defined(UNITY_UV_STARTS_AT_TOP)
        normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
    #endif
    
    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
    gi.indirect.diffuse *= colShadow;
    gi.indirect.specular *= colShadow;
    
    half oneMinusReflectivity;
    half3 specColor;
    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    // brdf pre
    BRDFPreData brdfPreData = (BRDFPreData)0;
    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);
    
    // pbs
    // #if defined(UNITY_UV_STARTS_AT_TOP)
        col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
    // #else
    //     col = BRDF_Unity_PBS_Add(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
    // #endif

    col.a = alpha;

    return col;
}
#endif