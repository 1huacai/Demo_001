//@@@DynamicShaderInfoStart
//Lod1 皮肤 Skin
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Skin" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: 皮肤Specular, B: SSS)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        _Spec ("Specular", Range(0, 1)) = 0.5
        _Spec2 ("Specular 2", Range(0, 4)) = 1
        _SpecScale ("Specular 强度", Range(0, 2)) = 1
        [Header(Mask Map)]
        [NoScaleOffset] _MaskMap ("遮罩纹理(R: , G: , B: 曲率贴图)", 2D) = "white" {}
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)
        _AOSkinColor ("AO 皮肤颜色", Color) = (0, 0, 0, 1)
        
        [Header(Skin Edge)]
        _EdgeOffset ("皮肤边缘范围", Range(0, 0.5)) = 0.2
        _EdgeStrength ("皮肤边缘强度", Range(0, 1)) = 0.5

        [Header(Shadow)]
        [HideInInspector]_ShadowAlpha ("阴影透明#透明度小于此值不会产生投影。", Range(0, 1)) = 0
        
        // sss 散射
        [Header(Scattering)]
        [NoScaleOffset]_SkinMap ("SSS RampMap", 2D) = "white" {}
        _SkinToneScale("SSS scale", Range(0, 1)) = 0.5
        _SkinToneOffset("SSS offset", Range(0, 0.2)) = 0

        // sss 透射
        [Header(Translucency)]
        // [Toggle(_SSS)]_Toggle_SSS_ON("Translucency", Float) = 0
        [HDR]_SSSColor ("透射颜色", Color) = (1, 1, 1, 1)
        _SSSStrength ("透射强度", Range(0, 1)) = 0
        _SSSDistortion ("透射法线干扰", Range(0, 1)) = 0
        _SSSPower ("透射衰减", Range(0.000001, 5)) = 2
        _SSSScale ("Translucency ambient (环境增强)", Range(0, 1)) = 0.7
	}
	SubShader 
    {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_CharacterPBR_Lod1" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "../../Library/YoukiaEffect.cginc"
            #include "YoukiaCharacterLod1.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            // sss
            half4 _SSSColor;
            half _SSSDistortion, _SSSPower, _SSSScale, _SSSStrength;

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

			ZWrite On
			ZTest LEqual
			Cull back

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma shader_feature _SSS
                #pragma multi_compile __ _UNITY_RENDER
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C

                // mask
                sampler2D _MaskMap;

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;  
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;
                    YOUKIA_RIMLIGHT_DECLARE(9)

                    // YOUKIA_ATMOSPERE_DECLARE(9, 10)
                    YOUKIA_LIGHTING_COORDS(11, 12)
                    // YOUKIA_HEIGHTFOG_DECLARE(13)

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
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _DissolveTex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);

                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
                    YOUKIA_TRANSFER_RIMLIGHT(o, worldNormal);
                    // shadow
                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // height fog
                    // YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                    // atmosphere
                    // YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    return o;
                }

                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir.xyz;
                    fixed3 lightDir = i.lightDir;
                    half2 uv = i.uv.xy;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    // r: m, g: r, b: sss, a: 各项异性
                    fixed4 m = tex2D(_MetallicMap, uv);
                    half sss = m.b;
                    // r: rim, g: sparkle, b: curvature
                    // mask map
                    fixed4 mask = tex2D(_MaskMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;
                    half curvature = mask.b;

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessSpecCalc(1 - m.g);
                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    half3 normalLow = half3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);

                    // ao
                    half occlusion = saturate(pow(n.b, _AOCorrect));
                    half3 ao = lerp(1, lerp(_AOColor.rgb, 1, occlusion), _AO);
                    ao = lerp(ao, SkinAO(occlusion, _AO, normal, viewDir, sss), saturate(ceil(sss)));

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // shadow
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao, _gCharacterSkinGIScale);

                    //sss
                    YoukiaSSSData SSS = YOUKIA_SSS(_SSSStrength * sss, _SSSDistortion, _SSSPower, _SSSScale, _SSSColor)

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_SSS_PBS(albedo, specColor, normal, normalLow, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, SSS, curvature);
                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityGI giSub = GetUnityGI_sub(albedo, normalize(_gVSLFwdVec_C), _gVSLColor_C.rgb);
                        col += BRDF_SSS_PBS_SUB(albedo, specColor, normal, normalLow, viewDir, brdfPreData, giSub.light, 1, SSS, curvature);
                    #endif

                    // rim light
                    YOUKIA_RIMLIGHT(col, i, atten);

                    col.a = alpha;// + sss;

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

			Blend One One
            ZWrite Off
			Cull back

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd

                #pragma shader_feature _SSS
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
                    o.uv.z = 1 - v.color.x;
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
                    // r: m, g: r, b: sss, a: 各项异性
                    fixed4 m = tex2D(_MetallicMap, uv);
                    half sss = m.b;
                    half curvature = sss;
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessSpecCalc(1 - m.g);

                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    half3 normalLow = half3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;

                    //sss
                    YoukiaSSSData SSS = YOUKIA_SSS(_SSSStrength * sss, _SSSDistortion, _SSSPower, _SSSScale, _SSSColor)

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_SSS_PBS_Add(albedo, specColor, normal, normalLow, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, SSS, curvature);
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

			ZWrite On ZTest LEqual Cull back
            
            CGPROGRAM
                #pragma vertex vertShadow
                #pragma fragment fragShadowCharacter
                #pragma multi_compile_instancing
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma shader_feature _USE_DISSOLVE
                #pragma fragmentoption ARB_precision_hint_fastest


            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "Lod1CharacterPBRSkinInspector"
}
