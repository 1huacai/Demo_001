//@@@DynamicShaderInfoStart
//视差Shader，支持视差、自发光等效果。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Environment/YoukiaCrystal"
{
    Properties 
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        [HDR]_ColorR("Color R", Color) = (0.33, 0.33, 0.33, 1)
        [HDR]_ColorG("Color G", Color) = (0.33, 0.33, 0.33, 1)
        [HDR]_ColorB("Color B", Color) = (0.33, 0.33, 0.33, 1)
		_MainTex("color: rgb, a: alpha", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5

        [Header(Parallax)]
        _DetailTex ("视差贴图 R通道", 2D) = "white" {}
        _Parallax ("视差深度", Range(0, 1)) = 0.24
        
        [Header(Emission)]
        [Toggle(_EMISSION)]_Toggle_EMISSION_ON("自发光", Float) = 0
        [Header(Others)]
        [NoScaleOffset] _EmissionMap ("r: 自发光颜色 R, g: 自发光颜色 G, b: 自发光颜色 B", 2D) = "white" {}
        [HDR]_ColorEmissionR("自发光颜色 R", Color) = (1, 1, 1, 1)
        [HDR]_ColorEmissionG("自发光颜色 G", Color) = (0.33, 0.33, 0.33, 1)
        [HDR]_ColorEmissionB("自发光颜色 B", Color) = (0.33, 0.33, 0.33, 1)
        _EmissionStrength("自发光强度", Range(0, 10)) = 1

        [Header(Sparkle)]
        [Toggle(_SPARKLE)]_Toggle_SPARKLE_ON("亮片", Float) = 0
        [HDR]_SparkleColor ("亮片颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_SparkleTex ("亮片纹理", 2D) = "black" {}
        _SparkleTilling ("亮片Tilling", Range(0, 20)) = 1
        _SparklePower ("亮片范围", Range(0, 10)) = 1
        _SparkleParallax ("亮片视差深度", Range(0, 2)) = 0.15
        _SparkleSpeedX ("亮片流动速度 X", float) = 0
        _SparkleSpeedY ("亮片流动速度 Y", float) = 0

        [Space(5)]
        [NoScaleOffset]_SparkleNoiseTex ("亮片遮罩纹理", 2D) = "white" {}
        _SparkleNoiseTilling ("亮片遮罩Tilling", Range(0, 1)) = 0.1
        _SparkleNoiseSpeedX ("亮片遮罩速度 X", float) = 0
        _SparkleNoiseSpeedY ("亮片遮罩速度 Y", float) = 0

        [Header(Shadow)]
        [MaterialToggle]_VertShadow ("顶点计算阴影 #勾选状态下阴影会根据顶点计算。#在顶点较少的模型上可能会出现阴影跳跃的问题。", float) = 1

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        _Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 1
	}
	SubShader 
    {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_PBRDetail" "Reflection" = "RenderReflectionOpaque" }
		
        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
            Pass [_Pass]
        }


        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"

            // #pragma shader_feature _SHADOW_BLUR
            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
            
            // 
            half4 _ColorR, _ColorG, _ColorB;

            // emission
            sampler2D _EmissionMap;
            half4 _ColorEmissionR, _ColorEmissionG, _ColorEmissionB;
            half _EmissionStrength;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                // half3 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        ENDCG

		Pass 
        {
            Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
            // ColorMask RGB

			CGPROGRAM
			
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma shader_feature _EMISSION
                #pragma shader_feature _SPARKLE
                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S
                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    half4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW0 : TEXCOORD2;  
                    half4 TtoW1 : TEXCOORD3;  
                    half4 TtoW2 : TEXCOORD4;
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

                    #ifdef _SKYENABLE
                        half3 inscatter : TEXCOORD9;
                        half3 extinction : TEXCOORD10;
                    #endif

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(11, 12)
                    #else
                        fixed4 screenPos : TEXCOORD11;
                    #endif

                    #ifdef _HEIGHTFOG
                        half4 heightfog : TEXCOORD13;
                    #endif
                    
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                sampler2D _DetailTex;
                half4 _DetailTex_ST;

                half _Parallax;

                sampler2D _SparkleTex, _SparkleNoiseTex;
                half4 _SparkleColor;
                half _SparklePower, _SparkleTilling, _SparkleNoiseTilling, _SparkleParallax;
                half _SparkleSpeedX, _SparkleSpeedY;
                half _SparkleNoiseSpeedX, _SparkleNoiseSpeedY;

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _DetailTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    // 切线空间view dir
                    TANGENT_SPACE_ROTATION;
                    half3 viewDirTangent = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                    o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, viewDirTangent.x);
                    o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, viewDirTangent.y);
                    o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, viewDirTangent.z);

                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
                    
                    // height fog
                    #ifdef _HEIGHTFOG
                        half fog = 0;
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, fog);
                        o.heightfog.a = fog;
                    #endif

                    // atmosphere
                    #ifdef _SKYENABLE
                        half3 extinction = 0;
                        // half3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                        o.extinction = extinction;
                    #endif

                    return o;
                }
                
                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    // half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                    
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    fixed3 worldNormal = fixed3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
                    fixed3 worldTangent = fixed3(i.TtoW0.x, i.TtoW1.x, i.TtoW2.x);
                    fixed3 worldBinormal = fixed3(i.TtoW0.y, i.TtoW1.y, i.TtoW2.y);

                    half4 detail = tex2D(_DetailTex, i.uv.zw);
                    half3 viewDirTangent = normalize(half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w));
                    half2 parallaxOffset = ParallaxOffset(detail.r, _Parallax, viewDirTangent);
                    
                    // tex
                    // rgb: color, a: alpha
                    fixed4 c = tex2D(_MainTex, i.uv.xy);
                    // rg: normal, b: Metallic, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    // rgb
                    fixed4 m = tex2D(_EmissionMap, i.uv.xy + parallaxOffset);
        
                    fixed4 col;
                    col.rgb = (c.r * _ColorR + c.g * _ColorG + c.b * _ColorB) * _Color.rgb;
                    // col.rgb = Saturation(col.rgb, _Sat);
                    half alpha = c.a * _Color.a;
                    fixed3 abledo = col.rgb;
                    // clip(alpha - _Cutoff);

                    // 写固定值，防止美术修改乱掉
                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // normal
                    fixed3 normal = UnpackNormalYoukia(n);

                    // ao
                    // ao 矫正
                    // 固有色 srgb，ao不需要srgb，可以做gamma矫正
                    // half3 ao = AOCalc(c.a);
                    // ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);
                    half3 ao = 1;

                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // light
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    // colShadow *= ao;
                    abledo *= ao;
                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.diffuse *= ao;
                    gi.indirect.specular *= ao;

                    half oneMinusReflectivity;
                    half3 specColor;
                    abledo = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, /*out*/ brdfPreData);

                    // sparkle
                    #if _SPARKLE
                        half sparkle0 = tex2D(_SparkleTex, i.uv.xy * _SparkleTilling + parallaxOffset);
                        sparkle0 = pow(sparkle0, _SparklePower);

                        parallaxOffset = ParallaxOffset(detail.r, _SparkleParallax, viewDirTangent);
                        half sparkle1 = tex2D(_SparkleTex, i.uv.xy * _SparkleTilling * 1.2f + parallaxOffset + _Time.xx * half2(_SparkleSpeedX, _SparkleSpeedY));
                        sparkle1 = pow(sparkle1, _SparklePower);

                        half f = (1 - brdfPreData.nv);

                        half noise = tex2D(_SparkleNoiseTex, i.uv.xy * _SparkleNoiseTilling + _Time.xx * half2(_SparkleNoiseSpeedX, _SparkleNoiseSpeedY)).r;

                        abledo.rgb += (sparkle0 + sparkle1) * _SparkleColor * f * noise;
                    #endif

                    // pbs
                    col = BRDF_Unity_PBS(abledo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    // sub light
                    #ifdef _SUB_LIGHT_S
                        UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                        col += BRDF_Unity_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, light, atten);
                    #endif

                    #if _EMISSION
                        col.rgb += (m.r * _ColorEmissionR + m.g * _ColorEmissionG + m.b * _ColorEmissionB) * _EmissionStrength * colShadow;
                    #endif

                    col.a = alpha;

                    // height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif

                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
                    #endif
                    
                    return OutPutDefault(col);
                }
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]
            // ColorMask RGB

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER
                
                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed2 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW0 : TEXCOORD2;  
                    half3 TtoW1 : TEXCOORD3;  
                    half3 TtoW2 : TEXCOORD4;
                    half3 sh : TEXCOORD5;
                    UNITY_LIGHTING_COORDS(6, 7)
                    fixed3 viewDir : TEXCOORD8;
                    fixed3 lightDir : TEXCOORD9;

                    half3 giColor : TEXCOORD10;

                    #ifndef _UNITY_RENDER
                        fixed4 screenPos : TEXCOORD11;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                    
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW0 = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.TtoW1 = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.TtoW2 = half3(worldTangent.z, worldBinormal.z, worldNormal.z);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    #ifndef _UNITY_RENDER
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif
                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    // half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                    
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    fixed3 worldNormal = fixed3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
                    fixed3 worldTangent = fixed3(i.TtoW0.x, i.TtoW1.x, i.TtoW2.x);
                    fixed3 worldBinormal = fixed3(i.TtoW0.y, i.TtoW1.y, i.TtoW2.y);

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
                    fixed3 abledo = col.rgb;
                    // clip(alpha - _Cutoff);
                    
                    // gamma correct
                    // 写固定值，防止美术修改乱掉
                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // normal
                    fixed3 normal = UnpackNormalYoukia(n);

                    // ao
                    half3 ao = AOCalc(c.a);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, c.a), _AO);

                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    // return atten;

                    fixed3 colShadow = atten;
                    abledo *= ao;

                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                    
                    // UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;
                    
                    half oneMinusReflectivity;
                    half3 specColor;
                    abledo = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    gi.indirect.specular *= colShadow;
                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, /*out*/ brdfPreData);
                    // pbs
                    col = BRDF_Unity_PBS(abledo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

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
            #pragma fragment fragShadowDetail
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            ENDCG
        }

	}
	FallBack "VertexLit"
}
