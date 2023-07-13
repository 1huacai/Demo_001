//@@@DynamicShaderInfoStart
//场景植被Shader。
//@@@DynamicShaderInfoEnd
// 植被体积感渲染
// http://walkingfat.com/foliage-rendering-%e6%a0%91%e5%8f%b6%e4%bd%93%e7%a7%af%e6%84%9f%e6%b8%b2%e6%9f%93%e4%ba%8c/
// 植被透光
// https://zhuanlan.zhihu.com/p/348259514

Shader "YoukiaEngine/Obsolete/YoukiaPlant" 
{
    Properties 
    {
        _Color("颜色(rgb, a: alpha)", Color) = (1, 1, 1, 1)
        _MainTex("纹理", 2D) = "white" {}
        [Header(Normal Metallic)]
        [Toggle(_BUMP)] _Toggle_BUMP_ON ("计算法线贴图", Float) = 0
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
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
        _PlantSSSDistortion ("植被透光扰动normalScale", Range(0, 1)) = 0.5
        _PlantSSSPow ("植被透光范围power", Range(0, 10)) = 1
        _PlantSSSScale ("植被透光系数scale", Range(0, 10)) = 1
        [Header(Emission)]
        // [Toggle(_EMISSION)]_Toggle_EMISSION_ON("自发光", Float) = 0
        _EmissionTex("自发光贴图(RGB)", 2D) = "black" {}
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        _EmissionStrength("自发光强度", Range(0, 10)) = 1
        [Header(Wind)]
        _WindAtten("全局风力衰减", Range(0.1, 1)) = 1

        [Header(Dissolve)]
        // [Toggle(_USE_DISSOLVE)]_Toggle_USE_DISSOLVE_ON("溶解", float) = 0
        _DissolveTex ("溶解贴图", 2D) = "white" {}
        _DissolveStrength ("溶解度", Range(0, 1.1)) = 0
        [HDR]_DissolveColor ("溶解边缘颜色", Color) = (1, 1, 1, 1)
        _DissolveWidth ("溶解边缘宽度", Range(0, 0.5)) = 0 
        _DissolveSmooth ("边缘硬度", Range(0, 1)) = 0

        [Space(10)]
        // [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
        // [Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode(除非必要，不要off)", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        _Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0

    }
    SubShader 
    {
        Tags { "RenderType" = "TransparentCutout" "Queue"="AlphaTest" "ShadowType" = "ST_Plant" "Reflection" = "RenderReflectionCutout" }
        
        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
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
        #include "YoukiaObsolete.cginc"

        #pragma shader_feature _USE_DISSOLVE
        // #pragma multi_compile __ _GWIND

        
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
            // Tags { "LightMode"="ForwardBase" }
            ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull [_cull]

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
            Cull [_cull]
            // ColorMask RGB

            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma shader_feature _BUMP
            #pragma multi_compile __ _SUB_LIGHT_S
            #pragma multi_compile __ _UNITY_RENDER
            // youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
            #pragma multi_compile __ _SKYENABLE
            // #pragma shader_feature _EMISSION
            #pragma multi_compile __ _GLOBALSSS

            sampler2D _EmissionTex;
            half4 _ColorEmission;
            half _EmissionStrength;
            half _ReceiveShadow;

            half _UseGlobalSSS;

            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half4 TtoW0 : TEXCOORD2;
                half4 TtoW1 : TEXCOORD3; 
                half4 TtoW2 : TEXCOORD4;
                half3 viewDir : TEXCOORD5;
                half3 lightDir : TEXCOORD6;
                half3 giColor : TEXCOORD7;
                half3 sh : TEXCOORD8;


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

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                
                // #ifdef _GWIND
                // #endif

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.texcoord, _DissolveTex);

                o.viewDir.xyz = normalize(UnityWorldSpaceViewDir(o.worldPos));
                o.lightDir.xyz = normalize(UnityWorldSpaceLightDir(o.worldPos));

                #ifdef _UNITY_RENDER
                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                #else
                    o.screenPos = ComputeScreenPos(o.pos);  
                #endif

                half3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, v.color.r);
                o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, v.color.g);
                o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, v.color.b);
                
                YoukiaVertSH(worldNormal, o.worldPos, o.sh.xyz, o.giColor.xyz);

                // height fog
                #ifdef _HEIGHTFOG
                    o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, o.heightfog.a);
                #endif

                // atmosphere
                #ifdef _SKYENABLE
                    o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, o.extinction);
                #endif

                return o;
            }
            
            FragmentOutput frag(v2f i)
            {
                return OutPutDefault(OBSOLETECOLOR);
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                half3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;
                half3 giColor = i.giColor;
                fixed3 vertColor = fixed3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);


                // tex
                // rgb: color, a: AO
                fixed4 c = tex2D(_MainTex, i.uv);
                // rg: normal, b: Metallic, a: Roughness
                fixed4 n = tex2D(_BumpMetaMap, i.uv);

                fixed4 col;
                col.rgb = c.rgb * _Color.rgb;
                half alpha = c.a * _Color.a;
                fixed3 abledo = col.rgb;
                // clip(alpha - _Cutoff);

                // dissolve
                half4 dissColor = Dissolve(i.uv.zw);

                _Roughness = 1;
                _Metallic = 1;
                half metallic = MetallicCalc(n.b);
                half smoothness = SmoothnessCalc(n.a);

                // normal
                // half3 normalSphere;
                // half3 normal;
                #ifdef _BUMP
                    fixed3 normal = UnpackNormalYoukia(n);
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                #else
                    fixed3 normal = fixed3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
                #endif


                #if defined (UNITY_UV_STARTS_AT_TOP)
                    i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, giColor);
                #endif
                
                
                // light
                #ifdef _UNITY_RENDER
                    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                    fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                #else
                    half atten = 0;
                    fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten); 
                #endif

                // colShadow *= YoukiaShadowXZ(worldPos, lightDir);
                colShadow = lerp(_ShadowColor, 1, colShadow);

                //是否接收阴影
                colShadow = lerp(1, colShadow, _ReceiveShadow);

                // colShadow = 1;


                UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh.rgb, giColor);


                half oneMinusReflectivity;
                half3 specColor;
                col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);


                // brdf pre
                BRDFPreData brdfPreData = (BRDFPreData)0;
                PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                // pbs
                col = BRDF_Plant_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                //col = BRDF_Unity_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                // sub light
                #ifdef _SUB_LIGHT_S
                    UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                    col += BRDF_Plant_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, light, atten);
                #endif

                // #if _EMISSION
                half3 emission = tex2D(_EmissionTex, i.uv);
                col.rgb += emission * _ColorEmission * _EmissionStrength;
                // #endif

                col.a = alpha;

                // dissolve
                col.rgb = lerp(col.rgb, dissColor.rgb, dissColor.a);

                #ifdef _SKYENABLE
                    col.rgb = col.rgb * i.extinction + i.inscatter;
                #endif

                // height fog
                #ifdef _HEIGHTFOG
                    col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                #endif

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
            Cull [_cull]
            // ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma shader_feature _BUMP
            #pragma multi_compile __ _UNITY_RENDER
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;

                half4 TtoW0 : TEXCOORD2;  
                half4 TtoW1 : TEXCOORD3;  
                half4 TtoW2 : TEXCOORD4;
                half3 sh : TEXCOORD5;
                
                UNITY_LIGHTING_COORDS(6, 7)

                half4 viewDir : TEXCOORD8;
                half4 lightDir : TEXCOORD9;
                half4 giColor : TEXCOORD10;

                #ifndef _UNITY_RENDER
                    fixed4 screenPos : TEXCOORD11;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_t v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // #ifdef _GWIND
                // #endif

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir.xyz = normalize(UnityWorldSpaceViewDir(o.worldPos));
                o.lightDir.xyz = normalize(UnityWorldSpaceLightDir(o.worldPos));
                
                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, v.color.r);
                o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, v.color.g);
                o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, v.color.b);
                
                #if defined (UNITY_UV_STARTS_AT_TOP)
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                #endif
                o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor.xyz);

                #ifndef _UNITY_RENDER
                    o.screenPos = ComputeScreenPos(o.pos);
                #endif
                UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                //plantSSS
                // half3 pivot = half3(v.vertex.x + _PivotOffsetX, v.vertex.y + _PivotOffsetY, v.vertex.z);
                // pivot = normalize(UnityObjectToWorldDir(pivot));
                // o.viewDir.a = pivot.x;
                // o.lightDir.a = pivot.y;
                // o.giColor.a = pivot.z;


                return o;
            }

            fixed4 frag(v2f i) : SV_Target 
            {
                half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                half3 viewDir = i.viewDir.xyz;
                half3 lightDir = i.lightDir.xyz;
                half3 giColor = i.giColor.xyz;
                half3 sssNormal = half3(i.viewDir.a, i.lightDir.a, i.giColor.a);

                // tex
                // rgb: color, a: AO
                fixed4 c = tex2D(_MainTex, i.uv);
                // rg: normal, b: Metalli, a: Roughness
                fixed4 n = tex2D(_BumpMetaMap, i.uv);

                fixed4 col;
                col.rgb = c.rgb * _Color.rgb;
                half alpha = c.a * _Color.a;
                fixed3 abledo = col.rgb;
                // clip(alpha - _Cutoff);
                
                half metallic = MetallicCalc(n.b);
                half smoothness = SmoothnessCalc(n.a);

                // normal
                #ifdef _BUMP
                    fixed3 normal = UnpackNormalYoukia(n);
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                #else
                    fixed3 normal = fixed3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
                #endif

                // light
                UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                fixed3 colShadow = atten;
                // #ifndef _UNITY_RENDER
                //     #ifdef SPOT
                //         colShadow *= YoukiaScreenShadow(i.screenPos);
                //     #endif
                // #endif
                
                // UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, giColor);
                gi.indirect.specular *= colShadow;
                
                half oneMinusReflectivity;
                half3 specColor;
                col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                gi.indirect.specular *= colShadow;

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

        Pass 
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            ZWrite On 
            ZTest LEqual
            Cull back
            
            CGPROGRAM
            #pragma vertex vertShadow
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
