//@@@DynamicShaderInfoStart
//大地图uv2半透Shader(计算法线)，用于 山 等地表物件。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/BigMap/YoukiaPBR-BigMap-Bump-UV2-Fade" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理 (rgb: color, a: AO)", 2D) = "white" {}
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
        [HideInInspector][Toggle] _ReceiveShadow("是否接受阴影,非特殊情况不要修改", Float) = 1      
        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        _EmissionStrength("自发光强度", Range(0, 10)) = 0

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10

        [Header(Hole)]
        Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] CO("Compare OP", Float) = 8 
        [Enum(UnityEngine.Rendering.StencilOp)] SO("Stencil OP", Float) = 0 
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest+6" "RenderType"="Opaque" "ShadowType" = "ST_PBRDetail" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0
            
            #define _BIGMAPGI 1

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"

            #define _UV2 1
            #define _BUMPTEXTURE 1

            #include "YoukiaBigMap.cginc"

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

            Stencil
            {
                Ref [Ref]
                Comp [CO]
                Pass [SO]
            }


			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment fragbase
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S
                // #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _EM

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
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd
                #pragma multi_compile __ _UNITY_RENDER
                
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
    CustomEditor "YoukiaPBRBigMapBumpUV2Fade"
}
