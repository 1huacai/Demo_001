Shader "YoukiaEngine/Particle/YoukiaParticle-UI"
{
    Properties
    {
		[HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
        _MainTex ("纹理", 2D) = "white" {}

		[Header(UV speed)]
		_UV_Speed_U_Main ("纹理U方向速度", float) = 0
		_UV_Speed_V_Main ("纹理V方向速度", float) = 0
		[Header(UV rotation)]
		[MaterialToggle]_UVRot("uv 旋转", float) = 0
		_MainTexAngle ("纹理旋转", Range(0, 1)) = 0

		[Header(Mask)]
		_MaskTex ("遮罩纹理", 2D) = "white" {}
		_UV_Speed_U_Mask ("纹理U方向速度", float) = 0
		_UV_Speed_V_Mask ("纹理V方向速度", float) = 0
		[MaterialToggle]_UVRot_Mask("遮罩纹理uv 旋转", float) = 0
		_MaskTexAngle ("遮罩纹理旋转", Range(0, 1)) = 0
		_MaskTexColor ("遮罩系数", Color) = (0, 0, 0, 1)

		[Header(Move)]
		_MoveTex ("流光纹理", 2D) = "black" {}
		_MoveColor ("流光颜色", Color) = (1, 1, 1, 1)
		_UV_Speed_U_Move ("纹理U方向速度", float) = 0
		_UV_Speed_V_Move ("纹理V方向速度", float) = 0

		[Header(Distort)]
		_DistortTex ("扭曲纹理", 2D) = "whiter" {}
		_DistortStrength ("扭曲强度", Range(0, 5)) = 0
		_DistortTex_Speed_U ("U 方向扭曲速度", float) = 0
		_DistortTex_Speed_V ("V 方向扭曲速度", float) = 0

		[Header(Dissolve)]
		[MaterialToggle]_DissolveAlpha("随粒子alpha值溶解", float) = 0
		_DissolveTex ("溶解贴图", 2D) = "white" {}
		_DissolveStrength ("溶解度", Range(0, 1.1)) = 0
		[HDR]_DissolveColor ("溶解边缘颜色", Color) = (1, 1, 1, 1)
		_DissolveWidth ("溶解边缘宽度", Range(0, 0.5)) = 0 
		_DissolveSmooth ("边缘硬度", Range(0, 1)) = 0

		[Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10

		[HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector]_Stencil ("Stencil ID", Float) = 0
		[HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector]_ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		
        Pass
        {
			Tags { "LightMode"="ForwardBase" }
			
			Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off

            CGPROGRAM
            #pragma vertex vertUI
            #pragma fragment fragUIEffect
            #pragma target 2.0
			#pragma multi_compile_particles

			// #pragma shader_feature _USE_DETAIL
			#pragma shader_feature _USE_MASK
			#pragma shader_feature _USE_MOVE
			#pragma shader_feature _USE_DISTORT
			#pragma shader_feature _USE_DISSOLVE
			#pragma shader_feature _USE_UIMASKCLIP_ON
			
			#include "../Library/ParticleLibrary.cginc"

            ENDCG
        }
    }
	CustomEditor "YoukiaParticleInspectorUI"
	Fallback "Transparent/VertexLit"
}
