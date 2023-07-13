// CSM - ESM
// ref: https://zhuanlan.zhihu.com/p/399462707
// ref: https://zhuanlan.zhihu.com/p/382202359
// 生成场景静态阴影, 只渲染场景, 不处理角色、怪物。
// 可以测试VSM

Shader "Hidden/Shadow/CastShadowStatic"
{
	CGINCLUDE
		// #pragma multi_compile_shadowcaster
		#pragma multi_compile_instancing

		#include "UnityCG.cginc" 
		#include "../Library/YoukiaTools.cginc"

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Opaque"}
		
		Pass
		{
			Name "CastShadow_ST_Opaque_Static"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_PBRDetail"}
		
		Pass
		{
			Name "CastShadow_ST_PBRDetail_Static"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_PBRDetailCutOff"}
		
		Pass
		{
			Name "CastShadow_ST_PBRDetailCutOff_Static"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment fragDetailCutOff
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "CastShadowLib.cginc"

				half4 fragDetailCutOff(v2f i) : SV_Target
				{	
					fixed alpha = tex2D(_MetallicMap, i.depth.xy);
					clip(alpha * _Color.a - _Cutoff);

					half depth = i.depth.z;
					return EncodeShadow(depth);
				}

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_ShadowCaster"}
		
		Pass
		{
			Name "CastShadow_ST_ShadowCaster_Static"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#define _TREE 1
				#pragma multi_compile_instancing
				#pragma vertex vertPlant
				#pragma fragment fragShadowCaster
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile __ _GWIND

				#include "CastShadowLib.cginc"

				half4 fragShadowCaster(v2f i) : SV_Target
				{	
					UNITY_SETUP_INSTANCE_ID(i);

					fixed4 texcol = tex2D(_MainTex, i.depth.xy);
					clip(texcol.r - _Cutoff);

					half depth = i.depth.z;
					return EncodeShadow(depth);
				}

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Tree"}
		
		Pass
		{
			Name "CastShadow_ST_Tree"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#define _TREE 1
				#pragma multi_compile_instancing
				#pragma vertex vertPlant
				#pragma fragment fragPlant
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile __ _GWIND

				#include "../Library/YoukiaTools.cginc"
				#include "CastShadowLib.cginc"

			ENDCG
		}
	}
}