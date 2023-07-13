Shader "YoukiaEngine/Effect/Rain" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGBA)", 2D) = "black" {}
		_ColorStrength ("颜色强度", Range(0, 10)) = 1
		_BumpAmt ("扭曲度", Range(0, 1)) = 0.5

		[Header(Soft Particle)]
		[Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ZTest LEqual
		Cull Back
		Lighting Off
		Fog { Mode Off}

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
            	#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ _UNITY_RENDER
				#pragma shader_feature _USE_SOFTPARTICLE
				#include "UnityCG.cginc"
				#include "../Library/YoukiaEffect.cginc"

				struct appdataRain
				{
					half4 vertex : POSITION;
					fixed2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2fRain
				{
					half4 vertex : SV_POSITION;
					fixed2 uv : TEXCOORD0;
					//UNITY_FOG_COORDS(1)
					half4 projPos : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				half4 _TintColor;
				half _ColorStrength;
				half _BumpAmt;
				sampler2D _gGrabTex;

				v2fRain vert(appdataRain v)
				{
					v2fRain o;
					UNITY_INITIALIZE_OUTPUT(v2fRain, o);
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.vertex = UnityObjectToClipPos(v.vertex);

					o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
					// o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);

					o.projPos = ComputeScreenPos(o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);

					return o;
				}

				fixed4 frag(v2fRain i) : SV_Target
				{
					// sample the texture
					half4 tex = tex2D(_MainTex, i.uv.xy);
					half alpha = tex.a;

					fixed2 uvGrab = fixed2(i.projPos.xy / i.projPos.w) + alpha * _BumpAmt;
					fixed3 colGrab = tex2D(_gGrabTex, uvGrab).rgb;
					half4 color = half4(colGrab.rgb * tex.rgb, alpha) * _TintColor;
					color.rgb *= _ColorStrength;

					#if _USE_SOFTPARTICLE
						color.a *= SoftParticle(i.projPos);
					#endif
					
					return color;
				}
			ENDCG
		}
	}

	Fallback "Transparent/VertexLit"
}

