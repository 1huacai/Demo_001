//@@@DynamicShaderInfoStart
//Lod1 透明水晶效果
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Crystal" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        _NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness, B: 流光遮罩)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)

        [Header(Transparent)]
        _TransparentColor ("透明颜色", Color) = (1, 1, 1, 1)
        _TransparentDistort ("透明扰动", Range(0, 4)) = 1

        [Header(Parallax)]
        [HDR]_ParallaxColor_0 ("视差颜色 最外层", Color) = (1, 1, 1, 1)
        [HDR]_ParallaxColor_1 ("视差颜色 最里层", Color) = (1, 1, 1, 1)
        _ParallaxTex ("视差贴图", 2D) = "black" {}
        _ParallaxDepth ("视差深度", Range(-1, 1)) = 0
        _ParallaxInc ("视差深度 递进", Range(0, 0.1)) = 0
        _ParallaxSpeed ("流动速度 xy: 视差", Vector) = (0, 0, 0, 0)

        [Header(Rim)]
        [HDR]_RimColor ("边缘光颜色", Color) = (0, 0, 0, 1)
        _RimStrength("边缘光强度", Range(0, 1)) = 1
        _RimRange ("边缘光范围", Range(0, 10)) = 1

        [Header(Emission)]
        [HDR]_ColorEmission ("自发光颜色", Color) = (0, 0, 0, 1)

        [Header(Flow)]
        [HDR]_FlowColor1 ("流光颜色 1", Color) = (0, 0, 0, 1)
        [HDR]_FlowColor2 ("流光颜色 2", Color) = (0, 0, 0, 1)
        _FlowIntensity ("流光 Intensity", Range(-1, 1)) = 0
        _FlowScale ("流光间隔", Range(0, 100)) = 10
        _FlowSpeed ("流光速度 #UV2 控制流光方向", Range(-10, 10)) = 0
        [NoScaleOffset]_FlowNoise ("流光扰动纹理", 2D) = "black" {}
        _FlowNoiseScale ("流光扰动纹理缩放", Range(0, 20)) = 1
        _FlowNoiseStrength ("流光纹理扰动强度", Range(0, 10)) = 0

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
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "ShadowType" = "ST_CharacterPBR_Lod1" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 3.0
            #define _UV2 1

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
            ColorMask RGB

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C

                sampler2D _ParallaxTex;
                half4 _ParallaxTex_ST;
                half4 _ParallaxColor_0, _ParallaxColor_1;
                half _ParallaxDepth, _ParallaxInc;
                half4 _ParallaxSpeed;

                sampler2D _gGrabTex;
                half4 _TransparentColor;
                half _TransparentDistort;

                sampler2D _FlowNoise;
                half _FlowNoiseScale;
                half _FlowNoiseStrength;
                half _FlowIntensity;
                half4 _FlowColor1, _FlowColor2;
                half _FlowScale;
                half _FlowSpeed;

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half4 TtoW[3] : TEXCOORD2;  
                    half3 sh : TEXCOORD5;
                    half4 viewDir : TEXCOORD6;
                    half4 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

                    // YOUKIA_ATMOSPERE_DECLARE(9, 10)

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(11, 12)
                    #endif
                    // YOUKIA_HEIGHTFOG_DECLARE(13)

                    fixed4 screenPos : TEXCOORD14;

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
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);
                    o.viewDir.w = v.texcoord1.x;
                    o.lightDir.w = v.texcoord1.y;
                    
                    half3 worldNormal, worldTangent, worldBinormal = 0;
                    T2W(v.normal, v.tangent, worldNormal, worldTangent, worldBinormal);

                    // 切线空间view dir
                    TANGENT_SPACE_ROTATION;
                    half3 viewDirTangent = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                    T2W_Vec2Array(worldTangent, worldBinormal, worldNormal, o.TtoW, viewDirTangent);
                    
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #endif
                    o.screenPos = ComputeScreenPos(o.pos);

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
				    fixed3 viewDir = i.viewDir.xyz;
                    fixed3 lightDir = i.lightDir.xyz;
                    half2 uv = i.uv.xy;
                    half2 uv2 = half2(i.viewDir.w, i.lightDir.w);
                    half3 viewDirTangent = normalize(half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w));

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    // r: m, g: r, b: 流光遮罩
                    fixed4 m = tex2D(_MetallicMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    clip(col.a - _Cutoff);

                    fixed3 albedo = col.rgb;

                    // parallax
                    half2 parallaxOffset_0 = ParallaxOffset(0, _ParallaxDepth, viewDirTangent);
                    half2 parallaxOffset_1 = ParallaxOffset(0, _ParallaxDepth + _ParallaxInc, viewDirTangent);
                    half2 parallaxOffset_2 = ParallaxOffset(0, _ParallaxDepth + _ParallaxInc + _ParallaxInc, viewDirTangent);
                    half2 parallaxOffset_3 = ParallaxOffset(0, _ParallaxDepth + _ParallaxInc + _ParallaxInc + _ParallaxInc, viewDirTangent);

                    fixed4 parallax_0 = tex2D(_ParallaxTex, i.uv.zw + parallaxOffset_0 + _ParallaxSpeed.xy * _Time.x) * _ParallaxColor_0;
                    fixed4 parallax_1 = tex2D(_ParallaxTex, i.uv.zw + parallaxOffset_1 + _ParallaxSpeed.xy * _Time.x) * lerp(_ParallaxColor_0, _ParallaxColor_1, 0.33f);
                    fixed4 parallax_2 = tex2D(_ParallaxTex, i.uv.zw + parallaxOffset_2 + _ParallaxSpeed.xy * _Time.x) * lerp(_ParallaxColor_0, _ParallaxColor_1, 0.66f);
                    fixed4 parallax_3 = tex2D(_ParallaxTex, i.uv.zw + parallaxOffset_3 + _ParallaxSpeed.xy * _Time.x) * _ParallaxColor_1;
                    albedo.rgb = albedo.rgb + (parallax_0.rgb + parallax_1.rgb + parallax_2.rgb + parallax_3.rgb) * (1 - alpha);

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);
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

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // grab 
                    half f = pow(saturate(1 - brdfPreData.nv), _TransparentDistort);
                    half4 colGrab = tex2D(_gGrabTex, i.screenPos.xy / i.screenPos.w + f) * _TransparentColor;

                    col = BRDF_Unity_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    
                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityGI giSub = GetUnityGI_sub(albedo, normalize(_gVSLFwdVec_C), _gVSLColor_C.rgb);
                        col += BRDF_Unity_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, giSub.light, giSub.indirect, 1, 1);
                    #endif

                    // emission
                    col.rgb = Emission(col.rgb, c.rgb, n.a);
                    // 
                    col = lerp(colGrab, col, alpha);
                    // 流光
                    half flowMask = m.b;
                    half4 flowNoiseTex = tex2D(_FlowNoise, uv * _FlowNoiseScale);
                    half flowNoise = (flowNoiseTex.r * flowNoiseTex.a);
                    half4 flowColor = lerp(_FlowColor1, _FlowColor2, saturate(flowNoise));
                    flowNoise *= _FlowNoiseStrength;
                    half flow = sin(uv2.y * _FlowScale - _Time.y * _FlowSpeed + flowNoise) + _FlowIntensity;
                    col.rgb += saturate(flow) * flowColor * flowMask;
                    // rim
                    col.rgb = Rim(col.rgb, brdfPreData.nv);

                    col.a = 1;
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
                    // r: m, g: r
                    fixed4 m = tex2D(_MetallicMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);

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
    CustomEditor "YoukiaCharacterPBRCrystalInspector"
}
