﻿Shader "Hidden/Shadow/ScreenSpaceShadow"
{
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "../Library/ShadowLibrary.cginc"
		#include "../Library/YoukiaEnvironment.cginc"
		#include "../Library/GPUSkinningLibrary.cginc"
		#include "../Library/YoukiaEffect.cginc"
		
		const static half FIDEDIS = 5.0f;
		// sampler2D_float _SSSDepthTexture;
		float4x4 _SSSInverseVP;
		float4x4 _SSSVP;

		struct appdata
		{
			float4 vertex : POSITION;
			half4 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
		};

		inline float4 WorldPos(half2 uv)
		{
			// wspos
			float zdepth = SAMPLE_DEPTH_TEXTURE(_SSSDepthTexture, uv);
			
			#if defined(UNITY_REVERSED_Z)
			#else
				//(-1, 1)-->(0, 1)
				zdepth = zdepth * 2 - 1; 
			#endif

			float4 clipPos = float4(uv * 2 - 1, zdepth, 1.0);

			float4 wpos = mul(_SSSInverseVP, clipPos);
			wpos /= wpos.w;

			return wpos;
		}

		half4 ShadowAtten(half4 shadow, float3 wpos)
		{
			float dis = distance(wpos, _WorldSpaceCameraPos);
			half fade = max(dis - (_shadowDistance - FIDEDIS), 0);
			fade = fade * fade / FIDEDIS;
			fade = 1 - saturate(fade);

			return lerp(1, shadow, fade);
		}
		
		v2f vert(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv.xy = v.texcoord;
			o.uv.zw = ComputeNonStereoScreenPos(o.pos);

			return o;
		}

		fixed4 fragCSM(v2f i) : SV_TARGET
		{
			// wspos
			float4 wpos = WorldPos(i.uv.xy);
			float4 vpos = mul(_SSSVP, wpos);

			fixed4 weights = GetCascadeWeights(vpos.w);
			// return weights.yyyy;
			// weights = half4(0, 1, 0, 0);
			float4 shadowCoord = GetShadowCoord(wpos, weights);

			float depth = GetShadowDepth(shadowCoord);
			int index = weights[0] * 0 + weights[1] * 1 + weights[2] * 2 + weights[3] * 3;
			
			half shadow = SampleCSM(index, shadowCoord, depth);
			
			// lerp
			// csm 第一级第二级过渡
			#if _SHADOW_CSMLERP
				float lerpDis = saturate(_gLightSplitsFar[index] - vpos.w);
				half flg = (1 - lerpDis) * (MAX_SPLIT - 1 - index);
				UNITY_BRANCH
				if (flg > 0)
				{
					int nextIndex = index + 1;
					shadowCoord = GetShadowCoord(wpos, constWeights[nextIndex]);
					depth = GetShadowDepth(shadowCoord);
					half shadowLerp = SampleCSM(nextIndex, shadowCoord, depth);
					// return lerpDis;
					shadow = lerp(shadowLerp, shadow, lerpDis);
				}
			#endif

			shadow = ShadowAtten(shadow, wpos);

			// clouds speed
			#if _CLOUDSSHADOW
				shadow *= CloudsShadow(wpos);
			#endif

			return shadow;
		}

		fixed4 fragStaticDynamic(v2f i) : SV_TARGET
		{
			// wspos
			float4 wpos = WorldPos(i.uv.xy);

			float4 shadowCoordDynamic = GetShadowCoordDynamic(wpos);
			float4 shadowCoordStatic = GetShadowCoordStatic(wpos);

			float depthDynamic = GetShadowDepth(shadowCoordDynamic);
			float depthStatic = GetShadowDepth(shadowCoordStatic);


			half shadowDynamic = PCFSample(_gSMDynamic, shadowCoordDynamic, _gSMDynamic_TexelSize, depthDynamic, _shadowBlurSize);
			shadowDynamic = ShadowAttenuate(shadowCoordDynamic, shadowDynamic);
			// half shadowStatic = ESMSample(_gSMStatic, shadowCoordStatic, depthStatic, bias);
			half shadowStatic = PCFSample(_gSMStatic, shadowCoordStatic,_gSMStatic_TexelSize, depthStatic, _shadowBlurSize);
			shadowStatic = ShadowAttenuate(shadowCoordStatic, shadowStatic);

			half shadow = min(shadowDynamic, shadowStatic);

			shadow = ShadowAtten(shadow, wpos);

			// clouds speed
			#if _CLOUDSSHADOW
				shadow *= CloudsShadow(wpos);
			#endif

			return shadow;
		}

	ENDCG

	SubShader
	{
		ZWrite Off ZTest Always Cull Off

		Pass
		{
			Name "ScreenSpaceShadow CSM"
			
			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile __ _SHADOW_BLUR
				#pragma multi_compile __ _YOUKIA_REVERSED_Z
				#pragma multi_compile __ _CLOUDSSHADOW
				#pragma multi_compile __ _SHADOW_CSMLERP

				#pragma vertex vert
				#pragma fragment fragCSM

			ENDCG
		}

		Pass
		{
			Name "ScreenSpaceShadow Static Dynamic"
			
			CGPROGRAM
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile __ _SHADOW_BLUR
				#pragma multi_compile __ _YOUKIA_REVERSED_Z
				#pragma multi_compile __ _CLOUDSSHADOW

				#pragma vertex vert
				#pragma fragment fragStaticDynamic

			ENDCG
		}
	}

	Fallback Off
}