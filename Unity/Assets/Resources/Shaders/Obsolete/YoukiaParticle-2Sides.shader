Shader "YoukiaEngine/Obsolete/YoukiaParticle-2Sides"
{
    Properties
    {
		[HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
		[HDR]_ColorBack ("背面颜色", Color) = (1, 1, 1, 1)
		_Alpha ("Alpha", Range(0, 5)) = 1
        _MainTex ("纹理", 2D) = "white" {}
		// 极坐标
		[Header(UV Polar)]
		[MaterialToggle]_UV_Polar_Main("uv 极坐标(中心方向)", float) = 0
		_PolarMainTiling ("极坐标缩放(Tiling: x, y; Offset: z, w)", vector) = (1, 1, 0, 0)

		[Header(Soft Particle)]
		// [Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0

		[Header(UV speed)]
		_UV_Speed_U_Main ("纹理U方向速度", float) = 0
		_UV_Speed_V_Main ("纹理V方向速度", float) = 0
		[Header(UV rotation)]
		[MaterialToggle]_UVRot("uv 旋转", float) = 0
		_MainTexAngle ("纹理旋转", Range(0, 1)) = 0

		// [Header(Detial)]
		// // [Toggle(_USE_DETAIL)]_Toggle_USE_DETAIL_ON("细节纹理", float) = 0
		// _DetailTex ("细节纹理", 2D) = "white" {}
		// [HDR]_DetailColor ("细节颜色", Color) = (1, 1, 1, 1)
		// // 极坐标
		// [Header(UV Polar)]
		// [MaterialToggle]_UV_Polar_Detial("uv 极坐标(中心方向)", float) = 0
		// _PolarDetialTiling ("极坐标缩放(Tiling: x, y; Offset: z, w)", vector) = (1, 1, 0, 0)
		// [Space(5)]
		// _UV_Speed_U_Detail ("纹理U方向速度", float) = 0
		// _UV_Speed_V_Detail ("纹理V方向速度", float) = 0
		// [MaterialToggle]_UVRot_Detail("细节纹理uv 旋转", float) = 0
		// _DetailTexAngle ("细节纹理旋转", Range(0, 1)) = 0
		// _DetailTexColorAdd ("细节纹理颜色叠加", Range(0, 1)) = 0
		// _DetailTexColorLerp ("细节纹理颜色混合", Range(0, 1)) = 0
		// _DetailTexAlphaAdd ("细节纹理alpha强度", Range(0, 10)) = 1
		// _DetailTexAlphaLerp ("细节纹理alpha混合", Range(0, 1)) = 0

		[Header(Mask)]
		// [Toggle(_USE_MASK)]_Toggle_USE_MASK_ON("遮罩纹理", float) = 0
		_MaskTex ("遮罩纹理", 2D) = "white" {}
		_UV_Speed_U_Mask ("纹理U方向速度", float) = 0
		_UV_Speed_V_Mask ("纹理V方向速度", float) = 0
		[MaterialToggle]_UVRot_Mask("遮罩纹理uv 旋转", float) = 0
		_MaskTexAngle ("遮罩纹理旋转", Range(0, 1)) = 0
		_MaskTexRotSpeed ("遮罩旋转速度", float) = 0
		_MaskTexColor ("遮罩系数", Color) = (0, 0, 0, 1)

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
		_DissolveStrength ("溶解度", Range(0, 1.1)) = 0
		[HDR]_DissolveColor ("溶解边缘颜色", Color) = (1, 1, 1, 1)
		_DissolveWidth ("溶解边缘宽度", Range(0, 0.5)) = 0 
		_DissolveSmooth ("边缘硬度", Range(0, 1)) = 0

		// [Header(ChangeColor)]
		// // [Toggle(_USE_CHANGECOLOR)]_Toggle_USE_CHANGECOLOR_ON("换色", float) = 0
		// [HDR]_ChangeColor ("换色", Color) = (0.5, 0.5, 0.5, 1)

		// [Header(Bloom)]
        // _BloomThreshold ("Bloom 阈值(作用于alpha mask)", Range(0, 1)) = 1

		[Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[HideInInspector][Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Pass
        {
			Tags { "LightMode"="ForwardBase" }
			
			// ColorMask RGB
			Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull Front
			Lighting Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment fragBack
            #pragma target 2.0
            // #pragma multi_compile_fog
			#pragma multi_compile_particles

			#pragma shader_feature _USE_SOFTPARTICLE
			// #pragma shader_feature _USE_DETAIL
			#pragma shader_feature _USE_MASK
			#pragma shader_feature _USE_DISTORT
			#pragma shader_feature _USE_RIM
			#pragma shader_feature _USE_DISSOLVE
			// #pragma shader_feature _USE_CHANGECOLOR
			#pragma multi_compile __ _UNITY_RENDER

			#include "../Library/ParticleLibrary.cginc"
           
            ENDCG
        }

		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			
			// ColorMask RGB
			Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull Back
			Lighting Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment fragFront
            #pragma target 2.0
            // #pragma multi_compile_fog
			#pragma multi_compile_particles

			#pragma shader_feature _USE_SOFTPARTICLE
			// #pragma shader_feature _USE_DETAIL
			#pragma shader_feature _USE_MASK
			#pragma shader_feature _USE_DISTORT
			#pragma shader_feature _USE_RIM
			#pragma shader_feature _USE_DISSOLVE
			// #pragma shader_feature _USE_CHANGECOLOR
			#pragma multi_compile __ _UNITY_RENDER

			#include "../Library/ParticleLibrary.cginc"
           
            ENDCG
		}
    }
	CustomEditor "YoukiaParticleInspector2Sides"
}
