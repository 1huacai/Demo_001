Shader "Hidden/ImageEffect/GaussianBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE
			#include "UnityCG.cginc"
			#include "../Library/YoukiaBlur.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST, _MainTex_TexelSize;

			// x: hori, y: vert
			half4 _GaussianParam;
			
			v2fGaussian vertH(appdata v)
			{
				v2fGaussian o;
				half4 sizeHori = half4(_GaussianParam.x, 0, _GaussianParam.x, 0);
				o = vertGaussian(v, sizeHori, _MainTex_TexelSize, _MainTex_ST, _MainTex);

				return o;
			}

			v2fGaussian vertV(appdata v)
			{
				v2fGaussian o;
				half4 sizeVert = half4(0, _GaussianParam.y, 0, _GaussianParam.y);
				o = vertGaussian(v, sizeVert, _MainTex_TexelSize, _MainTex_ST, _MainTex);

				return o;
			}

			half4 frag (v2fGaussian i) : SV_Target
			{
				return fragGaussian(i, _MainTex);
			}


		ENDCG

		Pass
		{
			ZWrite Off
			Cull Off
			Name "horizontal"

			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma vertex vertH
				#pragma fragment frag
				
			ENDCG
		}

		Pass
		{
			ZWrite Off
			Cull Off
			Name "vertical"

			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma vertex vertV
				#pragma fragment frag
				
			ENDCG
		}
	}
}
