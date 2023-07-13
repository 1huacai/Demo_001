Shader "YoukiaEngine/Lit/YoukiaPBR-UV2-Fade" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理 (rgb: color, a: AO(顶面贴花Mask))", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Others)]
        [NoScaleOffset] _MetallicMap ("(R: 透明度, G: 自发光)", 2D) = "white" {}
        [Header(AO)]
        _AO ("AO强度(固有色)", Range(0, 1)) = 0.5
        _AOEnvir ("AO强度(环境)", Range(0, 1)) = 0.5
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        [Header(Shadow)]
        [Toggle] _ReceiveShadow("是否接受阴影,非特殊情况不要修改", Float) = 1      
        [Header(Fade)]
        [MaterialToggle]_UV2Fade("使用UV2半透", float) = 1
        _FadeScale ("半透过渡#透明度根据UV2计算。", Range(0, 2)) = 1
        [Header(Dissolve)]
        // [Toggle(_USE_DISSOLVE)]_Toggle_USE_DISSOLVE_ON("溶解", float) = 0
        _DissolveStrength ("溶解强度#根据UV2计算。", Range(0, 1)) = 0
        [HDR]_DissolveColor ("溶解颜色", Color) = (1, 1, 1, 1)
        _DissolveTex ("溶解噪声图", 2D) = "white" {}
        _DissolveTexScale ("溶解噪声图缩放", Range(0, 10)) = 0.1
        _DissolveNoiseStrength ("溶解噪声强度", Range(0, 1)) = 0
        _DissolveWidth ("溶解边缘宽度", Range(0, 5)) = 0 
		_DissolveSmooth ("边缘硬度", Range(0, 1)) = 0
        [Header(TopTex)]
        //[Enum(TopType)] _TopType("Toptex Type", Float) = 0
        [MaterialToggle]_TopTex ("顶面贴花", float) = 0
        [MaterialToggle]_TopTexMask ("根据遮罩计算(_MainTex-A通道)", float) = 0
        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (1, 1, 1, 1)
        _EmissionStrength("自发光强度", Range(0, 10)) = 1
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
		Tags { "Queue"="AlphaTest+6" "RenderType"="Opaque" "ShadowType" = "ST_PBRDetailUV2Fade" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #define _ALPHACUT 1
            #define _UV2_FADE 1

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"

            // #define _EMISSION_ENBALE 1
            #include "YoukiaPBR.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        ENDCG

		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment fragbase
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S
                #pragma multi_compile __ _TOPTEX
                #pragma shader_feature _USE_DISSOLVE
                
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
			ENDCG
		}

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _TOPTEX
                
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadowDetailCutOff
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #pragma shader_feature _USE_DISSOLVE
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "YoukiaPBRDetialUV2Fade"
}
