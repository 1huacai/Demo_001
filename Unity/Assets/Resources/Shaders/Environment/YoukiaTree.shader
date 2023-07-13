//@@@DynamicShaderInfoStart
//场景植被Shader。
//@@@DynamicShaderInfoEnd
// 植被体积感渲染
// http://walkingfat.com/foliage-rendering-%e6%a0%91%e5%8f%b6%e4%bd%93%e7%a7%af%e6%84%9f%e6%b8%b2%e6%9f%93%e4%ba%8c/
// 植被透光
// https://zhuanlan.zhihu.com/p/348259514

Shader "YoukiaEngine/Environment/YoukiaTree" 
{
    Properties 
    {
        _Color("颜色(rgb, a: alpha)", Color) = (1, 1, 1, 1)
        _MainTex("纹理", 2D) = "white" {}
        [Header(Normal Metallic)]
        // [Toggle(_BUMP)] _Toggle_BUMP_ON ("计算法线贴图", Float) = 0
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度", Range(0.1, 2)) = 1
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
        [HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse 阈值", Range(0, 0.5)) = 0
        _DiffPow ("Diffuse 衰减", Range(0, 4)) = 1
        [HDR]_DiffColorBright ("Diffuse 亮部颜色", Color) = (1, 1, 1, 1)
        [HDR]_DiffColorDark ("Diffuse 暗部颜色", Color) = (0, 0, 0, 1)
        [Header(Specular)]
        [HDR]_SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
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
        [Header(Emission)]
        // [Toggle(_EMISSION)]_Toggle_EMISSION_ON("自发光", Float) = 0
        _EmissionTex("自发光贴图(RGB)", 2D) = "black" {}
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        _EmissionStrength("自发光强度", Range(0, 10)) = 1
        [Header(Wind)]
        _WindAtten("全局风力衰减", Range(0,10)) = 1

        [Space(10)]
        // [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
        // [Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        [Space(10)]
        _Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0 

    }
    SubShader 
    {
        Tags { "RenderType" = "TransparentCutout" "Queue"="Geometry" "ShadowType" = "ST_Tree" "Reflection" = "RenderReflectionCutout" }
        
        Stencil
        {
            Ref [_Ref]
            Pass [_Pass]
        }
        
        CGINCLUDE
        #pragma multi_compile_instancing
        #pragma target 2.0

        #define _DISABLE_GISPEC 1

        #include "../Library/YoukiaLightPBR.cginc"
        #include "../Library/YoukiaEnvironment.cginc"
        #include "../Library/Atmosphere.cginc"
        #include "../Library/YoukiaEaryZ.cginc"
        #include "../Library/YoukiaEffect.cginc"

        #pragma multi_compile __ _GWIND
        
        #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
        #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        struct appdata_t 
        {
            float4 vertex : POSITION;
            half2 texcoord : TEXCOORD0;
            half3 normal : NORMAL;
            half4 tangent : TANGENT;
            half3 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };


        ENDCG

        Pass
        {
            Name "EarlyZ"
            // Tags { "LightMode"="ForwardBase" }
            // ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull Back

            Blend[_SrcBlend][_DstBlend]

            // Stencil
            // {
            //     Ref [_Ref]
            //     Comp [_Comp]
            //     Pass [_Pass]
            // }

            CGPROGRAM
            #pragma vertex vertEaryZPlant
            #pragma fragment fragEaryZ
            #pragma fragmentoption ARB_precision_hint_fastest
            
            ENDCG
        }

        Pass 
        {
            Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite Off
            ZTest Equal
            Cull Back
            // ColorMask RGB

            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest

            // #pragma shader_feature _BUMP
            #pragma multi_compile __ _SUB_LIGHT_S
            #pragma multi_compile __ _UNITY_RENDER
            // youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
            #pragma multi_compile __ _SKYENABLE
            // #pragma shader_feature _EMISSION

            sampler2D _EmissionTex;
            half4 _ColorEmission;
            half _EmissionStrength;
            half _ReceiveShadow;

            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half4 TtoW[3] : TEXCOORD2;
                fixed3 viewDir : TEXCOORD5;
                fixed3 lightDir : TEXCOORD6;
                half3 giColor : TEXCOORD7;

                half4 sh : TEXCOORD8;

                YOUKIA_ATMOSPERE_DECLARE(9, 10)
                YOUKIA_LIGHTING_COORDS(11, 12)
                YOUKIA_HEIGHTFOG_DECLARE(13)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half4 _ShadowColor;

            v2f vert(appdata_t v) 
            {
                v2f o; 
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #ifdef _GWIND
                    float4 windValue = YoukiaWind(v.color, v.vertex.xyz);
                    v.vertex.xyz += windValue.xyz;
                #endif

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

                fixed3 worldNormal;
                T2W(v.normal, v.tangent, o.TtoW, worldNormal, v.color);
                
                YoukiaVertSH(worldNormal, o.worldPos, o.sh.xyz, o.giColor);

                YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                // height fog
                YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                // atmosphere
                YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                return o;
            }
            
            FragmentOutput frag(v2f i) 
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;

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

                // normal
                // half3 normalSphere;
                // half3 normal;
                _NormalStrength = 1;
                // #ifdef _BUMP
                    half3 normal = WorldNormal(n, i.TtoW);
                // #else
                //     half3 normal = half3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
                // #endif

                #if defined (UNITY_UV_STARTS_AT_TOP)
                    i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                #endif
                
                // light
                YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                // colShadow *= YoukiaShadowXZ(worldPos, lightDir);
                colShadow = lerp(_ShadowColor, 1, colShadow);
                //是否接收阴影
                colShadow = lerp(1, colShadow, _ReceiveShadow);

                UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh.rgb, i.giColor);
                
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
                    UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                    col += BRDF_Plant_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, light, atten);
                #endif

                half3 emission = tex2D(_EmissionTex, i.uv);
                col.rgb += emission * _ColorEmission * _EmissionStrength;

                col.a = alpha;

                // height fog
                YOUKIA_HEIGHTFOG(col, i)
                // atmosphere
                YOUKIA_ATMOSPHERE(col, i)

                // lum
                col.rgb = SceneLumChange(col.rgb);

                return OutPutDefault(col);
            }
            
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
            ZWrite Off
            ZTest Equal
            Cull back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            // #pragma fragmentoption ARB_precision_hint_fastest

            #pragma multi_compile __ _UNITY_RENDER
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                half4 sh : TEXCOORD5;
                
                UNITY_LIGHTING_COORDS(6, 7)

                float3 viewDir : TEXCOORD8;
                float3 lightDir : TEXCOORD9;

                half3 giColor : TEXCOORD10;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_t v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #ifdef _GWIND
                    v.vertex.xyz += YoukiaWind(v.color, v.vertex.xyz);
                #endif

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                
                half3 worldNormal = o.worldNormal = UnityObjectToWorldNormal(v.normal);  

                o.sh.rgb = ShadeSHPerVertex(worldNormal, o.sh.rgb);
                o.sh.rgb += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                // vert light
                half nl = saturate(dot(worldNormal, o.lightDir));
                o.sh.w = max(_DiffThreshold, nl);

                UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                return o;
            }

            half4 frag(v2f i) : SV_Target 
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;
                half nl = i.sh.w;

                // tex
                // rgb: color, a: AO
                fixed4 c = tex2D(_MainTex, i.uv);

                fixed4 col;
                col.rgb = c.rgb * _Color.rgb;
                half alpha = c.a * _Color.a;
                fixed3 albedo = col.rgb;

                // light
                UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                
                col.rgb = albedo * i.sh * atten;
                col.rgb += albedo * atten * nl * _LightColor0.rgb;

                col.a = alpha;

                return col;
            }
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            ZWrite On 
            ZTest LEqual
            Cull back
            
            CGPROGRAM
            #pragma vertex vertShadowWind
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

    }
    FallBack "Transparent/Cutout/VertexLit"
    CustomEditor "YoukiaTreeInspector"
}
