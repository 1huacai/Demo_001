//@@@DynamicShaderInfoStart
//基础草Shader，只接受投影，不产生投影。顶点计算Diffuse。
//@@@DynamicShaderInfoEnd
// 植被体积感渲染
// http://walkingfat.com/foliage-rendering-%e6%a0%91%e5%8f%b6%e4%bd%93%e7%a7%af%e6%84%9f%e6%b8%b2%e6%9f%93%e4%ba%8c/
// 植被透光
// https://zhuanlan.zhihu.com/p/348259514
// 顶点计算diffuse

Shader "YoukiaEngine/Environment/YoukiaGrass" 
{
    Properties 
    {
        _Color("颜色(rgb, a: alpha)", Color) = (1, 1, 1, 1)
        _MainTex("纹理", 2D) = "white" {}
        [Header(Normal Metallic)]
        [MaterialToggle]_SphereBump("球形法线", float) = 0
        [MaterialToggle]_UpBump("法线朝上", float) = 0
        _NormalStrength ("法线强度", Range(0.1, 2)) = 1
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
        [HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        _VertColorScale ("顶点AO修正", Range(0, 1)) = 0
        [Header(Specular)]
        [HDR]_SpecularColor ("Specular color", Color) = (1, 1, 1, 1)
        [Header(Shadow)]
        _ShadowColor ("Shadow color", Color) = (0, 0, 0, 1)
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        [Header(SSS)]
        _PlantSSSColor ("植被sss color", Color) = (0.15, 0.15, 0.0)
        _PlantSSSDistortion ("植被sss扰动", Range(0, 1)) = 0.5
        _PlantSSSPow ("植被sss范围", Range(0, 5)) = 1
        [Header(Wind)]
        // [Toggle(_WIND)]_Toggle_WIND_ON("风", Float) = 0
        _WindAtten("风强度", Range(0.1, 1)) = 1 

        [Space(30)]
        // [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
        // [Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        // [Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        // _Ref("Stencil Reference", Range(0, 255)) = 0
        // [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 3
        // [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0

    }
    SubShader 
    {
        Tags { "RenderType" = "Transparent" "Queue"="AlphaTest+1" "ShadowType" = "ST_Grass" }
        
        CGINCLUDE
        #pragma multi_compile_instancing
        #pragma target 2.0

        #define _DISABLE_GISPEC 1
        #define GI_TMPFIX 1

        #include "../Library/YoukiaLightPBR.cginc"
        #include "../Library/YoukiaEnvironment.cginc"
        #include "../Library/Atmosphere.cginc"
        #include "../Library/YoukiaEaryZ.cginc"

        #pragma multi_compile __ _GWIND

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

        half _SphereBump;
        half _UpBump;
        half _VertColorScale;

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

            CGPROGRAM
            #pragma vertex vertEaryZGrass
            #pragma fragment fragEaryZ
            #pragma fragmentoption ARB_precision_hint_fastest
            
            ENDCG
        }

        Pass 
        {
            Tags { "LightMode"="ForwardBase" }

            // Stencil
            // {
            //     Ref [_Ref]
            //     Comp [_Comp]
            //     Pass [_Pass]
            // }

            Blend[_SrcBlend][_DstBlend]
            // ZWrite[_zWrite]
            ZWrite Off
            ZTest Equal
            Cull Back
            // ColorMask RGB

            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma multi_compile __ _SUB_LIGHT_S
            #pragma multi_compile __ _UNITY_RENDER
            // youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
            #pragma multi_compile __ _SKYENABLE

            sampler2D _EmissionTex;
            half4 _ColorEmission;
            half _EmissionStrength;
            half4 _ShadowColor;

            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half4 TtoW[3] : TEXCOORD2;
                fixed4 viewDir : TEXCOORD5; // rgb: viewDir, a: nl
                fixed4 lightDir : TEXCOORD6; // rgb: lightDir, a: sub nl
                half3 giColor : TEXCOORD7; 
                half2 lightParam : TEXCOORD14; // x: lh, y: lh_sub

                #ifdef _GWIND
                    // rgb: sh, a : wind
                    half4 sh : TEXCOORD8;
                #else
                    half4 sh : TEXCOORD8;
                #endif

                YOUKIA_ATMOSPERE_DECLARE(9, 10)
                YOUKIA_LIGHTING_COORDS(11, 12)
                YOUKIA_HEIGHTFOG_DECLARE(13)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert(appdata_t v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.sh.a = 1;
                #ifdef _GWIND
                    // half4 grassWind = YoukiaGrassWind(v.color, v.vertex, o.worldPos);
                    float4 grassWind = YoukiaGrassWind_Ver02(o.worldPos, v.color.rgb);
                    v.vertex.xyz += grassWind.xyz;
                    o.sh.a = grassWind.z;
                #endif

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);
                half3 vertColor = lerp(1, v.color.rrr, _VertColorScale);
                // 球形法线
                half3 n = lerp(v.normal, v.vertex, FastSign(_SphereBump));
                fixed3 worldNormal;
                T2W(n, v.tangent, o.TtoW, worldNormal, vertColor);
                worldNormal = lerp(worldNormal, half3(0, 1, 0), _UpBump);

                //YoukiaVertSH(worldNormal, o.worldPos, o.sh.xyz, o.giColor);
                #if defined (UNITY_UV_STARTS_AT_TOP)
                    o.sh.xyz = ShadeSHPerVertex(worldNormal, o.sh.xyz);
                #endif
                o.sh.xyz += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);
                o.sh.xyz *= vertColor;

                // 顶点光照相关
                // 由于顶点光照的效果较弱，不再在顶点计算主光源nl
                // nl
                half nl = saturate(dot(worldNormal, o.lightDir));
                nl = max(_DiffThreshold, nl);
                o.viewDir.a = nl;

                half3 halfDir = normalize(o.lightDir.xyz + o.viewDir.xyz);
                // lh
                o.lightParam.x = saturate(dot(o.lightDir, halfDir));

                #ifdef _SUB_LIGHT_S
                    half3 subLightDir = normalize(_gVSLFwdVec_S);
                    nl = saturate(dot(worldNormal, subLightDir));
                    nl = max(_DiffThreshold, nl);
                    o.lightDir.a = nl;
                    
                    halfDir = normalize(subLightDir.xyz + o.viewDir.xyz);
                    // lh
                    o.lightParam.y = saturate(dot(subLightDir, halfDir));
                #endif

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
                fixed3 vertColor = fixed3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w);

                // tex
                // rgb: color, a: AO
                fixed4 c = tex2D(_MainTex, i.uv);
                // rg: normal, b: Metallic, a: Roughness
                fixed4 n = tex2D(_BumpMetaMap, i.uv);

                fixed4 col;
                col.rgb = c.rgb * _Color.rgb;
                half alpha = c.a * _Color.a; 
                fixed3 abledo = col.rgb * vertColor;

                _Roughness = 1;
                _Metallic = 1;
                half metallic = MetallicCalc(n.b);
                half smoothness = SmoothnessCalc(n.a);

                // normal
                half3 normal = WorldNormal(n, i.TtoW);

                // light
                YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                colShadow = lerp(_ShadowColor, 1, colShadow);
                
                UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh.rgb, i.giColor);

                half oneMinusReflectivity;
                half3 specColor;
                abledo = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                // brdf pre
                BRDFPreData brdfPreData = (BRDFPreData)0;
                PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                // pbs
                half nl = i.viewDir.a;
                half lh = i.lightParam.x;
                col = BRDF_Grass_PBS(abledo, specColor, normal, viewDir, brdfPreData, nl, lh, gi.light, gi.indirect, colShadow);
                
                // sub light
                #ifdef _SUB_LIGHT_S
                    nl = i.lightDir.a;
                    lh = i.lightParam.y;
                    UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                    col += BRDF_Grass_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, nl, lh, light);
                #endif

                col.a = alpha;

                // height fog
                YOUKIA_HEIGHTFOG(col, i)
                // atmosphere
                YOUKIA_ATMOSPHERE(col, i)
                
                // #ifdef _GWIND
                //     fixed3 temp = lerp(_RepplingColor.rgb, col.rgb, i.sh.a);
                //     col.rgb = lerp(temp, col.rgb, vertColor.r) * _colorMultiply;
                // #endif


                // lum
                col.rgb = SceneLumChange(col.rgb);

                return OutPutGrass(col);
            }
            
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Stencil
            {
                Ref [_Ref]
                Comp [_Comp]
                Pass [_Pass]
            }

            Blend [_SrcBlend] One
            ZWrite Off
            ZTest Equal
            Cull Back

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
                half4 color : TEXCOORD3;
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

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                #ifdef _GWIND
                    float4 grassWind = YoukiaGrassWind_Ver02(o.worldPos, v.color.rgb);
                    v.vertex.xyz += grassWind.xyz;
                #endif

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);
                
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.color.rgb = v.color.rgb;

                // vert light
                half nl = saturate(dot(worldNormal, o.lightDir));
                o.color.a = max(_DiffThreshold, nl);

                o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target 
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half3 vertColor = i.color.rgb;
                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;
                half nl = i.color.a;

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
            Cull Back
            
            CGPROGRAM
            #pragma vertex vertShadowGrass
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

    }
    FallBack "Transparent/Cutout/VertexLit"
    // CustomEditor "YoukiaTreeInspector"
}
