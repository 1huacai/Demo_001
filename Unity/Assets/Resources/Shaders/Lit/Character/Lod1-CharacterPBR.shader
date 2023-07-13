//@@@DynamicShaderInfoStart
//Lod1 基础 PBR
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        [Header(Mask Map)]
        [NoScaleOffset] _MaskMap ("遮罩纹理(R: 边缘光范围)", 2D) = "white" {}
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)

        [Header(Shadow)]
        _ShadowAlpha ("阴影透明#透明度小于此值不会产生投影。", Range(0, 1)) = 0

        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)

        [Header(Rim)]
        [HDR]_RimColor("边缘光颜色", Color) = (0, 0, 0, 1)
        _RimStrength("边缘光强度", Range(0, 1)) = 0
        _RimRange("边缘光范围#遮罩纹理 R 通道控制边缘光范围。", Range(0, 10)) = 0

        // [Header(Dissolve)]
		// // [Toggle(_USE_DISSOLVE)]_Toggle_USE_DISSOLVE_ON("溶解", float) = 0
		// _DissolveTex ("溶解贴图", 2D) = "white" {}
		// _DissolveStrength ("溶解度", Range(0, 1.1)) = 0
		// [HDR]_DissolveColor ("溶解边缘颜色", Color) = (1, 1, 1, 1)
		// _DissolveWidth ("溶解边缘宽度", Range(0, 0.5)) = 0 
		// _DissolveSmooth ("边缘硬度", Range(0, 1)) = 0
        
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        _VertAlpha ("顶点alpha", Range(0, 1)) = 0

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
                // #pragma shader_feature _USE_DISSOLVE
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
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
                    fixed4 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;
                    
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
                    o.viewDir.w = 1 - v.color.r;

                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    // sh
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

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
                    // 顶点alpha
                    half vertAlpha = 1 - i.viewDir.w;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, uv);
                    // r: m, g: r
                    fixed4 m = tex2D(_MetallicMap, uv);
                    // r: rim
                    fixed4 mask = tex2D(_MaskMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;
                    clip(min(col.a - _Cutoff, vertAlpha - _VertAlpha));

                    // dissolve
                    // half4 dissColor = Dissolve(i.uv.zw);

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);
                    // ao
                    half3 ao = OcclusionCalc(n.b);

                    // normal
                    half3 normal = WorldNormalCullOff(n, i.TtoW, viewDir);
                    
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // shadow
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao);
                    // return OutPutDefault(half4(gi.indirect.diffuse, 1));

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
                        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, 1);
                    #endif

                    // emission
                    col.rgb = Emission(col.rgb, c.rgb, n.a);
                    // rim
                    col.rgb = Rim(col.rgb, brdfPreData.nv, mask.r);
                    
                    col.a = alpha;

                    // dissolve
                    // col.rgb = lerp(col.rgb, dissColor.rgb, dissColor.a);

                    // height fog
                    // YOUKIA_HEIGHTFOG(col, i)
                    // atmosphere
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

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER

                struct v2f
                {
                    half4 pos : SV_POSITION;
                    fixed2 uv : TEXCOORD0;
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

                // #pragma shader_feature _USE_DISSOLVE
                #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "Lod1CharacterPBRInspector"
}
