Shader "Hidden/ImageEffect/SimpleBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		CGINCLUDE
			#include "UnityCG.cginc"
			#include "../Library/YoukiaBlur.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST, _MainTex_TexelSize;
			// x: simple blur radius, y: gaussian blur radius, z: blur noise scale, w: blur noise strength
			half4 _SimpleBlurParam;

			v2fBox vertSimpleBlur(appdata v)
			{
				v2fBox o;
				o = vertBoxBlur(v, _SimpleBlurParam.x, _MainTex_TexelSize, _MainTex_ST, _MainTex);
				return o;
			}

			half4 fragSimpleBlur(v2fBox i) : SV_Target
			{
				return fragBoxBlur(i, _MainTex);
			}
		ENDCG

		Pass
		{
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vertSimpleBlur
			#pragma fragment fragSimpleBlur
			#pragma target 3.0

			ENDCG
		}

	}
}
