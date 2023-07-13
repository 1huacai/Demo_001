Shader "Hidden/ImageEffect/TailEffect"
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
			#include "YoukiaImageEffect.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST, _MainTex_TexelSize;

			sampler2D _gTailRenderTex;
			sampler2D _gTailTex;

			// x: hori, y: vert, zw: camera pos offset
			half4 _gTailRenderParam;

			#define TailBlurScale _gTailRenderParam.x * 2
			#define TailDisappear _gTailRenderParam.y
			#define TailCameraOffset _gTailRenderParam.zw

			v2fBox vertSimpleBlur(appdata v)
			{
				v2fBox o;
				o.pos = UnityObjectToClipPos(v.vertex);

				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
					v.uv.y = 1 - v.uv.y;
				#endif

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				BoxBlurUV(o.uv + TailCameraOffset, o.uv1, o.uv2, _MainTex_TexelSize.xy * TailBlurScale.xx);

				return o;
			}

			v2fImg vert(appdataImg v)
			{
				return vertImg(v, _MainTex_TexelSize, _MainTex_ST, _MainTex);
			}

			half4 fragSimpleBlurTail(v2fBox i) : SV_Target
			{	
				half2 tailValue = tex2D(_MainTex, i.uv + TailCameraOffset).rg;
				half tailRender = tex2D(_gTailRenderTex, i.uv).r;
				half boxBlur = fragBoxBlurAround(i, _MainTex);

				half tail = tailRender;
				tail += -(tailValue.y - 0.5) * 2 + boxBlur;
				tail *= TailDisappear;
				tail = tail * 0.5 + 0.5;

				return half4(saturate(tail), tailValue.x, 0, 0);
			}

			half4 frag(v2fImg i) : SV_Target
			{
				return 0.5.xxxx;
			}

		ENDCG

		Pass
		{
			ZWrite Off
			Cull Off
			Name "tail effect"

			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma vertex vertSimpleBlur
				#pragma fragment fragSimpleBlurTail
				
			ENDCG
		}

		Pass
		{
			ZWrite Off
			Cull Off
			Name "tail init"

			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma vertex vert
				#pragma fragment frag
				
			ENDCG
		}
	}
}
