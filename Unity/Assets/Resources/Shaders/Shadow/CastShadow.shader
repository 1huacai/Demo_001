Shader "Hidden/Shadow/CastShadow"
{
	CGINCLUDE
		#include "UnityCG.cginc" 
		#include "../Library/YoukiaTools.cginc"

		#pragma multi_compile __ _SHADOW_SSS
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Opaque"}
		
		Pass
		{
			Name "CastShadow_ST_Opaque"
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
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_CharacterPBR_Lod1"}
		
		Pass
		{
			Name "CastShadow_ST_CharacterPBR_Lod1"
			ZWrite On
			ZTest LEqual
			Cull[_cull]
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment fragCharacterPBR
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_CharacterPBR"}
		
		Pass
		{
			Name "CastShadow_ST_CharacterPBR"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment fragCharacterPBR
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
			Name "CastShadow_ST_PBRDetail"
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
			Name "CastShadow_ST_PBRDetailCutOff"
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
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_PBRDetailUV2Fade"}
		
		Pass
		{
			Name "CastShadow_ST_PBRDetail"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#define _UV2_FADE 1
				#pragma shader_feature _USE_DISSOLVE
				
				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_PBRBigMapUV2"}
		
		Pass
		{
			Name "CastShadow_ST_PBRBigMapUV2"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vertGPU
				#pragma fragment fragBigMapUV2
				#pragma multi_compile __ _GPUAnimation
				#pragma fragmentoption ARB_precision_hint_fastest

				#define _BIGMAPUV2 1
				#include "../Library/GPUSkinningLibrary.cginc"
				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Terrain"}
		
		Pass
		{
			Stencil
            {
                Ref 0
                Comp Equal
                Pass Keep
            }


			Name "CastShadow_ST_Terrain"
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
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_ShadowCaster"}
		
		Pass
		{
			Name "CastShadow_ST_ShadowCaster"
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
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_GPUSkinning"}
		
		Pass
		{
			Name "CastShadow_ST_GPUSkinning"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM
			
				#pragma multi_compile_instancing
				#pragma multi_compile __ _GPUAnimation
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "../Library/GPUSkinningLibrary.cginc"
				#include "CastShadowLib.cginc"

				#pragma vertex vertGPU
				#pragma fragment frag

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Monster_Lod2"}
		
		Pass
		{
			Name "CastShadow_ST_Monster_Lod2"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM
			
				#pragma multi_compile_instancing
				#pragma multi_compile __ _GPUAnimation
				#pragma fragmentoption ARB_precision_hint_fastest
				#define _MonsterLod2 1

				#include "../Library/GPUSkinningLibrary.cginc"
				#include "CastShadowLib.cginc"

				#pragma vertex vertGPU
				#pragma fragment frag

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_GPUSkinning_4Bone"}
		
		Pass
		{
			Name "CastShadow_ST_GPUSkinning_4Bone"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vertGPU
				#pragma fragment frag
				#pragma multi_compile __ _GPUAnimation
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#define _4BONES 1
				#include "../Library/GPUSkinningLibrary.cginc"
				#include "CastShadowLib.cginc"

			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Plant"}
		
		Pass
		{
			Name "CastShadow_ST_Plant"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex vertPlant
				#pragma fragment fragPlant
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "../Library/YoukiaTools.cginc"
				#include "CastShadowLib.cginc"

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

	SubShader
	{
		Tags { "RenderType" = "Opaque" "ShadowType" = "ST_Grass"}
		
		Pass
		{
			Name "CastShadow_ST_Tree"
			ZWrite On
			ZTest LEqual
			Cull back
			CGPROGRAM

				#define _GRASS 1
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

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue"="Geometry-1000" "ShadowType" = "ST_StencilPanel"}
		
		Pass
		{
			Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

			Name "CastShadow_ST_StencilPanel"
			ZWrite Off
			ZTest Off
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
}