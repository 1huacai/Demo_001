Shader "YoukiaEngine/Effect/YoukiaFlowMap"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex("MainTex", 2D) = "white" {}

		[Header(FlowMap)]
		_FlowMap("FlowMap", 2D) = "gray" {}
		_FlowStrength("强度", Range(-1, 1)) = 1
		_Speed("流动速度", Range(-2, 2)) = 0.1

		[Header(Bloom)]
		_Opacity("Opacity", Range(0, 1)) = 1
		_BloomThreshold("BloomThreshold", Range(0, 10)) = 5
		_BloomRange("BloomRange", Range(-2, 20)) = 0

		[Space(5)]
		[MaterialToggle]_EnableAtomsphere("计算大气", float) = 0

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
		Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" "Reflection"="RenderReflectionTransparent"}	

		Blend[_SrcBlend][_DstBlend]
		ZWrite[_zWrite]
		ZTest[_zTest]
		Cull[_cull]

		Pass
		{		
			CGPROGRAM										
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "../Library/Atmosphere.cginc"
			#include "../Library/YoukiaEnvironment.cginc"
			#include "../Library/YoukiaEffect.cginc"

			#pragma multi_compile __ _HEIGHTFOG
			#pragma multi_compile __ _SKYENABLE

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;									
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;	

				float4 worldPos : TEXCOORD2;

				#ifdef _SKYENABLE
					float3 inscatter : TEXCOORD3;
					float3 extinction : TEXCOORD4;
				#endif
			};	

			half _Opacity;
			half _BloomThreshold, _BloomRange;
			half _EnableAtomsphere;

			v2f vert (appdata_t v)
			{
				v2f o;						
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex, o.worldPos);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _FlowMap);
				
				// atmosphere
				#ifdef _SKYENABLE
					UNITY_BRANCH
					if (_EnableAtomsphere > 0)
					{
						float3 extinction = 0;
						// float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
						o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos.xyz, extinction);
						o.extinction = extinction;
					}
				#endif

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// flowmap
				half2 flowmap = tex2D(_FlowMap, i.uv.zw).rg;
				half2 flowUV = (flowmap * 2 - 1) * _FlowStrength;

				float time = _Time.y * _Speed;
				float phase0 = frac(time * 0.5f + 0.5f);
                float phase1 = frac(time * 0.5f + 1.0f);

				half4 tex0 = tex2D(_MainTex, i.uv.xy - flowUV * phase0);
				half4 tex1 = tex2D(_MainTex, i.uv.xy - flowUV * phase1);

				float flowlerp = saturate(abs((0.5f - phase0) / 0.5f));
                half4 color = lerp(tex0, tex1, flowlerp);

				color.rgb *= _Color.rgb;
				half lum = Luminance(color.rgb);
				half BloomRange = lerp(lum, _BloomRange, lum);
				color = _Opacity * color + BloomRange * _BloomThreshold * color;

				
				#ifdef _SKYENABLE
					UNITY_BRANCH
					if (_EnableAtomsphere > 0)
					{
						color.rgb = color.rgb * i.extinction + i.inscatter;
						color.rgb = saturate(color.rgb);
					}
				#endif

				//height fog
				#ifdef _HEIGHTFOG
					half fog = 0;
					half3 heightfog = YoukiaHeightFog(i.worldPos, 0, fog);
					color.rgb = lerp(color.rgb, heightfog.rgb, fog);
				#endif


				return color;
			}
			ENDCG 
		}
	}	
	
	FallBack "VertexLit"	
}
