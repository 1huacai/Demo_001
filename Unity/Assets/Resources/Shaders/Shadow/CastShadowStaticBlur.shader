// ESM blur

Shader "Hidden/Shadow/CastShadowStaticBlur"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white"{}
	}

	CGINCLUDE
		#include "UnityCG.cginc" 
		#include "../Library/YoukiaTools.cginc"

		sampler2D_float _MainTex;
		half4 _MainTex_ST;
		float4 _MainTex_TexelSize;

		half _BlurOffset_0;
		half _BlurOffset_1;

		// guassian blur
		half _GaussianBlurSize;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 uv1 : TEXCOORD1;
			float4 uv2 : TEXCOORD2;	
			float4 uv3 : TEXCOORD3;	
			float4 pos : SV_POSITION;
		};

		// kawase blur
		float KawaseBlur(sampler2D tex, float2 uv, float2 texelSize, half pixelOffset)
		{
			// return 1;
			float c = tex2D(tex, uv).r;

			// half background = c.a;
			// o += DecodeFloatRGBA(c);
			// o += DecodeFloatRGBA(tex2D(tex, uv + float2(pixelOffset + 0.1, pixelOffset + 0.1) * texelSize));
			// o += DecodeFloatRGBA(tex2D(tex, uv + float2(-pixelOffset - 0.1, pixelOffset + 0.1) * texelSize));
			// o += DecodeFloatRGBA(tex2D(tex, uv + float2(-pixelOffset - 0.1, -pixelOffset - 0.1) * texelSize));
			// o += DecodeFloatRGBA(tex2D(tex, uv + float2(pixelOffset + 0.1, -pixelOffset - 0.1) * texelSize));
			// o *= 0.25f;
			// c = EncodeFloatRGBA(o);
			// c = background == 1 ? 1 : c;

			c += tex2D(tex, uv + float2(pixelOffset + 0.1, pixelOffset + 0.1) * texelSize).r;
			c += tex2D(tex, uv + float2(-pixelOffset - 0.1, pixelOffset + 0.1) * texelSize).r;
			c += tex2D(tex, uv + float2(-pixelOffset - 0.1, -pixelOffset - 0.1) * texelSize).r;
			c += tex2D(tex, uv + float2(pixelOffset + 0.1, -pixelOffset - 0.1) * texelSize).r;
			c *= 0.2f;

			return c;
		}

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);

			return o;
		}

		// gaussian blur
		v2f vertGaussian (appdata v, half4 blurSize)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			
			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					v.uv.y = 1 - v.uv.y;
			#endif

			o.uv = TRANSFORM_TEX(v.uv, _MainTex);

			o.uv1 = o.uv.xyxy + blurSize.xyxy * float4(1, 1, -1, -1) * _MainTex_TexelSize.xyxy;
			o.uv2 = o.uv.xyxy + blurSize.xyxy * float4(1, 1, -1, -1) * 2.0 * _MainTex_TexelSize.xyxy;
			// o.uv3 = o.uv.xyxy + blurSize * float4(1, 1, -1, -1) * 2.0 * _MainTex_TexelSize.xyxy;
			return o;
		}

		v2f vertH(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			half4 sizeHori = half4(_GaussianBlurSize, 0, _GaussianBlurSize, 0);
			o = vertGaussian(v, sizeHori);

			return o;
		}

		v2f vertV(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			half4 sizeVert = half4(0, _GaussianBlurSize, 0, _GaussianBlurSize);
			o = vertGaussian(v, sizeVert);

			return o;
		}

		float fragGaussian (v2f i, out float outDepth : SV_Depth) : SV_Target
		{
			// return _GaussianBlurSize;
			float weight[3] = {0.4026, 0.2442, 0.0545};

			outDepth = tex2D(_MainTex, i.uv).r * weight[0];

			outDepth += tex2D(_MainTex, i.uv1.xy).r * weight[1];
			outDepth += tex2D(_MainTex, i.uv1.zw).r * weight[1];

			outDepth += tex2D(_MainTex, i.uv2.xy).r * weight[2];
			outDepth += tex2D(_MainTex, i.uv2.zw).r * weight[2];

			// outDepth = tex2D(_MainTex, i.uv).r;
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(0, 1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(0, -1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(1, -1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(1, 0) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(1, 1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(-1, -1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(-1, 0) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth = max(outDepth, tex2D(_MainTex, i.uv + half2(-1, 1) * _GaussianBlurSize * _MainTex_TexelSize.xy).r);
			// outDepth /= 9;
			return 0;
			

			// sample the texture
			// outDepth = 0;

			// outDepth += 0.40 * tex2D(_MainTex, i.uv).r;
			// outDepth += 0.15 * tex2D(_MainTex, i.uv1.xy).r;
			// outDepth += 0.15 * tex2D(_MainTex, i.uv1.zw).r;
			// outDepth += 0.10 * tex2D(_MainTex, i.uv2.xy).r;
			// outDepth += 0.10 * tex2D(_MainTex, i.uv2.zw).r;
			// outDepth += 0.05 * tex2D(_MainTex, i.uv3.xy).r;
			// outDepth += 0.05 * tex2D(_MainTex, i.uv3.zw).r;

			// return 0;
		}
	ENDCG

	SubShader
	{
		Cull Off ZWrite On ZTest Always

		Pass
		{
			name "KawaseBlur 0"
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				half4 frag(v2f i, out float outDepth : SV_Depth) : SV_TARGET
				{
					outDepth = KawaseBlur(_MainTex, i.uv, _MainTex_TexelSize.xy, _BlurOffset_0);
					return 0;
				}

			ENDCG
		}

		Pass
		{
			name "KawaseBlur 1"
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				half4 frag(v2f i, out float outDepth : SV_Depth) : SV_TARGET
				{
					outDepth = KawaseBlur(_MainTex, i.uv, _MainTex_TexelSize.xy, _BlurOffset_1);
					return 0;
				}

			ENDCG
		}

		Pass
		{
			name "Gaussian Horiz"
			CGPROGRAM

				#pragma vertex vertH
				#pragma fragment fragGaussian
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		Pass
		{
			name "Gaussian Vert"
			CGPROGRAM

				#pragma vertex vertV
				#pragma fragment fragGaussian
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}
	}
}