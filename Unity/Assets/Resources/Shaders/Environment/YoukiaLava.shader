//@@@DynamicShaderInfoStart
//岩浆、熔岩。UV1->遮罩纹理。 UV2->岩浆流动方向。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Environment/YoukiaLava" 
{
	Properties 
    {
        [Header(Lava1)]
        [HDR]_Color1_B("下层岩浆颜色1", Color) = (1, 1, 1, 1)
        [HDR]_Color2_B("下层岩浆颜色2", Color) = (1, 1, 1, 1)
		_LavaTex_B("下层岩浆纹理", 2D) = "white" {}
        _LavaScale_B("下层岩浆颜色过渡", Range(1, 20)) = 1
        _LavaNoise_B("下层岩浆扰动", Range(0, 0.5)) = 0.1

        [Header(Normal)]
        [NoScaleOffset] _BumpMap_B("下层法线纹理 (rg: normal)", 2D) = "white" {}

        [Gamma]_Metallic ("下层岩浆 Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("下层岩浆 Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold ("下层岩浆 Diffuse threshold", Range(0, 0.5)) = 0

        [Header(Speed)]
        _LavaSpeedV_B ("岩浆流动 速度", Range(0, 10)) = 0

        [Header(Parallax)]
        _ParallaxDepth ("视差深度", Range(0, 2)) = 0

        [Header(Lava2)]
        [HDR]_Color1_T("上层岩浆颜色", Color) = (1, 1, 1, 1)
        _LavaTex_T("上层岩浆纹理", 2D) = "white" {}
        _LavaTop("上层岩浆范围", Range(0, 1)) = 0.2
        _LavaTopAlpha ("上层岩浆透光", Range(0, 1)) = 0

        [Header(Normal)]
        [NoScaleOffset] _BumpMap_T("上层法线纹理 (rg: normal)", 2D) = "white" {}

        [Gamma]_Metallic_T ("上层 Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness_T ("上层 Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold_T ("上层 Diffuse threshold", Range(0, 0.5)) = 0

        [Header(Speed)]
        _LavaSpeedV_T ("岩浆流动 速度", Range(0, 10)) = 0

        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 1)) = 1

        [Header(Others)]
        [NoScaleOffset]_MaskMap ("(R: 上下层岩浆Mask, G: 透明度, B: 自发光)", 2D) = "white" {}
        [HDR]_EmissionColor ("自发光", Color) = (0, 0, 0, 1)
        _EmissionScale ("自发光范围", Range(1, 10)) = 1

        [Header(VertexAnim)]
        _VertexNoiseStrength ("顶点噪声强度", Range(0, 0.5)) = 0

        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
	}
	SubShader 
    {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_PBRDetail" "Reflection" = "RenderReflectionOpaque" }


        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"
            #include "../Library/YoukiaEffect.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            // normal
            // half _NormalStrength;

            // lava
            // blow
            half4 _Color1_B, _Color2_B;
            half _LavaScale_B, _LavaNoise_B;
            sampler2D _LavaTex_B, _BumpMap_B;
            float4 _LavaTex_B_ST;
            half _LavaSpeedV_B;

            half _ParallaxDepth;

            // top
            half4 _Color1_T;
            sampler2D _LavaTex_T, _BumpMap_T;
            float4 _LavaTex_T_ST;
            half _LavaTop, _LavaTopAlpha;
            half _LavaSpeedV_T;

            half _Metallic_T, _Roughness_T, _DiffThreshold_T;

            // mask
            sampler2D _MaskMap;
            half4 _MaskMap_ST;
            half4 _EmissionColor;
            half _EmissionScale;

            // vertex noise
            half _VertexNoiseStrength;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                fixed2 texcoord2 : TEXCOORD1;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点noise
            float3 VertNoise(half2 uv, half3 normal)
            {
                float2 vertUV = TRANSFORM_TEX(uv, _LavaTex_B) + _Time.x * half2(0, _LavaSpeedV_B);
                half vertNoise = 1 - tex2Dlod(_LavaTex_B, half4(vertUV, 0, 0));
                vertUV = TRANSFORM_TEX(uv, _LavaTex_T) + _Time.x * half2(0, _LavaSpeedV_T);
                vertNoise += tex2Dlod(_LavaTex_T, half4(vertUV, 0, 0));
                vertNoise *= 0.5f;
                vertNoise = vertNoise * 2 - 1;
                vertNoise *= _VertexNoiseStrength;
                float3 noise = normal * vertNoise;
                return noise;
            }

            void Lava(half3 viewDirTangent, half2 maskUV, half2 uv1, half2 uv2, 
                in out half3 albedo, in out half3 normal, in out half alpha, in out half metallic, in out half smoothness, in out half diffThreshold)
            {
                // parallax
                half2 parallaxOffset = ParallaxOffset(0, _ParallaxDepth, viewDirTangent);

                // r: mask, g: alpha, b: emission
                fixed4 m = tex2D(_MaskMap, maskUV);

                half2 lavaUV = uv1 + _Time.x * half2(0, _LavaSpeedV_B);
                half2 lavaUV2 = uv2 + _Time.x * half2(0, _LavaSpeedV_T);

                // lava 
                half4 lava = tex2D(_LavaTex_T, lavaUV2);

                half texNoise = lava.r * _LavaNoise_B;
                // rgb: color
                fixed4 c = tex2D(_LavaTex_B, lavaUV + parallaxOffset + texNoise);
                // rg: normal
                fixed4 n = tex2D(_BumpMap_B, lavaUV + parallaxOffset + texNoise);

                half emission = pow(tex2D(_MaskMap, maskUV + texNoise).b, _EmissionScale);

                half4 lavaN = tex2D(_BumpMap_T, lavaUV2);
                half lavaMask = saturate(lava * m.r);
                lavaMask = smoothstep(0, (1 - _LavaTop), lavaMask);

                // lava color
                fixed4 col;
                col.rgb = lerp(_Color1_B.rgb, _Color2_B.rgb, saturate(pow((1 - c.r), _LavaScale_B)));
                col.rgb = lerp(col.rgb, _EmissionColor, emission);
                lava.rgb = lerp(col.rgb, _Color1_T.rgb * lavaMask, saturate(pow(lava, _LavaTopAlpha)));
                col.rgb = lerp(col.rgb, lava.rgb, lavaMask);

                alpha = m.g * _Color1_B.a;
                clip(alpha * lavaMask - lerp(_Cutoff, 0, alpha));
                albedo = col.rgb;

                metallic = lerp(_Metallic, _Metallic_T, lavaMask);
                smoothness = lerp(_Roughness, _Roughness_T, lavaMask);
                diffThreshold = lerp(_DiffThreshold, _DiffThreshold_T, lavaMask);

                // normal
                normal = UnpackNormalYoukia(n, _NormalStrength);
                fixed3 normalLaval = UnpackNormalYoukia(lavaN, _NormalStrength);
                normal = lerp(normal, normalLaval, lavaMask);
            }

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S

                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW[3] : TEXCOORD2;
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

                    half2 uvLava : TEXCOORD14;
                    
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    // vertex noise
                    v.vertex.xyz += VertNoise(v.texcoord2, v.normal);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord2, _LavaTex_B);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _MaskMap);
                    o.uvLava.xy = TRANSFORM_TEX(v.texcoord2, _LavaTex_T);
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

                    o.TtoW[0] = half4(worldTangent.x, worldBinormal.x, worldNormal.x, viewDirTangent.x);
                    o.TtoW[1] = half4(worldTangent.y, worldBinormal.y, worldNormal.y, viewDirTangent.y);
                    o.TtoW[2] = half4(worldTangent.z, worldBinormal.z, worldNormal.z, viewDirTangent.z);

                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
                    
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
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    half3 viewDirTangent = normalize(half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w));

                    half4 col = 1;
                    half3 albedo = 0;
                    half3 normal = 0;
                    half alpha = 1;
                    half metallic = 0;
                    half smoothness = 0;

                    Lava(viewDirTangent, i.uv.zw, i.uv.xy, i.uvLava.xy, albedo, normal, alpha, metallic, smoothness, _DiffThreshold);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // ao
                    half3 ao = 1;

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
                    albedo *= ao;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
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
                        UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, atten);
                    #endif

                    col.a = alpha;

                    // height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif

                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
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
			ZTest LEqual
			Cull[_cull]
            // ColorMask RGB

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd
                
                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW[3] : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    UNITY_LIGHTING_COORDS(6, 7)
                    fixed3 viewDir : TEXCOORD8;
                    fixed3 lightDir : TEXCOORD9;

                    half3 giColor : TEXCOORD10;

                    half2 uvLava : TEXCOORD11;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    // vertex noise
                    v.vertex.xyz += VertNoise(v.texcoord2, v.normal);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord2, _LavaTex_B);
                    o.uv.zw = v.texcoord;
                    o.uvLava.xy = TRANSFORM_TEX(v.texcoord2, _LavaTex_T);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                    
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    // 切线空间view dir
                    TANGENT_SPACE_ROTATION;
                    half3 viewDirTangent = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                    o.TtoW[0] = half4(worldTangent.x, worldBinormal.x, worldNormal.x, viewDirTangent.x);
                    o.TtoW[1] = half4(worldTangent.y, worldBinormal.y, worldNormal.y, viewDirTangent.y);
                    o.TtoW[2] = half4(worldTangent.z, worldBinormal.z, worldNormal.z, viewDirTangent.z);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    half3 viewDirTangent = normalize(half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w));

                    half4 col = 1;
                    half3 albedo = 0;
                    half3 normal = 0;
                    half alpha = 1;
                    half metallic = 0;
                    half smoothness = 0;

                    Lava(viewDirTangent, i.uv.zw, i.uv.xy, i.uvLava.xy, albedo, normal, alpha, metallic, smoothness, _DiffThreshold);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

                    fixed3 colShadow = atten;
                    
                    // UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;
                    
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

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
            #pragma vertex vertShadowLava
            #pragma fragment fragShadowDetail
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            void vertShadowLava (appdata_shadow v, 
                out float4 opos : SV_POSITION,
                out v2fShadow o)
            {
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SETUP_INSTANCE_ID(v);
                // vertex noise
                v.vertex.xyz += VertNoise(v.uv1, v.normal);
                TRANSFER_SHADOW_CASTER_NOPOS(o,opos)

                o.tex.xy = v.uv0;

                #if _USE_DISSOLVE
                    o.tex.zw = TRANSFORM_TEX(v.uv0, _DissolveTex);
                #endif
            }

            ENDCG
        }

	}

	FallBack "VertexLit"
    CustomEditor "YoukiaLavaInspector"
}
