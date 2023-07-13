//@@@DynamicShaderInfoStart
//Lod1 极坐标-视差-漩涡效果
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Polar" 
{
	Properties 
    {
        [HDR]_Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)

        [Header(Center)]
        [HDR]_CenterColor ("中心颜色", Color) = (0, 0, 0, 1)
        _CenterRange ("中心范围", Range(0, 1)) = 1
        _CenterPow ("中心过渡", Range(0, 10)) = 1
        _CenterNoiseStrength ("中心噪声强度", Range(0, 1)) = 1

        [Header(Noise)]
        [NoScaleOffset]_NoiseTex ("噪声图", 2D) = "black" {}
        _NoiseTilling ("噪声Tilling", float) = 1
        _NoiseStrength ("噪声强度", Range(0, 1)) = 0
        _NoiseSpeed ("噪声速度 xy", Vector) = (0, 0, 0, 0)

        [Header(Parallax)]
        _ParallaxDepth ("视差深度", Range(-1, 2)) = 0
        [Header(Detial)]
        [HDR]_ParallaxColor ("细节颜色", Color) = (0, 0, 0, 1)
        _ParallaxTex ("细节纹理", 2D) = "black" {}
        _ParallaxDetialDepth ("细节深度", Range(0, 2)) = 0
        _ParallaxDetialDepthInc ("细节深度 渐进", Range(0, 1)) = 0.5
        _ParallaxDetialSpeed ("速度", Range(0, 20)) = 1

        [Header(Sparkle)]
        // [Toggle(_SPARKLE)]_Toggle_SPARKLE_ON("亮点", Float) = 0
        _SparkleTex ("亮点纹理 (rgb: 颜色, a: 遮罩)", 2D) = "black" {}
        [Header(Color)]
        [HDR]_SparkleColor1 ("亮点颜色 1", Color) = (1, 1, 1, 1)
        [HDR]_SparkleColor2 ("亮点颜色 2", Color) = (1, 1, 1, 1)
        _SparkleColor01 ("颜色权重 (0: 颜色1, 1: 颜色2)", Range(0, 1)) = 0.5
        [Header(Size Scale)]
        _SparkleScale ("亮点密度", Range(0, 1000)) = 1
        _SparkleRange ("亮点范围", Range(0, 10)) = 1
        _SparkleSize ("亮点大小 最大值", Range(0, 0.5)) = 0.5
        _SparkleSizeMin ("亮点大小 最小值", Range(0, 0.5)) = 0.2
        _SparkleUVScale ("亮点 UV Scale", vector) = (1, 1, 1, 1)
        [Header(Shine)]
        _SparkleShine ("大小闪烁频率", Range(0, 20)) = 0
        _SparkleShineColor ("颜色闪烁频率", Range(0, 20)) = 0
        [Header(Speed)]
        _SparkleSpeedU ("亮点速度 u", Range(-5, 5)) = 0
        _SparkleSpeedV ("亮点速度 v", Range(-5, 5)) = 0

        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)

        // [Header(CutOut)]
        // _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0

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
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_CharacterPBR_Lod1" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "YoukiaCharacterLod1.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
            // ColorMask RGB

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
                #pragma shader_feature _SPARKLE

                // center
                half4 _CenterColor;
                half _CenterRange, _CenterPow, _CenterNoiseStrength;

                // noise
                sampler2D _NoiseTex;
                half _NoiseTilling, _NoiseStrength;
                half4 _NoiseSpeed;

                // parallax
                half _ParallaxDepth;
                half4 _ParallaxColor;
                sampler2D _ParallaxTex;
                half4 _ParallaxTex_ST;
                half _ParallaxDetialDepth, _ParallaxDetialDepthInc;
                half _ParallaxDetialSpeed;

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW[3] : TEXCOORD2;  
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
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
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _ParallaxTex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal, worldTangent, worldBinormal = 0;
                    T2W(v.normal, v.tangent, worldNormal, worldTangent, worldBinormal); 

                    // 切线空间view dir
                    TANGENT_SPACE_ROTATION;
                    half3 viewDirTangent = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                    T2W_Vec2Array(worldTangent, worldBinormal, worldNormal, o.TtoW, viewDirTangent);
                    
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

                    // shadow
                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // // height fog
                    // YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                    // // atmosphere
                    // YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    return o;
                }

                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    fixed3 lightDir = i.lightDir;
                    half2 uv = i.uv.xy;
                    half3 viewDirTangent = normalize(half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w));

                    // parallax
                    half2 parallaxOffset = ParallaxOffset(0, _ParallaxDepth, viewDirTangent);
                    half2 uvPolar = UVPolar(uv + parallaxOffset);
                    half noise = tex2D(_NoiseTex, uvPolar * half2(_NoiseTilling, 1) + _NoiseSpeed.xy * _Time.x) * _NoiseStrength;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uvPolar + noise);
                    // center
                    half centerFade = saturate(pow(saturate(uvPolar.x + (noise * _CenterNoiseStrength) - _CenterRange), _CenterPow));

                    // parallax detail
                    half seed = (noise * 2 - 1) * 0.5;
                    half time = (_Time.x * _ParallaxDetialSpeed) + seed;
                    half detailFade = time - floor(time);
                    half detialNoise = half2(detailFade, 0);
                    parallaxOffset = ParallaxOffset(0, _ParallaxDetialDepth + detialNoise, viewDirTangent);
                    half4 pDetail = tex2D(_ParallaxTex, i.uv.zw + parallaxOffset) * saturate(sin(detailFade * UNITY_PI));

                    seed = (noise * 2 - 1) * 0.5;
                    time = (_Time.x * _ParallaxDetialSpeed) - seed;
                    detailFade = time - floor(time);
                    detialNoise = half2(detailFade, 0);
                    parallaxOffset = ParallaxOffset(0, _ParallaxDetialDepth + _ParallaxDetialDepthInc + detialNoise, viewDirTangent);
                    pDetail += tex2D(_ParallaxTex, i.uv.zw + parallaxOffset + parallaxOffset) * saturate(sin(detailFade * UNITY_PI));

                    pDetail *= _ParallaxColor * lerp(1, noise, FastSign(_NoiseStrength));

                    c.rgb += pDetail.rgb;
                    c.rgb = lerp(_CenterColor * c.rgb, c.rgb, centerFade);

                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);

                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    // clip(col.a - _Cutoff);

                    fixed3 albedo = col.rgb;

                    // _Roughness *= 0.8f;
                    half metallic = _Metallic;
                    half smoothness = _Roughness;
                    // ao
                    half3 ao = OcclusionCalc(n.b);

                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // shadow
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao);

                    // sparkle
                    #if _SPARKLE
                        albedo = Sparkle(albedo, uv, normal, viewDir);
                    #endif

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Unity_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    
                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityGI giSub = GetUnityGI_sub(albedo, normalize(_gVSLFwdVec_C), _gVSLColor_C.rgb);
                        col += BRDF_Unity_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, giSub.light, giSub.indirect, 1, 1);
                    #endif

                    // emission
                    col.rgb = Emission(col.rgb, c.rgb, n.a);

                    col.a = alpha;
                    // // height fog
                    // YOUKIA_HEIGHTFOG(col, i)
                    // // atmosphere
                    // YOUKIA_ATMOSPHERE(col, i)

                    return OutPutCharacterLod1(col);
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest[_zTest]
			Cull[_cull]
            // ColorMask RGB

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER
                
                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed3 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;

                    half3 TtoW[3] : TEXCOORD2;  
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
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    UNITY_INITIALIZE_OUTPUT(v2f,o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    
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
                    half2 uv = i.uv.xy;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = _Metallic;
                    half smoothness = _Roughness;

                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    col.a = alpha;

                    return col;
                }
            ENDCG
        }

        // shadow
        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
                #pragma vertex vertShadow
                #pragma fragment fragShadowCharacter
                #pragma multi_compile_instancing
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "YoukiaCharacterPBRPolarInspector"
}
