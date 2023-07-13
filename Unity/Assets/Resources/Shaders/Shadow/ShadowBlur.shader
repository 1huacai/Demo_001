Shader "Hidden/Shadow/ShadowBlur"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white"{}
	}

	SubShader
	{
		pass
		{
			ZWrite Off
			ZTest Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			#include "../Library/ShadowLibrary.cginc"

			float4 _MainTex_TexelSize;
			float _gShadowBlurRadius;
			float _gShadowBlueNoiseOffset, _gShadowBlueNoiseStrength;
			
			// bloom
			uniform sampler2D _PreBloomTex;

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
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				float2 poissonDisk[4] = 
				{
					float2( -0.94201624, -0.39906216 ),
					float2( 0.94558609, -0.76890725 ),
					float2( -0.094184101, -0.92938870 ),
					float2( 0.34495938, 0.29387760 )
				};

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// o.uv1.xy = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * float2(1, 1);
				// o.uv1.zw = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * float2(-1, 1);
				// o.uv2.xy = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * float2(-1, -1);
				// o.uv2.zw = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * float2(1, -1);

				o.uv1.xy = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * poissonDisk[0];
				o.uv1.zw = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * poissonDisk[1];
				o.uv2.xy = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * poissonDisk[2];
				o.uv2.zw = v.uv.xy + _gShadowBlurRadius * _MainTex_TexelSize * poissonDisk[3];

				return o;
			}

			fixed4 frag(v2f i):SV_TARGET
			{
				fixed4 col = 0;

				half blueNoise = tex2D(_gBlueNoise, i.uv).r * _gShadowBlueNoiseStrength;
				blueNoise -= _gShadowBlueNoiseOffset;
				// return blueNoise;

				col += tex2D(_MainTex, i.uv + blueNoise);
				col += tex2D(_MainTex, i.uv1.xy + blueNoise);
				col += tex2D(_MainTex, i.uv1.zw + blueNoise);
				col += tex2D(_MainTex, i.uv2.xy + blueNoise);
				col += tex2D(_MainTex, i.uv2.zw + blueNoise);
				col *= 0.2f;

				// bloom
				// half4 bloom = tex2D(_PreBloomTex, i.uv);
				// col.rgb *= bloom;

				return col;
			}

			ENDCG
		}
	}
}