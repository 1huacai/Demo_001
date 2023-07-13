//@@@DynamicShaderInfoStart
//大地图PBR uber Shader，UV2 采样 AO-贴花mask, 支持 2 bones GPU 动画
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/BigMap/YoukiaPBR-BigMap-Uber" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理 (rgb: color, a: alpha)", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Others)]
        _MetallicMap ("(R: AO(UV2), G: 贴花Mask(UV2))", 2D) = "white" {}
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 0
        [Toggle] _ReceiveShadow("是否接受阴影,非特殊情况不要修改", Float) = 1      
        [Header(TopTex)]
        [Enum(TopType)] _TopType("Toptex Type", Float) = 0
        [Header(Emission)]
        _EmissionStrength("自发光强度", Range(0, 10)) = 0
        _EmissionTex("自发光纹理(UV1)", 2D) = "black" {}

        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0

        [Header(Anim)]
        [Header(GPU)]
        [Toggle(_GPUAnimation)] _GPUAnimation("Enable GPUAnimation", Float) = 0
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimControl("AnimControl", vector) = (0,0,0,0)
		_AnimStatus("_AnimStatus", vector) = (0,0,0,0)

        
        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        //_Ref("Stencil Reference", Range(0, 255)) = 0
        //[Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 3
        //[Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0 
	}
	SubShader 
    {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_PBRBigMapUV2" "Reflection" = "RenderReflectionOpaque" }

            //Stencil
            //{
            //    Ref [_Ref]
            //    Comp [_Comp]
            //    Pass [_Pass]
            //}


        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #define _BIGMAPGI 1

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/YoukiaEffect.cginc"

            #define _BUMPTEXTURE 1
            #define _UV2_MASK 1
            #pragma shader_feature _GPUAnimation

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

			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment fragbase
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S
                #pragma multi_compile __ _TOPTEX
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
                #pragma multi_compile_fwdadd//_fullshadows

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
            #pragma fragment fragShadowBigmapUber
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "YoukiaPBRBigMapUber"
}
