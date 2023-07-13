//@@@DynamicShaderInfoStart
//大地图植被Shader。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/BigMap/YoukiaPlant-BigMap" 
{
    Properties 
    {
        _Color("颜色(rgb, a: alpha)", Color) = (1, 1, 1, 1)
        _MainTex("纹理", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
        [HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse 阈值", Range(0, 0.5)) = 0
        _DiffPow ("Diffuse 衰减", Range(0, 4)) = 0.5
        [HDR]_DiffColorBright ("Diffuse 亮部颜色", Color) = (1, 1, 1, 1)
        [HDR]_DiffColorDark ("Diffuse 暗部颜色", Color) = (0, 0, 0, 1)
        [Header(Shadow)]
        _ShadowColor ("阴影颜色", Color) = (0, 0, 0, 1)
        [Toggle] _ReceiveShadow("是否接受阴影", Float) = 1
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        [Header(SSS)]
        [HDR]_PlantSSSColor ("植被透光color", Color) = (0.15, 0.15, 0.0)
        _PlantSSSDistortion ("植被透光扰动", Range(0, 1)) = 0.5
        _PlantSSSPow ("植被透光范围", Range(0, 10)) = 1
        _PlantSSSScale ("植被透光系数", Range(-1, 1)) = 1
    }
    SubShader 
    {
        Tags { "RenderType" = "Opaque" "Queue"="Geometry" "ShadowType" = "ST_Tree" }

        CGINCLUDE
            #pragma multi_compile_instancing
            #pragma target 2.0

            #define _DISABLE_GISPEC 1
            #define _BIGMAPGI 1

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/YoukiaEaryZ.cginc"
            #include "../Library/YoukiaEffect.cginc"
            
            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK 

            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half3 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        ENDCG

        Pass
        {
            Name "EarlyZ"
            // ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
                #pragma vertex vertEaryZBigmapPlant
                #pragma fragment fragEaryZ
                #pragma fragmentoption ARB_precision_hint_fastest
            
            ENDCG
        }

        Pass 
        {
            Tags { "LightMode"="ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Equal
            Cull Back


            CGPROGRAM
            
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _SUB_LIGHT_S
                #pragma multi_compile __ _UNITY_RENDER
                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _EM

                half _ReceiveShadow;

                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 worldNormal : TEXCOORD2;
                    fixed3 viewDir : TEXCOORD3;
                    fixed3 lightDir : TEXCOORD4;
                    half3 giColor : TEXCOORD5;
                    half3 sh : TEXCOORD6;

                    YOUKIA_LIGHTING_COORDS(7, 8)
                    // YOUKIA_HEIGHTFOG_DECLARE(9)

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                half4 _ShadowColor;

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    v.vertex.xyz += YoukiaWind_BigmapPlant(v.color, 1);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _DissolveTex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

                    half3 worldNormal = o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    
                    // em
                    EMData emData;
                    YoukiaEMLod(o.worldPos, emData);
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh.xyz, o.giColor, emData);

                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // height fog
                    // YOUKIA_TRANSFER_HEIGHTFOG_EM(o, 0, emData)

                    return o;
                }
                
                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    fixed3 normal = i.worldNormal;

                    // tex
                    // rgb: color, a: AO
                    fixed4 c = tex2D(_MainTex, i.uv);
                    // rg: normal, b: Metallic, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv);

                    fixed4 col;
                    col.rgb = c.rgb * _Color.rgb;
                    half alpha = c.a * _Color.a;
                    fixed3 abledo = col.rgb;

                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // em
                    EMData emData;
                    YoukiaEM(worldPos, emData);

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor, emData);
                    #endif
                    
                    // light
                    YOUKIA_LIGHT_ATTENUATION_EM(atten, i, i.worldPos.xyz, colShadow, emData);
                    colShadow = lerp(_ShadowColor, 1, colShadow);
                    //是否接收阴影
                    colShadow = lerp(1, colShadow, _ReceiveShadow);

                    UnityGI gi = GetUnityGI_bigmap(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh.rgb, i.giColor, emData);

                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Plant_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    // sub light
                    #ifdef _SUB_LIGHT_S
                        UnityLight light = CreateUnityLight(YoukiaVSLSceneColor(emData), YoukiaVSLSceneRotation(emData));
                        col += BRDF_Plant_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, light, atten);
                    #endif

                    col.a = alpha;

                    // height fog
                    // YOUKIA_HEIGHTFOG(col, i)

                    col.rgb = BigMapFog(col.rgb, worldPos, emData);

                    // lum
                    col.rgb = SceneLumChange(col.rgb);
                    
                   return OutPutDefault(col);
                }
            
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest Equal
            Cull Back
            // ColorMask RGB
            
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _UNITY_RENDER
                
                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed2 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 worldNormal : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    
                    UNITY_LIGHTING_COORDS(6, 7)

                    fixed3 viewDir : TEXCOORD8;
                    fixed3 lightDir : TEXCOORD9;

                    half3 giColor : TEXCOORD10;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(o.worldNormal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(o.worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    fixed3 normal = i.worldNormal;

                    // tex
                    // rgb: color, a: AO
                    fixed4 c = tex2D(_MainTex, i.uv);
                    // rg: normal, b: Metalli, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv);

                    fixed4 col;
                    col.rgb = c.rgb * _Color.rgb;
                    half alpha = c.a * _Color.a;
                    fixed3 abledo = col.rgb;
                    
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

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

                    // pbs
                    col = BRDF_Plant_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    col.a = alpha;

                    return col;
                }
            ENDCG
        }

        // Pass 
        // {
            //     Name "ShadowCaster"
            //     Tags{ "LightMode" = "ShadowCaster" }

            //     ZWrite On 
            //     ZTest LEqual
            //     Cull back`
            
            //     CGPROGRAM
            //     #pragma vertex vertShadowWind
            //     #pragma fragment fragShadow
            //     #pragma multi_compile_shadowcaster
            //     #include "UnityCG.cginc"

            //     #pragma fragmentoption ARB_precision_hint_fastest

            //     ENDCG
        // }

    }
    FallBack "Transparent/Cutout/VertexLit"
    CustomEditor "YoukiaShaderBigMapInspector"
}
