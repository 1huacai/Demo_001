Shader "YoukiaEngine/Particle/YoukiaParticle"
{
    Properties
    {
		[HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
		_Alpha ("Alpha", Range(0, 5)) = 1
        _MainTex ("纹理", 2D) = "white" {}
		[MaterialToggle] _CustomDataMainOffset("CustomData 主纹理Offset#Custom Data2: xy", float) = 0
	
		// 极坐标
		[Header(UV Polar)]
		[MaterialToggle]_UV_Polar_Main("uv 极坐标(中心方向)", float) = 0
		_PolarMainTiling ("极坐标缩放(Tiling: x, y; Offset: z, w)", vector) = (1, 1, 0, 0)

		[MaterialToggle]_PolarOffsetData("CustomData 极坐标Offset#Custom Data2: xy", float) = 0

		[Header(Soft Particle)]
		// [Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0

		[Header(UV speed)]
		_UV_Speed_U_Main ("纹理U方向速度", float) = 0
		_UV_Speed_V_Main ("纹理V方向速度", float) = 0
		[Header(UV rotation)]
		[MaterialToggle]_UVRot("uv 旋转", float) = 0
		_MainTexAngle ("纹理旋转", Range(0, 1)) = 0

		[Header(Mask)]
		// [Toggle(_USE_MASK)]_USE_MASK("遮罩纹理", float) = 0
		_MaskTex ("遮罩纹理", 2D) = "white" {}
		_UV_Speed_U_Mask ("纹理U方向速度", float) = 0
		_UV_Speed_V_Mask ("纹理V方向速度", float) = 0
		[MaterialToggle]_UVRot_Mask("遮罩纹理uv 旋转", float) = 0
		_MaskTexAngle ("遮罩纹理旋转", Range(0, 1)) = 0
		_MaskTexRotSpeed ("遮罩旋转速度", float) = 0
		[HDR]_MaskTexColor ("遮罩系数", Color) = (0, 0, 0, 1)

		[MaterialToggle] _MaskUvOffsetMain("CustomData 遮罩UV Offset#Custom Data1: yz", float) = 0

		[Header(Distort)]
		// [Toggle(_USE_DISTORT)]_Toggle_USE_DISTORT_ON("扭曲", float) = 0
		_DistortTex ("扭曲纹理", 2D) = "whiter" {}
		_DistortStrength ("扭曲强度", Range(0, 5)) = 0
		_DistortTex_Speed_U ("U 方向扭曲速度", float) = 0
		_DistortTex_Speed_V ("V 方向扭曲速度", float) = 0
		[MaterialToggle]_UVRot_DistortTex("扭曲纹理uv 旋转", float) = 0
		_DistortTexRotSpeed ("扭曲旋转速度", float) = 0

		[Header(Rim)]
		// [Toggle(_USE_RIM)]_Toggle_USE_RIM_ON("边缘光", float) = 0
		[HDR]_RimColor ("边缘光颜色", Color) = (1, 1, 1, 1)
		_RimPower ("边缘光强度", Range(0, 10)) = 1
		[MaterialToggle]_RimAlpha("边缘光Alpha遮罩", float) = 0
		[MaterialToggle]_RimRevert("边缘光Alpha遮罩反向", float) = 0

		[Header(Dissolve)]
		// [Toggle(_USE_DISSOLVE)]_Toggle_USE_DISSOLVE_ON("溶解", float) = 0
		[MaterialToggle]_DissolveAlpha("随粒子alpha值溶解", float) = 0
		_DissolveTex ("溶解贴图", 2D) = "white" {}
		[MaterialToggle]_CustomDataDissolveOffset("CustomData 溶解纹理Offset#Custom Data2: zw", float) = 0
		// _DissolveUVOffset("CustomOffset影响系数-相乘", Range(0, 5)) = 1
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
		_Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Pass
        {
			Tags { "LightMode"="ForwardBase" }
			
			Stencil
			{
				Ref [_Ref]
				Comp [_Comp]
				Pass [_Pass]
			}

			// ColorMask RGB
			Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragFront
            // #pragma target 2.0
			// #pragma multi_compile_particles
			#pragma multi_compile_instancing
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature _USE_SOFTPARTICLE
			#pragma shader_feature _USE_MASK
			#pragma shader_feature _USE_DISTORT
			#pragma shader_feature _USE_RIM
			#pragma shader_feature _USE_DISSOLVE
			#pragma multi_compile __ _UNITY_RENDER
			
			#include "../Library/ParticleLibrary.cginc"

            ENDCG
        }
    }
	CustomEditor "YoukiaParticleInspector"
	Fallback "Transparent/VertexLit"
}
