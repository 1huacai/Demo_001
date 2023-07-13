// 

Shader "YoukiaEngine/Effect/YoukiaCloudMap"
{
	Properties
	{
		_Shift("Shift", Float) = 2
		_MainTexture("MainTexture", 2D) = "white" {}
		_MixTexture("MixTexture", 2D) = "white" {}
		_Speend("Speend", Range(0 , 1)) = 1		
		_BloomThreshold("BloomThreshold", Range(0 , 10)) = 5
		_BloomRange("BloomRange", Range(0 , 1)) = 1
		_Brightness("Brightness", Range(0 , 5)) = 0		
		_Saturation("Saturation", Range(1 , 5)) = 0
		_Contrast("Contrast", Range(1 , 5)) = 0	
		_CorrectionColor("CorrectionColor", Color) = (0.2125 ,0.7154,0.0721,0)

		[Space(30)]
		[Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10

	}

	SubShader	
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" "IgnoreProjector" = "True" }	
		Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
		ZWrite[_zWrite]
		ZTest[_zTest]
		Cull[_cull]	
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"		

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD;	
			};

			 sampler2D _MainTexture;
			 sampler2D _MixTexture;
			 float4 _MixTexture_ST;
			 float _Speend;
			 float _Shift;
			 float _BloomThreshold, _BloomRange,_Brightness, _Saturation, _Contrast;
			 float4 _CorrectionColor;

			v2f vert(appdata v)
			{
				v2f o;	
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MixTexture);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _MixTexture);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{	
				float TimeOffset = 0.5;
				float4 MixTexture = i.uv;
				float4 MixTex = tex2D(_MixTexture, i.uv.xy);
				float speend = (_Time.y * _Speend);
				float4 MixTexScale = lerp(MixTexture, MixTex, frac(speend));
				float4 MixTexOffset = lerp(MixTexture , MixTex, frac((speend + TimeOffset)));
				//��չ����������UV����
				float FlowAlpha = (sin(((UNITY_PI * _Shift * speend) - (UNITY_PI / _Shift))) * TimeOffset) + TimeOffset;
				float4 Blend = lerp(tex2D(_MainTexture, MixTexOffset.rg) , tex2D(_MainTexture, MixTexScale.rg) , FlowAlpha);
				//MainTex's UV= MIxflowmap,
				float3 BlendDecomposing = clamp(float3((_CorrectionColor.r * Blend.r), (_CorrectionColor.g * Blend.g), (_CorrectionColor.b * Blend.r)), float3( 0,0,0 ) , float3( 1,0,0 ));
				float4 Gray = lerp(float4(BlendDecomposing, 0.0), (_Brightness * Blend), _Saturation);
				float4 GrayColor = lerp(float4(float3(0.5, 0.5, 0.5), 0.0), Gray, _Contrast);
				// Color range control
				float Discolor = Luminance(Blend.rgb);
				// discoloration 
				float4 BaseColor = (Discolor).xxxx;
				float4 BloomRange = lerp(BaseColor, Blend, _BloomRange);

				float4 finalColor = GrayColor + (saturate(BloomRange * _BloomThreshold * Blend));
				return finalColor;
			}
			ENDCG
		}
	}
		Fallback "Transparent/VertexLit"
}