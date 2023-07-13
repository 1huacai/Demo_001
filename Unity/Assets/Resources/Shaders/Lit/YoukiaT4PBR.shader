Shader "YoukiaEngine/Lit/YoukiaT4PBR"
{
    Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_Splat0("纹理 0 (rgb: color, a: AO)", 2D) = "white" {}
        _Splat1("纹理 1 (rgb: color, a: AO)", 2D) = "white" {}
        _Splat2("纹理 2 (rgb: color, a: AO)", 2D) = "white" {}
        _Splat3("纹理 3 (rgb: color, a: AO)", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpSplat0("法线-金属度纹理 0 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_0 ("法线强度", Range(0, 5)) = 1
        [NoScaleOffset] _BumpSplat1("法线-金属度纹理 1 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_1 ("法线强度", Range(0, 5)) = 1
        [NoScaleOffset] _BumpSplat2("法线-金属度纹理 2 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_2 ("法线强度", Range(0, 5)) = 1
        [NoScaleOffset] _BumpSplat3("法线-金属度纹理 3 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_3 ("法线强度", Range(0, 5)) = 1
        [Header(Blend)]
        _Control("混合贴图 (rgba)", 2D) = "white" {}
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Metallic Gloss)]
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Roughness ("Roughness", Range(0, 1)) = 0.5
        // [Header(AO)]
        // _AO ("AO强度", Range(0, 1)) = 0
        // _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        // [Header(Edge)]
        // _BlendWeight("边缘混合过渡" , Range(0.001,1)) = 1
        // _Ref("Stencil Reference", Range(0, 255)) = 0
        // [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 3
        // [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest+5" "RenderType"="Opaque" "ShadowType" = "ST_Terrain" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 2.0

		    #include "UnityStandardUtils.cginc"
            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
            half _SplatScale_0, _SplatScale_1, _SplatScale_2, _SplatScale_3;

            sampler2D _BumpSplat0, _BumpSplat1, _BumpSplat2, _BumpSplat3;
            half _NormalStrength_0, _NormalStrength_1, _NormalStrength_2, _NormalStrength_3;

            sampler2D _Control;
            half4 _Control_ST;

            half _gTerrainBlendWeight;

            struct appdata_t 
            {
                half4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                // half3 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            inline half4 Blend(half4 control)
            {
                half4 blend = control;
                
                half ma = max(blend.r, max(blend.g, max(blend.b, blend.a)));
                blend = max(blend - ma + _gTerrainBlendWeight , 0) * control;
                return blend / (blend.r + blend.g + blend.b + blend.a);
            }

            inline void SampleT4Tex(half2 uv, half4 uv0, half4 uv1, out fixed4 blend, out fixed4 blend_n, out fixed3 normal)
            {
                half4 blendmask = tex2D(_Control, uv);

                // blendmask = Blend(blendmask);

                half4 lay0 = 0;
                half4 lay1 = 0;
                half4 lay2 = 0;
                half4 lay3 = 0;
                half4 lay0_n = 0;
                half4 lay1_n = 0;
                half4 lay2_n = 0;
                half4 lay3_n = 0;
                fixed3 normal0 = 0;
                fixed3 normal1 = 0;
                fixed3 normal2 = 0;
                fixed3 normal3 = 0;

                lay0 = tex2D(_Splat0, uv0.xy) * blendmask.r;
                lay0_n = tex2D(_BumpSplat0, uv0.xy);
                lay0_n.ba *= blendmask.r;
                normal0 = UnpackNormalYoukia(lay0_n, _NormalStrength_0) * blendmask.r;

                lay1 = tex2D(_Splat1, uv0.zw) * blendmask.g;
                lay1_n = tex2D(_BumpSplat1, uv0.zw);
                lay1_n.ba *= blendmask.g;
                normal1 = UnpackNormalYoukia(lay1_n, _NormalStrength_1) * blendmask.g;

                lay2 = tex2D(_Splat2, uv1.xy) * blendmask.b;
                lay2_n = tex2D(_BumpSplat2, uv1.xy);
                lay2_n.ba *= blendmask.b;
                normal2 = UnpackNormalYoukia(lay2_n, _NormalStrength_2) * blendmask.b;

                lay3 = tex2D(_Splat3, uv1.zw) * blendmask.a;
                lay3_n = tex2D(_BumpSplat3, uv1.zw);
                lay3_n.ba *= blendmask.a;
                normal3 = UnpackNormalYoukia(lay3_n, _NormalStrength_3) * blendmask.a;

                blend = lay0 + lay1 + lay2 + lay3;
                blend_n = lay0_n + lay1_n + lay2_n + lay3_n;
                normal = normal0 + normal1 + normal2 + normal3;
            }

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            // Stencil
            // {
            //     Ref [_Ref]
            //     Comp [_Comp]
            //     Pass [_Pass]
            // }

			ZWrite On
			ZTest LEqual
			Cull Back

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest
                
                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_S

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW[3] : TEXCOORD2;
                    half4 sh : TEXCOORD5;

                    fixed4 uv_0 : TEXCOORD6;
                    fixed4 uv_1 : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

                    YOUKIA_ATMOSPERE_DECLARE(9, 10)
                    YOUKIA_LIGHTING_COORDS(11, 12)
                    YOUKIA_HEIGHTFOG_DECLARE(13)

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    
                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control);
                    o.uv_0.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
                    o.uv_0.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
                    o.uv_1.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
                    o.uv_1.zw = TRANSFORM_TEX(v.texcoord, _Splat3);

                    fixed3 worldNormal;
                    fixed3 viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    half3 lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal, viewDir);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh.xyz = ShadeSHPerVertex(worldNormal, o.sh.xyz);
                    #endif
                    o.sh.xyz += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);
                    o.uv.zw = lightDir.xy;
                    o.sh.w = lightDir.z;

                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // height fog
                    YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                    // atmosphere
                    YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    return o;
                }

                FragmentOutput frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = normalize(half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w));
                    half3 lightDir = normalize(half3(i.uv.z, i.uv.w, i.sh.w));

                    fixed4 blend = 0;
                    fixed4 blend_n = 0;
                    fixed3 normal = 0;

                    SampleT4Tex(i.uv, i.uv_0, i.uv_1, blend, blend_n, normal);

                    fixed4 col;
                    col.rgb = blend.rgb * _Color.rgb;
                    col.a = 1;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    // Metallic R: Metallic, G: Roughness
                    half metallic = MetallicCalc(blend_n.b);
                    half smoothness = SmoothnessCalc(blend_n.a);

                    // normal
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // light
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                    // ao
                    albedo *= blend.a;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.diffuse *= blend.a;
                    gi.indirect.specular *= blend.a;

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

                    col.a = alpha;

                    // height fog
                    YOUKIA_HEIGHTFOG(col, i)
                    // atmosphere
                    YOUKIA_ATMOSPHERE(col, i)

                    // lum
                    col.rgb = SceneLumChange(col.rgb);

                    return OutPutTerrain(col);
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Stencil
            {
                Ref [_Ref]
                Comp [_Comp]
                Pass [_Pass]
            }

            Blend One One
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
                    half4 pos : SV_POSITION;
                    fixed2 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    
                    fixed4 uv_0 : TEXCOORD6;
                    fixed4 uv_1 : TEXCOORD7;

                    UNITY_LIGHTING_COORDS(8, 9)

                    fixed3 viewDir : TEXCOORD10;
                    fixed3 lightDir : TEXCOORD11;

                    half3 giColor : TEXCOORD12;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control);
                    o.uv_0.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
                    o.uv_0.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
                    o.uv_1.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
                    o.uv_1.zw = TRANSFORM_TEX(v.texcoord, _Splat3);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    fixed3 lightDir = i.lightDir;

                    fixed4 blend = 0;
                    fixed4 blend_n = 0;
                    fixed3 normal = 0;
                    SampleT4Tex(i.uv, i.uv_0, i.uv_1, blend, blend_n, normal);

                    fixed4 col;
                    col.rgb = blend.rgb * _Color.rgb;
                    half alpha = 1;
                    fixed3 albedo = col.rgb;

                    // Metallic R: Metallic, G: Roughness
                    half metallic = MetallicCalc(blend_n.b);
                    half smoothness = SmoothnessCalc(blend_n.a);

                    // normal
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;

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
                    col = BRDF_Unity_PBS_Add(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    col.a = alpha;


                    return col;
                }
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadowTerrain
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
}
