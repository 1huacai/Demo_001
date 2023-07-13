//@@@DynamicShaderInfoStart
//Lod1 服装（布料）
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Cloth" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理 (rbg: color, a: alpha)", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理 (rg: normal, b: ao)", 2D) = "white" {}
        _NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (r: Metallic, g: Roughness, b: UV2遮罩, a: UV3 & UV4遮罩)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)

        [Header(Anisotropic)]
        _Anositropy ("各项异性", Range(0, 1)) = 1
        _AnisoDir ("各项异性 方向", Range(0, 360)) = 0
        _SpecAnisoScale ("各项异性-固有色颜色差值", range(0, 1)) = 0
        [HDR]_SpecAnisoColor ("各项异性自定义颜色", Color) = (1, 1, 1, 1)

        [Header(UV2(Detail))]
        // _DetialTex ("细节纹理", 2D) = "white" {}
        _DetialBumpMetaMap ("细节法线金属纹理 (rg: normal, b: metallic, a: roughness)", 2D) = "white" {}

        [Header(UV3)]
        _UV3Tex("UV3 纹理 (rbg: color, a: alpha)", 2D) = "white" {}
        [NoScaleOffset] _UV3BumpMap("UV3 法线纹理 (rg: normal, b: metallic, a: roughness)", 2D) = "white" {}
        [Header(UV4)]
        _UV4Tex("UV4 纹理2 (rbg: color, a: alpha)", 2D) = "white" {}
        [NoScaleOffset] _UV4BumpMap("UV4 法线纹理 (rg: normal, b: metallic, a: roughness)", 2D) = "white" {}

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

        [Header(Shadow)]
        _ShadowAlpha ("阴影透明#透明度小于此值不会产生投影。", Range(0, 1)) = 0

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
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_CharacterPBR_Lod1" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 3.0

            #define _ANISO_PURE 1

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "YoukiaCharacterLod1.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
            
            struct appdata_uv4
            {
                half4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                fixed2 texcoord1 : TEXCOORD1;
                fixed2 texcoord2 : TEXCOORD2;
                fixed2 texcoord3 : TEXCOORD3;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            sampler2D _DetialBumpMetaMap;
            half4 _DetialBumpMetaMap_ST;

            sampler2D _UV3Tex;
            half4 _UV3Tex_ST;
            sampler2D _UV3BumpMap;

            sampler2D _UV4Tex;
            half4 _UV4Tex_ST;
            sampler2D _UV4BumpMap;

            // 采样 uv3 & 4
            inline void SampleUV(sampler2D uvTex, sampler2D uvBumpTex, half2 uv, out half4 cUV, out half2 nUV, out half2 mUV, out half alphaUV, half uvMask)
            {
                cUV = tex2D(uvTex, uv);
                // rg: normal, g: metallic, a: roughness
                fixed4 n = tex2D(uvBumpTex, uv);
                alphaUV = cUV.a * uvMask;
                nUV = n.rg;
                mUV = n.ba;
            }

            inline void SampleUV(half2 uv2, half2 uv3, half2 uv4, in out half3 albedo, in out half4 n, in out half4 m)
            {
                half uvMask2 = m.b;
                half uvMask3 = m.a;

                // uv2 detial
                half4 nDetial = tex2D(_DetialBumpMetaMap, uv2);
                n.rg = n.rg * (1 - uvMask2) + nDetial.rg * uvMask2;
                m.rg = m.rg * (1 - uvMask2) + nDetial.ba * uvMask2;

                // uv3
                half4 c3 = 0;
                half2 n3 = 0;
                half2 m3 = 0;
                half alpha3 = 0;
                SampleUV(_UV3Tex, _UV3BumpMap, uv3, c3, n3, m3, alpha3, uvMask3);

                // uv4
                half4 c4 = 0;
                half2 n4 = 0;
                half2 m4 = 0;
                half alpha4 = 0;
                SampleUV(_UV4Tex, _UV4BumpMap, uv4, c4, n4, m4, alpha4, uvMask3);

                half4 cUV = saturate(c3 * alpha3 + c4 * alpha4);
                half2 nUV = saturate(n3 * alpha3 + n4 * alpha4);
                half2 mUV = saturate(m3 * alpha3 + m4 * alpha4);
                half alphaUV = saturate(alpha3 + alpha4);
                albedo = albedo * (1 - alphaUV) + cUV;
                n.rg = n.rg * (1 - alphaUV) + nUV;
                m.rg = m.rg * (1 - alphaUV) + mUV;
            }

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
                #pragma shader_feature _SPARKLE

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    fixed4 uv2 : TEXCOORD1;
                    float4 worldPos : TEXCOORD2;
                    half4 TtoW[3] : TEXCOORD3;  
                    half3 sh : TEXCOORD6;
                    fixed3 viewDir : TEXCOORD7;
                    fixed3 lightDir : TEXCOORD8;
                    half3 giColor : TEXCOORD9;
                    YOUKIA_RIMLIGHT_DECLARE(10)

                    // YOUKIA_ATMOSPERE_DECLARE(10, 11)
                    YOUKIA_LIGHTING_COORDS(12, 13)
                    // YOUKIA_HEIGHTFOG_DECLARE(14)

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_uv4 v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord1, _DetialBumpMetaMap);
                    o.uv2.xy = TRANSFORM_TEX(v.texcoord2, _UV3Tex);
                    o.uv2.zw = TRANSFORM_TEX(v.texcoord3, _UV4Tex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2WAniso(v.normal, v.tangent, o.viewDir.xyz, o.TtoW, worldNormal);
                    // sh
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

                    YOUKIA_TRANSFER_RIMLIGHT(o, worldNormal);

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
                    half2 uv2 = i.uv.zw;
                    half2 uv3 = i.uv2.xy;
                    half2 uv4 = i.uv2.zw;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    // r: m, g: r, b: uv2 遮罩, a: uv3&4遮罩
                    fixed4 m = tex2D(_MetallicMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    clip(col.a - _Cutoff);
                    fixed3 albedo = col.rgb;

                    // normal
                    fixed3 normal = UnpackNormalYoukia(n, _NormalStrength);
                    half3 wNormal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
                    i.TtoW[0].z = wNormal.x;
                    i.TtoW[1].z = wNormal.y;
                    i.TtoW[2].z = wNormal.z;

                    SampleUV(uv2, uv3, uv4, albedo, n, m);

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);

                    // ao
                    half3 ao = OcclusionCalc(n.b);

                    // normal
                    normal = WorldNormalCullOff(n, i.TtoW, viewDir);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // shadow
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao);

                    // Anisotropic
                    half3 b = Binormal(normal, i.TtoW);

                    // sparkle
                    #if _SPARKLE
                        albedo = Sparkle(albedo, uv, normal, viewDir);
                    #endif

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Aniso_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, b);
                    
                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
                        col += BRDF_Aniso_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, 1, b);
                    #endif

                    // rim light
                    YOUKIA_RIMLIGHT(col, i, atten);

                    // // height fog
                    // YOUKIA_HEIGHTFOG(col, i)
                    // // atmosphere
                    // YOUKIA_ATMOSPHERE(col, i)
                    col.a = alpha;

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
                    fixed4 uv : TEXCOORD0;
                    fixed4 uv2 : TEXCOORD1;
                    float4 worldPos : TEXCOORD2;

                    half4 TtoW[3] : TEXCOORD3;  
                    half3 sh : TEXCOORD6;

                    UNITY_LIGHTING_COORDS(7, 8)

                    fixed3 viewDir : TEXCOORD9;
                    fixed3 lightDir : TEXCOORD10;

                    half3 giColor : TEXCOORD11;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_uv4 v) 
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    UNITY_INITIALIZE_OUTPUT(v2f,o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord1, _DetialBumpMetaMap);
                    o.uv2.xy = TRANSFORM_TEX(v.texcoord2, _UV3Tex);
                    o.uv2.zw = TRANSFORM_TEX(v.texcoord3, _UV4Tex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2WAniso(v.normal, v.tangent, o.viewDir.xyz, o.TtoW, worldNormal);
                    
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
                    half2 uv2 = i.uv.zw;
                    half2 uv3 = i.uv2.xy;
                    half2 uv4 = i.uv2.zw;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    // r: m, g: r, b: sss, a: 各项异性
                    fixed4 m = tex2D(_MetallicMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    // normal
                    fixed3 normal = UnpackNormalYoukia(n, _NormalStrength);
                    half3 wNormal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
                    i.TtoW[0].z = wNormal.x;
                    i.TtoW[1].z = wNormal.y;
                    i.TtoW[2].z = wNormal.z;

                    SampleUV(uv2, uv3, uv4, albedo, n, m);

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);

                    // normal
                    normal = WorldNormal(n, i.TtoW);
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;

                    // Anisotropic
                    half3 b = Binormal(normal, i.TtoW);

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Aniso_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, b);

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
    CustomEditor "Lod1CharacterPBRClothInspector"
}
